using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.SignUp.Models.DTOS;

namespace WebLuanVan_ASP.NET_MVC.Areas.SignUp.Controllers
{
    [Area("SignUp")]
    public class SignUpController : Controller
    {
        [Route("SignUp")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        
        public IActionResult DangKyTK(DangKyDTO x)
        {
            try
            {
                if(CXuLy.DangKy(x))
                {
                    TempData["success"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Index", "Login", new {area="Login" } );
                }
                else
                {
                    TempData["error"] = "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin.";
                    return Content("Đăng ký không thành công");
                }
            }
            catch
            {
                return Content("Đăng ký không thành công");
            }
        }
    }
}
