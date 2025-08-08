using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WEBAPI.DTOS;
using WEBAPI.Models;
using WEBAPI.Services.Models;
using WEBAPI.Services.ModelsVnPay;
using WEBAPI.Services.VnPay;
using WEBAPI.Services.ZaloPay;

namespace WEBAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {

        //zalo pay
        private readonly ZaloPayService _zaloPayService;
        private readonly LuanvantienganhContext _dbContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentController> _logger;


        //vnpay
        private readonly IVnPayService _vnPayService;

        public PaymentController(ZaloPayService zaloPayService, LuanvantienganhContext dbContext, IConfiguration config, ILogger<PaymentController> logger, IVnPayService vnPayService)
        {
            _zaloPayService = zaloPayService;
            _dbContext = dbContext;
            _config = config;
            _httpClient = new HttpClient();
            _logger = logger;
            _vnPayService = vnPayService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("TotalRevenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                // Calculate total revenue from orders with Status = "Success"
                var totalRevenue = await _dbContext.Orders
                    .Where(o => o.Status == "Success")
                    .SumAsync(o => o.Amount);

                _logger.LogInformation($"Total revenue calculated: {totalRevenue}");

                return Ok(new { totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating total revenue: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Error calculating total revenue", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetListOrders")]
        public async Task<IActionResult> GetListOrders()
        {
            var orders = await _dbContext.Orders.ToListAsync();

            // Map từ entity sang DTO 
            var ordersDTO =
                _dbContext.Orders.Include(c => c.PaymentMethod)
                .Include(c=>c.User).Include(c=>c.Package)
                .Select(c => new OrderDTO
                
                {
                    OrderId = c.OrderId,
                    TransactionId = c.TransactionId,
                    Status = c.Status,
                    UserId = c.UserId,
                    FullName=c.User.Fullname ??"N/A",
                    PackageId = c.PackageId,
                    PackageName = c.Package.PackageName, // Thêm PackageName vào DTO
                    CreatedAt = c.CreatedAt,
                    PaymentMethodId = c.PaymentMethodId,
                    Amount = c.Amount, // Thêm Amount vào DTO
                    Name = c.PaymentMethod.Name
                }).ToList();

            return Ok(ordersDTO);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("mytransactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            // 1) Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            // 2) Lấy giao dịch của user cùng thông tin gateway
            var list = await _dbContext.Orders
                .Where(tx => tx.UserId == userId)
                .Include(tx => tx.PaymentMethod) // Lấy thông tin PaymentMethod
                .OrderByDescending(tx => tx.OrderId)
                .Select(tx => new OrderDTO
                {
                    OrderId = tx.OrderId,
                    TransactionId = tx.TransactionId,
                    UserId = tx.UserId,
                    PackageId = tx.PackageId,
                    Amount = tx.Amount,
                    Status = tx.Status,
                    CreatedAt = tx.CreatedAt,
                    PaymentMethodId = tx.PaymentMethodId,
                    Name = tx.PaymentMethod.Name // Lấy description từ bảng GatewayPayment
                })
                .ToListAsync();

            return Ok(list);
        }
        // GetDSOrderByUserId
        [Authorize(Roles ="User,Admin")]
        [HttpGet("GetDSOrderByUserId")]
        public async Task<IActionResult> GetDSOrderByUserId()
        {
            // 1) Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            // 2) Lấy giao dịch của user cùng thông tin gateway
            var list = await _dbContext.Orders
                .Where(tx => tx.UserId == userId)
                .Include(tx => tx.PaymentMethod) // Lấy thông tin PaymentMethod
                .OrderByDescending(tx => tx.OrderId)
                .Select(tx => new OrderDTO
                {
                    OrderId = tx.OrderId,
                    TransactionId = tx.TransactionId,
                    UserId = tx.UserId,
                    FullName = tx.User.Fullname,
                    PackageId = tx.PackageId,
                    PackageName = tx.Package.PackageName,
                    Amount = tx.Amount,
                    Status = tx.Status,
                    CreatedAt = tx.CreatedAt,
                    PaymentMethodId = tx.PaymentMethodId,
                    Name = tx.PaymentMethod.Name // Lấy description từ bảng GatewayPayment
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("GetListPaymentMethods")]
        public async Task<IActionResult> getListPaymentMethos()
        {
            var paymentMethods = await _dbContext.PaymentMethods.ToListAsync();

            // Map từ entity sang DTO 
            var paymentMethodsDTO = paymentMethods.Select(c => new PaymentMethodDTO
            {
                PaymentMethodId = c.PaymentMethodId,
                Name = c.Name,
                Logo = c.Logo

            }).ToList();

            return Ok(paymentMethodsDTO);
        }
        // Lấy chi tiết phương thức thanh toán theo id
        [HttpGet("GetPaymentMethodById/{id}")]
        public async Task<IActionResult> GetPaymentMethodById(int id)
        {
            var paymentMethod = await _dbContext.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
                return NotFound("Không tìm thấy phương thức thanh toán");

            var dto = new PaymentMethodDTO
            {
                PaymentMethodId = paymentMethod.PaymentMethodId,
                Name = paymentMethod.Name,
                Logo = paymentMethod.Logo
            };
            return Ok(dto);
        }

        // Thêm mới phương thức thanh toán
        [Authorize(Roles = "Admin")]
        [HttpPost("AddPaymentMethod")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] PaymentMethodDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Dữ liệu không hợp lệ");

            var paymentMethod = new PaymentMethod
            {
                Name = dto.Name,
                Logo = dto.Logo
            };

            _dbContext.PaymentMethods.Add(paymentMethod);
            await _dbContext.SaveChangesAsync();

            dto.PaymentMethodId = paymentMethod.PaymentMethodId;
            return Ok(dto);
        }

        // Sửa phương thức thanh toán
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdatePaymentMethod/{id}")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] PaymentMethodDTO dto)
        {
            var paymentMethod = await _dbContext.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
                return NotFound("Không tìm thấy phương thức thanh toán");

            paymentMethod.Name = dto.Name;
            paymentMethod.Logo = dto.Logo;

            await _dbContext.SaveChangesAsync();
            return Ok("Cập nhật phương thức thanh toán thành công");
        }

        // Xóa phương thức thanh toán
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeletePaymentMethod/{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            var paymentMethod = await _dbContext.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
                return NotFound("Không tìm thấy phương thức thanh toán");

            _dbContext.PaymentMethods.Remove(paymentMethod);
            await _dbContext.SaveChangesAsync();
            return Ok("Xóa phương thức thanh toán thành công");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteOrders/{id}")]
        public async Task<IActionResult> DeleteOrders(string id)
        {
            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound("Không tìm thấy Orders");
            }


            try
            {
                _dbContext.Orders.Remove(order);
                await _dbContext.SaveChangesAsync();
                return Ok("Xóa Orders thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa Orders");
            }
        }

        //Create Zalopay

        [HttpPost("create-zalopay")]
        public async Task<IActionResult> CreateZaloPay([FromBody] ZaloPayRequest request)
        {
            try
            {
                // Kiểm tra đầu vào
                if (string.IsNullOrEmpty(request.OrderId) || request.UserId <= 0 || request.PackageId <= 0)
                {
                    _logger.LogError("OrderId, UserId hoặc PackageId không hợp lệ");
                    return BadRequest(new { Error = "OrderId, UserId và PackageId là bắt buộc" });
                }

                string paymentUrl = await _zaloPayService.CreatePayment(request);
                return Ok(new { PaymentUrl = paymentUrl, OrderId = request.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tạo thanh toán ZaloPay: {ex.Message}");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        //callback zalo
        [HttpPost("zalopay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ZaloPayCallback()
        {
            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var rawBody = await reader.ReadToEndAsync();
                _logger.LogInformation($"Dữ liệu callback từ ZaloPay: {rawBody}");

                var callback = JsonConvert.DeserializeObject<ZaloPayCallback>(rawBody);
                if (callback == null)
                {
                    _logger.LogError("Không thể deserialize dữ liệu callback");
                    return BadRequest(new { Error = "Dữ liệu callback không hợp lệ" });
                }

                var callbackData = JsonConvert.DeserializeObject<ZaloPayCallbackData>(callback.Data);
                if (callbackData == null)
                {
                    _logger.LogError("Không thể deserialize trường data");
                    return BadRequest(new { Error = "Dữ liệu giao dịch không hợp lệ" });
                }

                if (!_zaloPayService.VerifyCallback(callback))
                {
                    _logger.LogError($"Chữ ký không hợp lệ. Callback: {rawBody}");
                    return BadRequest(new { Error = "Chữ ký không hợp lệ" });
                }

                var order = await _dbContext.Orders
                    .FirstOrDefaultAsync(p => p.TransactionId == callbackData.AppTransId);
                if (order == null)
                {
                    _logger.LogError($"Không tìm thấy giao dịch với AppTransId: {callbackData.AppTransId}");
                    return BadRequest(new { Error = "Không tìm thấy giao dịch" });
                }

                // Cập nhật trạng thái dựa trên status của callback
                if (callbackData.Status == 1)
                {
                    order.Status = "Success";
                    order.TransactionId = callbackData.ZpTransId;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Cập nhật trạng thái đơn hàng {order.OrderId} thành Success");
                }
                else if (callbackData.Status == 0)
                {
                    order.Status = "Pending";
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Giao dịch {order.OrderId} đang ở trạng thái Pending");
                }
                else
                {
                    order.Status = "Failed";
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Giao dịch {order.OrderId} thất bại");
                }

                return Ok(new
                {
                    return_code = 1,
                    return_message = "success",
                    OrderId = order.OrderId,
                    TransactionId = callbackData.ZpTransId,
                    TransactionStatus = order.Status,
                    Message = "Callback processed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xử lý callback: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Ok(new { return_code = 0, return_message = ex.Message });
            }
        }
        [HttpPost("update-order-status")]        
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDTO dto)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.TransactionId == dto.AppTransId);
            if (order == null)
                return NotFound();

            order.Status = dto.Status;
            await _dbContext.SaveChangesAsync();
            return Ok(new { Success = true });
        }
        [HttpGet("zalopay/verify")]
        public async Task<IActionResult> VerifyPayment(string orderId)
        {
            try
            {
                var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogError($"Không tìm thấy đơn hàng với OrderId: {orderId}");
                    return NotFound(new { Error = "Không tìm thấy đơn hàng" });
                }

                if (order.Status == "Success")
                {
                    return Ok(new ZaloPayVerifyResponse
                    {
                        IsSuccess = true,
                        Message = "Giao dịch thành công"
                    });
                }

                // Kiểm tra trạng thái giao dịch với ZaloPay nếu đang Pending
                if (order.Status == "Pending")
                {
                    var appId = _config["ZaloPayConfig:AppId"];
                    var key1 = _config["ZaloPayConfig:Key1"];
                    var queryEndpoint = "https://sb-openapi.zalopay.vn/v2/query";

                    var data = new Dictionary<string, string>
                    {
                        { "app_id", appId },
                        { "app_trans_id", order.TransactionId }
                    };

                    var dataToSign = $"{data["app_id"]}|{data["app_trans_id"]}|{key1}";
                    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key1)))
                    {
                        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                        data["mac"] = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }

                    var response = await _httpClient.PostAsync(queryEndpoint, new FormUrlEncodedContent(data));
                    var responseString = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Phản hồi từ ZaloPay Query: {responseString}");

                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    if (result.return_code == 1)
                    {
                        order.Status = "Success";
                        order.TransactionId = result.zp_trans_id?.ToString() ?? order.TransactionId;
                        await _dbContext.SaveChangesAsync();
                        // Đăng ký hoặc gia hạn gói cho user khi verify thành công (giống VnPay callback)
                        try
                        {
                            _logger.LogInformation($"Bắt đầu đăng ký/gia hạn gói {order.PackageId} cho user {order.UserId}");

                            var regDate = DateTime.Now;
                            await RegisterOrRenewAsync(order.UserId, order.PackageId, regDate);

                            _logger.LogInformation($"Hoàn thành đăng ký/gia hạn gói {order.PackageId} cho user {order.UserId}");
                        }
                        catch (Exception regEx)
                        {
                            _logger.LogError($"Lỗi khi đăng ký/gia hạn gói trong verify: {regEx.Message}\nStackTrace: {regEx.StackTrace}");
                            // Không return error ở đây để vẫn trả về kết quả verify thành công
                        }

                        return Ok(new ZaloPayVerifyResponse
                        {
                            IsSuccess = true,
                            Message = "Giao dịch thành công"
                        });
                    }
                    else if (result.return_code == 3)
                    {
                        // Giao dịch vẫn đang xử lý, không cập nhật trạng thái
                        return Ok(new ZaloPayVerifyResponse
                        {
                            IsSuccess = false,
                            Message = "Giao dịch đang chờ xử lý"
                        });
                    }
                    else
                    {
                        order.Status = "Failed";
                        await _dbContext.SaveChangesAsync();
                        return Ok(new ZaloPayVerifyResponse
                        {
                            IsSuccess = false,
                            Message = result.sub_return_message?.ToString() ?? "Giao dịch thất bại"
                        });
                    }
                }

                return Ok(new ZaloPayVerifyResponse
                {
                    IsSuccess = false,
                    Message = "Giao dịch thất bại"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xác minh thanh toán: {ex.Message}");
                return StatusCode(500, new { Error = "Lỗi server khi xác minh thanh toán", Details = ex.Message });
            }
        }




        //VNPAY//////////////////
        [Authorize(Roles = "User,Admin")]
        [HttpPost("create-vnpay")]
        public async Task<IActionResult> CreatePaymentAsync([FromBody] PaymentInformationModel model, [FromQuery] int packageId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Lấy userId
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(uid, out var userId))
                return Unauthorized("Không lấy được thông tin user.");

            // Block **chỉ** khi user đã có gói vĩnh viễn
            bool hasPermanent = await _dbContext.UserPackageRegistrations
                .AnyAsync(r => r.UserId == userId
                            && r.PackageId == packageId
                            && r.ExpirationDate == null);
            if (hasPermanent)
                return BadRequest("Bạn đã có gói vĩnh viễn, không thể gia hạn.");

            // Sinh OrderID
            model.OrderID = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Tạo transaction Pending
            var txn = new Order
            {
                OrderId = model.OrderID,
                TransactionId = null,
                UserId = userId,
                PackageId = packageId,
                Amount = Convert.ToDecimal(model.Amount),
                Status = "Pending",
                CreatedAt = DateTime.Now,
                PaymentMethodId = 2,

            };
            _dbContext.Orders.Add(txn);
            await _dbContext.SaveChangesAsync();

            // Trả URL VnPay
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { PaymentUrl = url });
        }



        [AllowAnonymous]
        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> PaymentCallbackAsync()
        {
            var resp = _vnPayService.PaymentExecute(Request.Query);
            var txn = await _dbContext.Orders
                                .FirstOrDefaultAsync(x => x.OrderId == resp.OrderId);
            if (txn != null)
            {
                txn.TransactionId = resp.TransactionId;
                txn.Status = resp.VnPayResponseCode == "00"
                                      ? "Success"
                                      : "Failed";
                if (DateTime.TryParseExact(resp.PaymentDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    txn.CreatedAt = parsedDate;
                }

                await _dbContext.SaveChangesAsync();

                if (txn.Status == "Success")
                {
                    var regDate = DateTime.Now;
                    await RegisterOrRenewAsync(txn.UserId, txn.PackageId, regDate);
                }

            }

            return Ok(resp);
        }




        private async Task RegisterOrRenewAsync(int userId, int packageId, DateTime registrationDate)
        {
            // existing?
            var exist = await _dbContext.UserPackageRegistrations
                .FirstOrDefaultAsync(r => r.UserId == userId
                                       && r.PackageId == packageId);

            // load package + includes
            var pkg = await _dbContext.Packages
                .Include(p => p.PackageInclusionParentPackages)
                  .ThenInclude(pi => pi.IncludedPackage)
                .FirstOrDefaultAsync(p => p.PackageId == packageId);
            if (pkg == null) return;

            int days = pkg.DurationDay ?? 0;
            DateTime baseDate;

            if (exist == null)
            {
                baseDate = registrationDate;
                _dbContext.UserPackageRegistrations.Add(new UserPackageRegistration
                {
                    UserId = userId,
                    PackageId = packageId,
                    RegistrationDate = registrationDate,
                    ExpirationDate = days > 0
                                       ? baseDate.AddDays(days)
                                       : (DateTime?)null
                });
            }
            else
            {
                // renew từ ExpirationDate nếu còn hạn, hoặc từ registrationDate
                baseDate = exist.ExpirationDate.HasValue
                           && exist.ExpirationDate > registrationDate
                    ? exist.ExpirationDate.Value
                    : registrationDate;

                exist.ExpirationDate = days > 0
                    ? baseDate.AddDays(days)
                    : (DateTime?)null;
            }

            await _dbContext.SaveChangesAsync();

            // đệ quy gói con
            foreach (var pi in pkg.PackageInclusionParentPackages)
            {
                await RegisterOrRenewAsync(
                    userId,
                    pi.IncludedPackage.PackageId,
                    registrationDate);
            }
        }
    }








}