using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : BaseController
    {
        [Route("Report")]
        public IActionResult Index(int? type, int page = 1)
        {
            List<CReview> dsReport = CXuLy.getDSReport();
            if (dsReport == null)
            {
                ViewBag.Error = "Không thể tải danh sách báo cáo.";
                dsReport = new List<CReview>();
            }

            var reportTypes = dsReport
                .Select(r => new { ReviewType = r.ReviewType })
                .Distinct()
                .OrderBy(r => r.ReviewType)
                .Select(r => new CReview { ReviewType = r.ReviewType })
                .ToList();

            if (type.HasValue && type.Value > 0)
            {
                dsReport = dsReport.Where(r => r.ReviewType == type.Value.ToString()).ToList();
            }

            ViewBag.ReportTypes = reportTypes.Any() ? reportTypes : new List<CReview> { new CReview { ReviewType = "0" } };
            ViewBag.CurrentType = type;

            const int pageSize = 5;
            if (page < 1)
            {
                page = 1;
            }

            int dem = dsReport.Count;
            var paginatenew = new Paginate(dem, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = dsReport.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginatenew;
            return View(data);
        }

        [Route("Report/viewDetail/{id}")]
        public IActionResult ViewDetail(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var report = CXuLy.GetReviewById(id, token);
            if (report == null)
            {
                TempData["error"] = "Không tìm thấy báo cáo.";
                return RedirectToAction("Index");
            }
            return View(report);
        }

        [Route("Report/xoaReport/{id}")]
        public IActionResult XoaReport(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.DeleteReview(id, token))
            {
                TempData["success"] = "Xóa báo cáo thành công.";
            }
            else
            {
                TempData["error"] = "Xóa báo cáo thất bại.";
            }
            return RedirectToAction("Index");
        }

      
        [Route("Report/formThemReport")]
        public IActionResult FormThemReport()
        {
            var users = CXuLy.getDSUsers() ?? new List<CUsers>();
            var courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            var lessons = CXuLy.getDSLesson() ?? new List<CLesson>();
            ViewBag.Users = users;
            ViewBag.Courses = courses;
            ViewBag.Lessons = lessons;
            return View(new CReview());
        }

        [HttpPost]
        [Route("Report/formThemReport")]
        public IActionResult ThemReport(CReview report)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            report.ReviewType = report.ReviewType ?? "2"; // Default to lesson report
            report.Rating = 1; // Default rating for reports

            if (CXuLy.CreateReport(report, token))
            {
                TempData["success"] = "Thêm báo cáo thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thêm báo cáo thất bại.";
                var users = CXuLy.getDSUsers() ?? new List<CUsers>();
                var courses = CXuLy.getDSCourse() ?? new List<CCourse>();
                var lessons = CXuLy.getDSLesson() ?? new List<CLesson>();
                ViewBag.Users = users;
                ViewBag.Courses = courses;
                ViewBag.Lessons = lessons;
                return View("FormThemReport", report);
            }
        }
    }
}