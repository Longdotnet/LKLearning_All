using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Areas.User.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class PackageController : BaseController
    {
        private readonly string _apiBaseUrl;

        private readonly ILogger<PackageController> _logger;

        public PackageController(ILogger<PackageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:ApiZaloPay"];
        }
        [Route("cart")]
        public IActionResult Index()
        {
            List<CPackage> dsPackage = CXuLy.getDSPackage()?.Where(t => t.PackageName != "FREE").ToList();
            ViewBag.dsPackage = dsPackage;
            ViewBag.Categories = CXuLy.getDSCategory() ?? new List<CCategory>();

            // Lấy danh sách phương thức thanh toán
            string token = HttpContext.Session.GetString("AuthToken");
            List<CPaymentMethod> dsPaymentMethod = CXuLy.getDSPaymentMethod(token) ?? new List<CPaymentMethod>();
            ViewBag.dsPaymentMethod = dsPaymentMethod;
            return View();
        }

        [Route("purchase/{packageId}")]
        [HttpPost]
        public async Task<IActionResult> Purchase(int packageId, string paymentMethod)
        {
            var package = CXuLy.getPackageById(packageId.ToString());
            if (package == null || package.Price == null)
                return NotFound();

            string token = HttpContext.Session.GetString("AuthToken");
            var user = CXuLy.LayThongTinUser(token);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mua gói cước.";
                return RedirectToAction("Index");
            }

            var orderId = $"{paymentMethod.ToUpper()}_{DateTime.Now:yyyyMMddHHmmss}_{user.UserId}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                if (paymentMethod == "ZaloPay")
                {
                    var requestData = new
                    {
                        OrderId = orderId,
                        Amount = package.Price,
                        Description = $"Thanh toán cho gói {package.PackageName} của người dùng {user.Fullname}",
                        UserId = user.UserId,
                        PackageId = packageId
                    };

                    var response = await client.PostAsJsonAsync("payment/create-zalopay", requestData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<ZaloPayCreateResponse>();
                        if (result != null && !string.IsNullOrEmpty(result.PaymentUrl))
                        {
                            HttpContext.Session.SetString("CurrentOrder", JsonConvert.SerializeObject(new
                            {
                                OrderId = orderId,
                                PackageId = packageId,
                                UserId = user.UserId,
                                Amount = package.Price
                            }));
                            return Redirect(result.PaymentUrl);
                        }
                    }
                }
                else if (paymentMethod == "VNPay")
                {
                    var callbackUrl = Url.Action("VnPayCallback", "Package", new { area = "User" }, protocol: Request.Scheme);

                    var requestData = new
                    {
                        orderID = orderId,
                        amount = package.Price,
                        orderDescription = $"Thanh toán cho gói {package.PackageName} của người dùng {user.Fullname}",
                        returnUrl = callbackUrl,
                        vnp_ReturnUrl = callbackUrl
                    };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    // VNPay expects packageId as query param
                    var response = await client.PostAsJsonAsync($"payment/create-vnpay?packageId={packageId}", requestData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<VnPayCreateResponse>();
                        if (result != null && !string.IsNullOrEmpty(result.PaymentUrl))
                        {
                            HttpContext.Session.SetString("CurrentOrder", JsonConvert.SerializeObject(new
                            {
                                OrderId = orderId,
                                PackageId = packageId,
                                UserId = user.UserId,
                                Amount = package.Price
                            }));
                            return Redirect(result.PaymentUrl);
                        }
                    }
                }
            }

            TempData["ErrorMessage"] = "Không thể tạo yêu cầu thanh toán. Vui lòng thử lại.";
            return RedirectToAction("Index");
        }
        // Callback cho ZaloPay
        [Route("User/Payment/ZaloPayCallback")]
        [HttpGet, HttpPost]
        public IActionResult ZaloPayCallback()
        {
            var status = Request.Query["status"].ToString();
            var appTransId = Request.Query["apptransid"].ToString();

            var orderJson = HttpContext.Session.GetString("CurrentOrder");
            if (string.IsNullOrEmpty(orderJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Index");
            }

            var order = JsonConvert.DeserializeAnonymousType(orderJson, new
            {
                OrderId = "",
                PackageId = 0,
                UserId = "",
                Amount = 0m
            });

            if (status == "1" && !string.IsNullOrEmpty(appTransId))
            {
                // Cập nhật trạng thái đơn hàng thành công vào DB qua WebAPI
                var token = HttpContext.Session.GetString("AuthToken");
                var capNhatThanhCong = CXuLy.CapNhatTrangThaiDonHangZaloPay(appTransId, "Success", token);

                if (capNhatThanhCong)
                {
                    // Xác nhận thành công, cập nhật gói cho user
                    var userPackage = new CUserPackage
                    {
                        UserId = int.Parse(order.UserId),
                        PackageId = order.PackageId,
                        RegistrationDate = DateTime.Now
                    };

                    var saveResult = CXuLy.themUserPackage(userPackage, token);
                    if (saveResult)
                    {
                        HttpContext.Session.Remove("CurrentOrder");
                        TempData["SuccessMessage"] = "Thanh toán thành công! Hãy khám phá gói cước của bạn.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng.";
                    return RedirectToAction("Index");
                }
            }

            TempData["ErrorMessage"] = "Thanh toán thất bại hoặc giao dịch chưa hoàn tất.";
            return RedirectToAction("Index");
        }


        // Callback cho VNPay
        [Route("User/Payment/VnPayCallback")]
        [HttpGet, HttpPost]
        public async Task<IActionResult> VnPayCallback()
        {
            // Lấy mã phản hồi từ VNPay
            var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();

            // Lấy thông tin đơn hàng từ session
            var orderJson = HttpContext.Session.GetString("CurrentOrder");
            if (string.IsNullOrEmpty(orderJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Index");
            }

            var order = JsonConvert.DeserializeAnonymousType(orderJson, new
            {
                OrderId = "",
                PackageId = 0,
                UserId = "",
                Amount = 0m
            });

            // Nếu thanh toán thành công
            if (vnpResponseCode == "00")
            {
                // Gọi WebAPI để cập nhật trạng thái đơn hàng và đăng ký gói cho user
                string token = HttpContext.Session.GetString("AuthToken");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Truyền toàn bộ query string của VNPay sang WebAPI
                    var callbackUrl = $"payment/vnpay-callback{Request.QueryString}";
                    var response = await client.GetAsync(callbackUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Đăng ký gói cho user trên hệ thống MVC (nếu cần lưu local hoặc đồng bộ)
                        var userPackage = new CUserPackage
                        {
                            UserId = int.Parse(order.UserId),
                            PackageId = order.PackageId,
                            RegistrationDate = DateTime.Now
                        };

                     
                        HttpContext.Session.Remove("CurrentOrder");
                        TempData["SuccessMessage"] = "Thanh toán thành công! Hãy khám phá gói cước của bạn.";
                        return RedirectToAction("Index");
                        
                    }
                }
            }

            TempData["ErrorMessage"] = "Thanh toán thất bại hoặc giao dịch chưa hoàn tất.";
            return RedirectToAction("Index");
        }
    }
}