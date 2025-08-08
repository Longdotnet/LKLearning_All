using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : BaseController
    {
        [Route("Review")]
        public IActionResult Index(int? rating, int page = 1)
        {


            List<CReview> dsReview = CXuLy.getDSReview();
            if (dsReview == null)
            {
                ViewBag.Error = "Không thể tải danh sách đánh giá.";
                dsReview = new List<CReview>();
            }

            var ratings = dsReview
                .Select(r => new { Rating = r.Rating })
                .Distinct()
                .OrderBy(r => r.Rating)
                .Select(r => new CReview { Rating = r.Rating })
                .ToList();

            if (rating.HasValue && rating.Value > 0)
            {
                dsReview = dsReview.Where(r => r.Rating == rating.Value).ToList();
            }

            ViewBag.Ratings = ratings.Any() ? ratings : new List<CReview> { new CReview { Rating = 0 } };
            ViewBag.CurrentRating = rating;
            // Phân trang 
            const int pageSize = 5;
            if (page < 1)
            {
                page = 1; // Đảm bảo trang bắt đầu từ 1
            }
            // B2: Tính toán dữ liệu
            int dem = dsReview.Count; // Tổng số item có trong ds đó

            var paginatenew = new Paginate(dem, page, pageSize); //Khởi tạo đối tượng phân trang
            // B3: lấy dữ liệu trang hiện tại
            int recSkip = (page - 1) * pageSize; // Vĩ trí bắt đầu lấy dữ liệu

            var data = dsReview
                .Skip(recSkip) // Bỏ qua các item trước đó
                .Take(pageSize) // Lấy số lượng item của hiện tại
                .ToList();
            // B4 Truyền dữ liệu ra View
            ViewBag.Paginate = paginatenew; // Truyền thông tin phân trang
            return View(data);

        }

        // Xem chi tiết review
        [Route("viewDetail/{id}")]
        public IActionResult viewDetail(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");  
            var review = CXuLy.GetReviewById(id,token);
            if (review == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá.";
                return RedirectToAction("Index");
            }
            return View(review);
        }

        // Xóa review
        [Route("xoaReview/{id}")]
        public IActionResult xoaReview(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.DeleteReview(id, token))
            {
                TempData["success"] = "Xóa đánh giá thành công.";
            }
            else
            {
                TempData["error"] = "Xóa đánh giá thất bại.";
            }
            return RedirectToAction("Index");
        }

        // Sửa review (GET)
        [Route("formSuaReview/{id}")]
        public IActionResult formSuaReview(int id)
        {
            string token=HttpContext.Session.GetString("AuthToken");
            var review = CXuLy.GetReviewById(id,token);
            if (review == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá.";
                return RedirectToAction("Index");
            }
            return View(review);
        }

        // Sửa review (POST)
        [HttpPost]
        [Route("formSuaReview/{id}")]
        public IActionResult SuaReview(int id, CReview review)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // Gán ReviewType mặc định nếu chưa có
            review.ReviewType = "1"; // Mặc định là review khóa học
            if (CXuLy.UpdateReview(id, review, token))
            {
                TempData["success"] = "Cập nhật đánh giá thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Cập nhật đánh giá thất bại.";
                return View(review);
            }
        }

        // Thêm review mới (nếu cần)
        [Route("formThemReview")]
        public IActionResult formThemReview()
        {
            var users = CXuLy.getDSUsers() ?? new List<CUsers>();
            var courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            ViewBag.Users = users;
            ViewBag.Courses = courses;
            return View(new CReview());
        }


        [HttpPost]
        [Route("formThemReview")] // Sửa route để khớp với GET
        public IActionResult ThemReview(CReview review)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            // Gán ReviewType mặc định nếu chưa có
            review.ReviewType = review.ReviewType ?? "1"; // Mặc định là review khóa học

            if (CXuLy.CreateReview(review, token))
            {
                TempData["success"] = "Thêm đánh giá thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thêm đánh giá thất bại.";
                var users = CXuLy.getDSUsers() ?? new List<CUsers>();
                var courses = CXuLy.getDSCourse() ?? new List<CCourse>();
                ViewBag.Users = users;
                ViewBag.Courses = courses;
                return View("formThemReview", review);
            }
        }

    }
}