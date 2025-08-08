using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class DashboardController : BaseController
    {
        [Route("")]
        public IActionResult Index()
        {
            List<CCourse> dsCourse = Admin.Models.CXuLy.getDSCourse();
            List<CBanner> cBanner = Admin.Models.CXuLy.getBanner();
            List<CCategory> dsCategory = Admin.Models.CXuLy.getDSCategory();
            ViewBag.Banner = cBanner;
            ViewBag.Courses = dsCourse;
            ViewBag.Categories = dsCategory;
            string token = HttpContext.Session.GetString("AuthToken");
            UserProfile x = UpdateProfile.Models.CXuLy.GetUserInformation(token);

            // Get the latest 3 reviews
            var allReviews = Admin.Models.CXuLy.getDSReview();
            var recentReviews = allReviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToList();
            ViewBag.RecentReviews = recentReviews;

            // --- Add these lines for statistics ---
            var users = Admin.Models.CXuLy.getDSUsers() ?? new List<CUsers>();
            var questions = Admin.Models.CXuLy.getDSQuestion() ?? new List<CQuestion>();
            ViewBag.TotalCourses = dsCourse?.Count??0;
            ViewBag.TotalAdmin = users.Count(u => u.RoleId == 2 );
            ViewBag.TotalStudents = users.Count(u => u.RoleId == 1);
            ViewBag.TotalQuestions = questions.Count;

            ViewBag.UserProfile = x;
            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;

            return View(x);
        }
        [HttpGet]
        public async Task<IActionResult> VocabularySuggestionAjax(string q)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var vocabularies = Admin.Models.CXuLy.GetListVocabularyUser(token);

            // Only return word and id for suggestions
            var results = vocabularies
                .Where(v => v.Word.Contains(q ?? "", StringComparison.OrdinalIgnoreCase))
                .Select(v => new { v.VocabularyId, v.Word })
                .Take(10)
                .ToList();

            return Json(results);
        }

        public async Task<IActionResult> VocabularyDetailAjax(int id, string word = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var vocab = Admin.Models.CXuLy.GetVocabularyById(id, token);

            // If not found, try API with word
            if (vocab == null && !string.IsNullOrEmpty(word))
            {
                using (var client = new HttpClient())
                {
                    var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
                    var apiUrl = $"{baseUrl}QuanLyVocabulary/get-word/{word}";
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        vocab = System.Text.Json.JsonSerializer.Deserialize<CVocabulary>(json);
                    }
                }
            }

            return PartialView("_VocabularySearchResults", new List<CVocabulary> { vocab });
        }


        [Route("User/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Dashboard", new { area = "User" });
        }
    }
}
