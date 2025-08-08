using Microsoft.AspNetCore.Mvc;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TestQuizController : Controller
    {
        [Route("Admin/TestQuiz/Index")]
        public IActionResult Index()
        {

            return View();
        }
    }
}
