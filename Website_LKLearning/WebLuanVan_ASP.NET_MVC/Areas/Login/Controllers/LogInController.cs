using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Areas.Login.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS;

namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Controllers
{
    [Area("Login")]
    public class LogInController : BaseController
    {
        [Route("Login")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LogIn(DangNhapDTO x)
        {
            try
            {
                TokenResultDTO tokenResult = await CXuLy.xacThucLogIN(x);
                if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
                {
                    return Content("Đăng nhập không thành công");
                }

                // Lưu token và thông tin user vào Session nếu cần sử dụng riêng (tùy chọn)
                HttpContext.Session.SetString("AuthToken", tokenResult.Token);
           

                // Tạo danh sách các Claim dựa trên kết quả token từ API
                var claims = new List<Claim>
            {
              
               
                new Claim(ClaimTypes.Role, tokenResult.RoleId) // tokenResult.RoleId phải trả về "Admin" hoặc "User"
            };

                // Tạo đối tượng ClaimsIdentity, xác định scheme là CookieAuthentication
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Tùy chọn đăng nhập, ví dụ: cho phiên làm việc kéo dài 1 giờ
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                // Đăng nhập thông qua Cookie Authentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Sau khi tạo cookie, bạn vẫn có thể dùng tokenResult.RoleId để chuyển hướng ngay lúc đăng nhập.
                if (tokenResult.RoleId == "Admin")
                {
                    TempData["success"] = "Đăng nhập thành công với quyền Admin!";

                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (tokenResult.RoleId == "User")
                {
                    TempData["success"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Dashboard", new { area = "User" });
                }
                else
                {
                    return Content("Role không xác định: " + tokenResult.RoleId);
                }
            }
            catch (Exception ex)
            {
                return Content("Đăng nhập không thành công: " + ex.Message);
            }
        }
    }
}