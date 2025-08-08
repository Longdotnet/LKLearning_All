using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class ProfileController : BaseController
    {
        private string GetToken() => HttpContext.Session.GetString("AuthToken");

        [Route("User/Profile")]
        public IActionResult Index()
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            if (user == null)
            {
                TempData["error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Dashboard", new { area = "User" });
            }
            return View(user);
        }

        [Route("User/Profile/Avatar")]
        public IActionResult Avatar()
        {
            var token = GetToken();
            var avatars = CXuLy.GetListAvatar(token);
            return View(avatars);
        }

        [Route("User/Profile/ExamHistory")]
        public IActionResult ExamHistory(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            var histories = user != null ? CXuLy.GetUserExamHistory(user.UserId) : new List<CUserExamHistory>();

            // Pagination
            const int pageSize = 5;
            int totalItems = histories.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedHistories = histories.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(pagedHistories);
        }

        [Route("User/Profile/Report")]
        public IActionResult Report(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            var reports = user != null ? CXuLy.getDSReport()?.Where(r => r.UserId == user.UserId).ToList() : new List<CReview>();

            // Pagination
            const int pageSize = 5;
            int totalItems = reports.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedReports = reports.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(pagedReports);
        }

        [Route("User/Profile/Review")]
        public IActionResult Review(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            var reviews = user != null ? CXuLy.getDSReview()?.Where(r => r.UserId == user.UserId).ToList() : new List<CReview>();

            // Pagination
            const int pageSize = 5;
            int totalItems = reviews.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedReviews = reviews.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(pagedReviews);
        }

        [Route("User/Profile/Review/{id}")]
        public IActionResult ReviewById(int id)
        {
            var token = GetToken();
            var review = CXuLy.GetReviewById(id, token);
            if (review == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá/báo cáo.";
                return RedirectToAction("Review");
            }
            return View(review);
        }

        [Route("User/Profile/Certificate")]
        public IActionResult Certificate(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);

            // If GetCertificateById returns a list, paginate it
            var certificates = CXuLy.GetCertificateById(user.UserId, token);
            var certList = certificates is IEnumerable<CCertificate> list ? list.ToList() : new List<CCertificate>();
            if (certList.Count > 0)
            {
                const int pageSize = 5;
                int totalItems = certList.Count;
                if (page < 1) page = 1;
                var paginate = new Paginate(totalItems, page, pageSize);
                int skip = (page - 1) * pageSize;
                var pagedCerts = certList.Skip(skip).Take(pageSize).ToList();

                ViewBag.Paginate = paginate;
                return View(pagedCerts);
            }
            // If not a list, just return as before
            return View(certificates);
        }

        [Route("User/Profile/Package")]
        public IActionResult Package(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            var registrations = user != null ? CXuLy.getPackagesByUserId(user.UserId.ToString()) : new List<CUserPackage>();

            // Pagination
            const int pageSize = 5;
            int totalItems = registrations.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedRegistrations = registrations.Skip(skip).Take(pageSize).ToList();

            ViewBag.AllPackages = CXuLy.getDSPackage();
            ViewBag.Paginate = paginate;
            return View(pagedRegistrations);
        }

        [Route("User/Profile/Payment")]
        public IActionResult Payment(int page = 1)
        {
            var token = GetToken();
            var payments = CXuLy.GetDSOrderByUserId(token);
            if (payments == null)
            {
                TempData["error"] = "Không tìm thấy thông tin thanh toán.";
                return RedirectToAction("Index");
            }

            // Pagination
            const int pageSize = 5;
            int totalItems = payments.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedPayments = payments.Skip(skip).Take(pageSize).ToList();

            ViewBag.PaymentMethods = CXuLy.getDSPaymentMethod(token);
            ViewBag.Paginate = paginate;
            return View(pagedPayments);
        }

        [Route("User/Profile/Course")]
        public IActionResult Course(int page = 1)
        {
            var token = GetToken();
            var courses = CXuLy.getDSCourse() ?? new List<CCourse>();

            // Pagination
            const int pageSize = 5;
            int totalItems = courses.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedCourses = courses.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(pagedCourses);
        }

        [Route("User/Profile/AcademicResult")]
        public IActionResult AcademicResult(int page = 1)
        {
            var token = GetToken();
            var user = CXuLy.LayThongTinUser(token);
            var results = user != null ? CXuLy.GetDSAcademicResultByUserId(user.UserId.ToString(), token) : new List<CAcademicResult>();

            // Pagination
            const int pageSize = 5;
            int totalItems = results.Count;
            if (page < 1) page = 1;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var pagedResults = results.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(pagedResults);
        }

        [Route("User/Profile/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Dashboard", new { area = "User" });
        }
    }
}
