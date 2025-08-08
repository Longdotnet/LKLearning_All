using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class LibraryController : BaseController
    {
        [Route("library")]
        public IActionResult Index(int page = 1, string searchTerm = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestion> dsCauHoi = Admin.Models.CXuLy.getDSQuestion()
                // QuestionTypeID = 6 là cái đó là id của nghe

                .Where(t => t.QuestionTypeId == 6)
                .ToList();
            ViewBag.Categories = CXuLy.getDSCategory() ?? new List<CCategory>();

            // Apply search if searchTerm is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsCauHoi = dsCauHoi
                    .Where(t => t.QuestionText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            const int pageSize = 6; // Adjusted for grid layout (3 columns x 2 rows)
            if (page < 1)
            {
                page = 1;
            }

            int totalItems = dsCauHoi.Count;
            var paginate = new Paginate(totalItems, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = dsCauHoi.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }
        [Route("library/{id}")]
        public IActionResult Detail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var question = Admin.Models.CXuLy.getQuestionById(id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);

        }
    }
}
