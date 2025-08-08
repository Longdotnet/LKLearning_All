using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Services.Solana;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class CertificateController : BaseController
    {
        // Danh sách chứng chỉ
        [Route("User/Certificate")]
        public IActionResult Index()
        {
            ViewBag.Categories = CXuLy.getDSCategory() ?? new List<CCategory>();

            return View();
        }
        [HttpPost]
        [Route("User/Certificate/Verify")]
        public IActionResult Verify([FromBody] VerifyRequest request)
        {
            var result = Admin.Models.CXuLy.VerifyCertificate(request.VerifyCode);
            return Json(result);
        }
    }
}
