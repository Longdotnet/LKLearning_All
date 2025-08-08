using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using WEBAPI.Services.Models;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Models;

namespace WEBAPI.Services.ZaloPay
{
    public class ZaloPayService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZaloPayService> _logger;
        private readonly LuanvantienganhContext _dbContext;

        public ZaloPayService(IConfiguration config, HttpClient httpClient, ILogger<ZaloPayService> logger, LuanvantienganhContext dbContext)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> CreatePayment(ZaloPayRequest request)
        {
            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(request.OrderId))
            {
                _logger.LogError("OrderId không được để trống");
                throw new ArgumentException("OrderId là bắt buộc");
            }
            if (request.UserId <= 0)
            {
                _logger.LogError("UserId không hợp lệ");
                throw new ArgumentException("UserId là bắt buộc");
            }
            if (request.PackageId <= 0)
            {
                _logger.LogError("PackageId không hợp lệ");
                throw new ArgumentException("PackageId là bắt buộc");
            }

            // Lấy thông tin cấu hình từ appsettings.json
            var appId = _config["ZaloPayConfig:AppId"] ?? throw new ArgumentNullException("ZaloPayConfig:AppId");
            var key1 = _config["ZaloPayConfig:Key1"] ?? throw new ArgumentNullException("ZaloPayConfig:Key1");
            var callbackUrl = _config["ZaloPayConfig:CallbackUrl"] ?? throw new ArgumentNullException("ZaloPayConfig:CallbackUrl");

            // Tạo mã giao dịch duy nhất dựa trên thời gian Việt Nam (GMT+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var appTransId = $"{vietnamTime:yyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Tạo thời gian Unix (milliseconds) cho giao dịch
            var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Tìm PaymentMethodId cho ZaloPay // định dạng lại lỡ là zaloPay thì sao

            var paymentMethod = await _dbContext.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Name == "ZaloPay");
            if (paymentMethod == null)
            {
                _logger.LogError("Phương thức thanh toán ZaloPay không tồn tại");
                throw new Exception("Phương thức thanh toán ZaloPay không tồn tại");
            }
            // Fetch package to get the price
            var package = await _dbContext.Packages
                .FirstOrDefaultAsync(p => p.PackageId == request.PackageId);
            if (package == null)
            {
                _logger.LogError($"Package not found for PackageId: {request.PackageId}");
                throw new Exception("Package not found");
            }
            // Chuẩn bị dữ liệu gửi đến ZaloPay
            var data = new Dictionary<string, string>
            {
                { "app_id", appId },
                { "app_user", "user123" },
                { "app_trans_id", appTransId },
                { "app_time", appTime },
                { "amount", package.Price.ToString() }, // Use package price
                { "description", request.Description },
                { "order_id", request.OrderId },
                { "callback_url", callbackUrl },
                { "item", "[]" },
                { "embed_data", JsonConvert.SerializeObject(new { redirecturl = _config["ZaloPayConfig:RedirectUrl"], order_id = request.OrderId }) },
                { "bank_code", "" }
            };

            // Tính mã xác thực (MAC) bằng HMAC-SHA256
            data["mac"] = ComputeHMACSHA256(key1, BuildSignatureData(data));

            // Lưu thông tin giao dịch vào database với trạng thái "Pending"
            var order = new Order
            {
                OrderId = request.OrderId,
                TransactionId = appTransId,
                Status = "Pending",
                UserId = request.UserId,
                PackageId = request.PackageId,
                CreatedAt = vietnamTime,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                Amount = request.Amount
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Yêu cầu thanh toán: {JsonConvert.SerializeObject(data)}");

            // Gửi yêu cầu đến ZaloPay
            var response = await _httpClient.PostAsync(
                _config["ZaloPayConfig:Endpoint"],
                new FormUrlEncodedContent(data)
            );

            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Phản hồi từ ZaloPay: {responseString}");

            // Kiểm tra phản hồi từ ZaloPay
            dynamic responseData = JsonConvert.DeserializeObject(responseString)!;
            if (responseData.return_code != 1)
            {
                _logger.LogError($"Lỗi ZaloPay: {responseData.return_message}, Sub-return code: {responseData.sub_return_code}");
                throw new Exception($"Lỗi ZaloPay: {responseData.return_message} (Mã lỗi phụ: {responseData.sub_return_code})");
            }

            string orderUrl = responseData.order_url?.ToString();
            if (string.IsNullOrEmpty(orderUrl))
            {
                _logger.LogError("Không nhận được URL thanh toán từ ZaloPay");
                throw new Exception("Không thể lấy URL thanh toán từ ZaloPay");
            }

            return orderUrl;
        }

        public bool VerifyCallback(ZaloPayCallback callback)
        {
            var key2 = _config["ZaloPayConfig:Key2"] ?? throw new ArgumentNullException("ZaloPayConfig:Key2");

            string dataToSign = callback.Data;
            string computedMac = ComputeHMACSHA256(key2, dataToSign);

            _logger.LogInformation($"Computed Mac: {computedMac}, Expected Mac: {callback.Mac}");
            return computedMac == callback.Mac;
        }

        private string ComputeHMACSHA256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private string BuildSignatureData(Dictionary<string, string> data)
        {
            return $"{data["app_id"]}|{data["app_trans_id"]}|{data["app_user"]}|{data["amount"]}|{data["app_time"]}|{data["embed_data"]}|{data["item"]}";
        }
    }
}