using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WEBAPI.Models;
using WEBAPI.Services.Bannerbear;
using WEBAPI.Services.Solana;
namespace WEBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly IBannerbearService _bannerbear;
        private readonly LuanvantienganhContext _db;
        private readonly IConfiguration _config;

        public CertificatesController(IBannerbearService bannerbear, LuanvantienganhContext db, IConfiguration config)
        {
            _bannerbear = bannerbear;
            _db = db;
            _config = config;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("CreateCertificates")]
        public async Task<IActionResult> GenerateUrl([FromBody] BannerRequest req)
        {


            //===================Khởi tạo ví Solana=========================    
            // Đọc đường dẫn từ appsettings.json
            string keypairPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                _config["Wallet:KeypairPath"]
            );

            var keypairArray = JsonSerializer.Deserialize<int[]>(System.IO.File.ReadAllText(keypairPath));

            // Convert the array of integers to a byte array
            var keypairBytes = keypairArray.Select(i => (byte)i).ToArray();

            //khởi tạo ví từ mảng byte
            var wallet = new Wallet(keypairBytes);

            // In ra địa chỉ ví
            Console.WriteLine($"Wallet Address public key: {wallet.Account.PublicKey}");

            
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            // In ra số dư của ví 
            var balance = rpcClient.GetBalance(wallet.Account.PublicKey);
            Console.WriteLine($"Số dư: {balance.Result.Value} lamports");
            //==================================================================================


            // 1) Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            // Kiểm tra chứng chỉ đã tồn tại cho user và khóa học này chưa
            var existingCertificate = await _db.Certificates
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == req.CourseId);

            if (existingCertificate != null)
            {
                // Nếu ngày hết hạn là null => vĩnh viễn
                if (existingCertificate.ExpirationDate == null)
                {
                    return BadRequest("Bạn đã có chứng chỉ này và có thời hạn vĩnh viễn.");
                }
                // Nếu chưa hết hạn
                if (existingCertificate.ExpirationDate >= DateTime.UtcNow)
                {
                    return BadRequest($"Chứng chỉ của bạn đã tồn tại và còn thời hạn đến {existingCertificate.ExpirationDate:dd/MM/yyyy}.");
                }
                // Nếu đã hết hạn thì cho phép tạo mới (không trả về, tiếp tục xử lý)
            }


            // 2) Truy vấn tên học viên từ UserId
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy thông tin học viên.");


            // 3) Truy vấn tên khóa học từ CourseId
            var course = await _db.Courses.FindAsync(req.CourseId);
            if (course == null)
                return NotFound("Không tìm thấy thông tin khóa học.");


            // 3) Gán tên học viên vào BannerRequest
            req.StudentName = user.Fullname;
            req.Subtitle = $"Chúc mừng bạn đã hoàn thành khóa học {course.CourseName}";
            req.CreatedAt = DateTime.UtcNow;
            req.Signature = "LK learing";


            // 5) Tính ngày hết hạn dựa vào CertificateDurationDays
            if (course.CertificateDurationDays.HasValue && course.CertificateDurationDays.Value > 0)
            {
                req.ExpirationDate = req.CreatedAt.AddDays(course.CertificateDurationDays.Value);
            }
            else
            {
                req.ExpirationDate = null; // Vĩnh Viễn
            }


            // Sinh mã và ngày
            req.VerificationCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
           
            req.UserId = userId; // Gán userId từ token vào request
            

            // Gọi service, nhận URL ảnh
            var imageUrl = await _bannerbear.GenerateCertificateImageUrlAsync(req);



            //===================Tạo Transaction=========================
            //sau khi đã có đầy đủ thông tin ta truyển nội dung vào memo của 1 transaction
            // Tạo object chứa nhiều trường
            if (imageUrl!=null) {
                var memo = new
                {
                    userid = userId,
                    courseid = req.CourseId,
                    name = req.StudentName,
                    imageurl = imageUrl,
                    createdAt = req.CreatedAt,
                    expirationDate = req.ExpirationDate

                };

                // Chuyển object thành chuỗi JSON
                string memoText = JsonSerializer.Serialize(memo);
                // Tạo instruction cho Memo Program
                var memoInstruction = MemoProgram.NewMemo(wallet.Account.PublicKey, memoText);


                // Tạo transaction
                var blockHash = rpcClient.GetLatestBlockHash();
                if (blockHash.Result == null)
                {
                    Console.WriteLine($"Lỗi lấy block hash: {blockHash.Reason}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Không thể lấy block hash.");
                }

                var tx = new TransactionBuilder()
                    .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                    .SetFeePayer(wallet.Account)
                    .AddInstruction(memoInstruction)
                    .Build(wallet.Account);

                // Gửi transaction
                var sendTx = rpcClient.SendTransaction(tx);
                Console.WriteLine($"Transaction signature: {sendTx.Result}");
                // sendTx.Result là chữ ký giao dịch
                var signature = sendTx.Result;
                // Tạo đối tượng Certificate để lưu vào DB
                var certificate = new Certificate
                {
                    UserId = req.UserId,
                    CourseId = req.CourseId,
                    VerificationCode = req.VerificationCode,
                    ImageUrl = imageUrl,
                    Signature = signature,
                    CreatedAt = req.CreatedAt,
                    ExpirationDate = req.ExpirationDate // hoặc set theo logic của bạn
                };

                _db.Certificates.Add(certificate);
                await _db.SaveChangesAsync();
                return Ok(new { imageUrl, certificateId = certificate.CertificateId });

            }

            else
                return BadRequest("Ảnh chứng chỉ không tồn tại không thể ký chữ ký giao dịch.");


            //========================================================================================


            return BadRequest("Không thể tạo chứng chỉ. Vui lòng thử lại sau.");

        }


        [AllowAnonymous]
        [HttpPost("VerifyCertificate")]
        public async Task<IActionResult> VerifyCertificate([FromBody] VerifyRequest request)
        {
            // 1) Tra DB theo verify code
            var certificate = await _db.Certificates
                .FirstOrDefaultAsync(c => c.VerificationCode == request.VerifyCode);

            if (certificate == null)
                return BadRequest("Verify code bạn không khả dụng.");

            // 2) Khởi tạo Solana RPC client DevNet
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

            // 3) Lấy transaction theo signature
            var txResult = rpcClient.GetTransaction(certificate.Signature);
            if (txResult.Result == null)
                return BadRequest("Chứng chỉ chưa được xác nhận hoặc không tồn tại. Có thể là chứng chỉ giả !!!!!");

            // 4) Giải mã memo
            string memoText = null;
            foreach (var instr in txResult.Result.Transaction.Message.Instructions)
            {
                try
                {
                    var bytes = Encoders.Base58.DecodeData(instr.Data);
                    memoText = Encoding.UTF8.GetString(bytes);
                    break;
                }
                catch { }
            }
            if (memoText == null)
                return BadRequest("Không thể giải mã nội dung memo. Chứng chỉ giả.");

            // 5) Parse JSON memo thành object
            var memoData = JsonSerializer.Deserialize<MemoData>(memoText);

            // 6) So khớp các trường, chỉ đến giây
            bool isMatch =
                memoData.UserId == certificate.UserId &&
                memoData.CourseId == certificate.CourseId &&
                memoData.ImageUrl == certificate.ImageUrl &&
                IsSameSecond(memoData.CreatedAt, certificate.CreatedAt) &&
                (
                    memoData.ExpirationDate.HasValue && certificate.ExpirationDate.HasValue
                        ? IsSameSecond(memoData.ExpirationDate.Value, certificate.ExpirationDate.Value)
                        : memoData.ExpirationDate == certificate.ExpirationDate
                );

            if (!isMatch)
                return BadRequest("Đây là Chứng Chỉ giả");

            // 7) Kiểm tra thời hạn và build message
            if (certificate.ExpirationDate.HasValue && certificate.ExpirationDate.Value < DateTime.UtcNow)
            {
                return Ok(new
                {
                    Message = "Chứng chỉ đã hết hạn. Đây là chứng chỉ thật được cấp bởi LK learning !",
                    ImageUrl = memoData.ImageUrl
                });
            }

            string validityMessage = certificate.ExpirationDate == null
                ? "Vĩnh viễn"
                : $"Còn thời hạn: {(certificate.ExpirationDate.Value.Date - DateTime.UtcNow.Date).Days} ngày";

            // 8) Trả về kết quả thành công
            return Ok(new
            {
                Message = $"Đây là chứng chỉ thật được cấp bởi LK learning ! {validityMessage}",
                Expiration = validityMessage,
                ImageUrl = memoData.ImageUrl
            });
        }

        // So sánh chính xác đến giây
        private static bool IsSameSecond(DateTime a, DateTime b)
        {
            return a.Year == b.Year
                && a.Month == b.Month
                && a.Day == b.Day
                && a.Hour == b.Hour
                && a.Minute == b.Minute
                && a.Second == b.Second;
        }



    }
}
