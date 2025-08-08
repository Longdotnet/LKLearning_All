using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;


namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers

{
    [Area("Admin")]
    public class DashboardController : BaseController
    {

        [Route("Admin")]
        public  IActionResult Index()
            {
            string token = HttpContext.Session.GetString("AuthToken");
            var users = CXuLy.getDSUsers() ?? new List<CUsers>();
            var packages = CXuLy.getDSPackage() ?? new List<CPackage>();
            var courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            var orders = CXuLy.getDSOrders(token) ?? new List<COrders>();
            var results = CXuLy.GetDSAcademicResult(token) ?? new List<CAcademicResult>();
            // Doanh thu hôm nay
            var today = DateTime.Today;
            var revenueToday = orders
                .Where(o => o.CreatedAt.Date == today)
                .Sum(o => o.Amount);

            // Doanh thu từng ngày trong tuần này (từ thứ 2 đến hôm nay)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Thứ 2
            var revenueByDay = Enumerable.Range(0, 7)
                .Select(i => startOfWeek.AddDays(i))
                .Select(date => new
                {
                    Date = date,
                    Revenue = orders.Where(o => o.CreatedAt.Date == date).Sum(o => o.Amount)
                })
                .ToList();
            // Lưu doanh thu hôm nay và doanh thu theo ngày vào ViewBag để sử dụng trong View
            ViewBag.RevenueToday = revenueToday;
            ViewBag.RevenueByDay = revenueByDay;
            // Đăng nhập gần đây
            var recentLogins = users
                .Where(u => u.LastLoginDate != null)
                .OrderByDescending(u => u.LastLoginDate)
                .Take(5)
                .ToList();

            // Lịch sử thanh toán gần đây (map tên user, package)
            var recentOrders = orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToList();

          

            ViewBag.RecentLogins = recentLogins;
            ViewBag.RecentOrders = recentOrders;
            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;
            // Các thống kê khác (giữ nguyên như trước)
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalCourses = CXuLy.getDSCourse()?.Count ?? 0;
            ViewBag.TotalLessons = CXuLy.getDSLesson()?.Count ?? 0;
            ViewBag.TotalQuestions = CXuLy.getDSQuestion()?.Count ?? 0;
            ViewBag.TotalPackages = CXuLy.getDSPackage()?.Count ?? 0;
            ViewBag.TotalAdmin = users.Count(u => u.RoleId == 2);
            ViewBag.TotalStudents = users.Count(u => u.RoleId == 1);
            ViewBag.TotalAmountOrders = orders.Sum(o => o.Amount);

            var questions = CXuLy.getDSQuestion() ?? new List<CQuestion>();
            var questionByContent = questions
                .GroupBy(q => q.ContentTypeId)
                .Select(g => new { ContentTypeId = g.Key, Count = g.Count() })
                .ToList();
            ViewBag.QuestionByContent = questionByContent;

            return View();
        }
        [Route("Admin/Logout")]
        public IActionResult Logout()
        {
            // Xóa toàn bộ các Session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "LogIn", new { area = "Login" });
        }
       

    }
}
