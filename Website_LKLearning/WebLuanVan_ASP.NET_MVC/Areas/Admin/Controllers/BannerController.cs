using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : BaseController
    {
        [Route("Admin/Banner")]
        public IActionResult Index(int page,string searchTerm)
        {
            // Lấy danh sách banner từ API
            List<CBanner> banners = CXuLy.getBanner() ?? new List<CBanner>();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                banners = banners.Where(u =>
                    (u.BannerTitle?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.BannerSubtitle?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false
                    )).ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            // phân trang paginate
            // hiển thị bao nhieu items 
            // B1: cài đặt thông số phân trang
            const int pageSize = 5;
            if (page < 1)
            {
                page = 1; // Đảm bảo trang bắt đầu từ 1
            }
            // B2: Tính toán dữ liệu
            int dem = banners.Count; // Tổng số item có trong ds đó

            var paginatenew = new Paginate(dem, page, pageSize); //Khởi tạo đối tượng phân trang
            // B3: lấy dữ liệu trang hiện tại
            int recSkip = (page - 1) * pageSize; // Vĩ trí bắt đầu lấy dữ liệu

            var data = banners
                .Skip(recSkip) // Bỏ qua các item trước đó
                .Take(pageSize) // Lấy số lượng item của hiện tại
                .ToList();
            // B4 Truyền dữ liệu ra View
            ViewBag.Paginate = paginatenew; // Truyền thông tin phân trang
            return View(data);
        }

        [Route("Admin/Banner/formThemBanner")]
        public IActionResult formThemBanner()
        {
            return View();
        }

        [HttpPost]
        [Route("Admin/Banner/themBanner")]
        public IActionResult themBanner(CBanner banner)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                // Gọi API để thêm banner
                bool success = CXuLy.themBanner(banner, token);
                if (success)
                {
                    TempData["success"] = "Thêm banner thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm banner không thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Admin/Banner/formSuaBanner/{id}")]
        public IActionResult formSuaBanner(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // Lấy banner theo id
            CBanner banner = CXuLy.getBannerById(id);
            if (banner == null)
            {
                TempData["error"] = "Không tìm thấy banner!";
                return RedirectToAction("Index");
            }
            return View(banner);
        }

        [HttpPost]
        [Route("Admin/Banner/formSuaBanner/{id}")]
        public IActionResult suaBanner(string id, CBanner banner)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                bool success = CXuLy.suaBanner(id, banner, token);
                if (success)
                {
                    TempData["success"] = "Sửa banner thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Sửa banner không thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Admin/Banner/xoaBanner/{id}")]
        public IActionResult xoaBanner(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                bool success = CXuLy.xoaBanner(id, token);
                if (success)
                {
                    TempData["success"] = "Xóa banner thành công!";
                }
                else
                {
                    TempData["error"] = "Xóa banner không thành công!";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [Route("Admin/Banner/viewDetail/{id}")]
        public IActionResult viewDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CBanner banner = CXuLy.getBannerById(id);
            if (token == null)
            {
                TempData["error"] = "Banner không tồn tại";
                return RedirectToAction("Index");
            }            
            return View(banner);
        }
    }
}
