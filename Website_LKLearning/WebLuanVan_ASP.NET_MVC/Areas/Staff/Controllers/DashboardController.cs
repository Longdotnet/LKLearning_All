using Microsoft.AspNetCore.Mvc;

namespace WebLuanVan_ASP.NET_MVC.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class DashboardController : Controller
    {
        [Route("1")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
