using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Services.Solana;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CertificateController : BaseController
    {
        // Danh sách chứng chỉ
        [Route("Admin/Certificate")]
        public IActionResult Index(int page, int? courseId = null, string? searchTerm=null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CCertificate> dsCertificate = CXuLy.GetListCertificate(token);
            // Lọc theo khóa học nếu có
            if (courseId.HasValue)
                dsCertificate = dsCertificate.Where(x => x.CourseId == courseId.Value).ToList();
            if (!string.IsNullOrEmpty(searchTerm))
                dsCertificate = dsCertificate.Where(x =>
                    (x.Fullname?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            ViewBag.SearchTerm = searchTerm;
            List<CCourse> courses = CXuLy.getDSCourse() ?? new List<CCourse>(); 
            ViewBag.Courses = courses;
            ViewBag.CurrentCourse = courseId;

            // Phân trang
            const int pageSize = 5;
            if (page < 1) page = 1;
            int total = dsCertificate.Count;
            var paginate = new Paginate(total, page, pageSize);
            int skip = (page - 1) * pageSize;
            var data = dsCertificate.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        // Xem chi tiết chứng chỉ
        [Route("Admin/Certificate/viewDetail/{id}")]
        public IActionResult viewDetail(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var certificate = CXuLy.GetCertificateById(id, token);
            if (certificate == null)
            {
                TempData["error"] = "Không tìm thấy chứng chỉ!";
                return RedirectToAction("Index");
            }
            return View(certificate);
        }
        // kiểm tra chứng chỉ treal hay không
        [HttpPost]
        [Route("Admin/Certificate/VerifyCertificate")]
        public JsonResult VerifyCertificate([FromBody] VerifyRequest request)
        {
            var result = CXuLy.VerifyCertificate(request.VerifyCode);
            return Json(result);
        }

        // Xóa chứng chỉ
        [Route("Admin/Certificate/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.DeleteCertificate(id, token))
            {
                TempData["success"] = "Xóa chứng chỉ thành công!";
            }
            else
            {
                TempData["error"] = "Xóa chứng chỉ thất bại!";
            }
            return RedirectToAction("Index");
        }
        // Xóa nhiều chứng chỉ 

        [Route("Admin/Certificate/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.DeleteCertificate(id, token))
                    successCount++;
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Đã xóa thành công {successCount} chứng chỉ.";
                return Json(new { success = true, message = $"Đã xóa thành công {successCount} chứng chỉ." });
            }
            else
            {
                TempData["error"] = "Không có chứng chỉ nào được xóa.";
                return Json(new { success = false, message = $"Đã xóa thất bại." });
            }
        }

    }
}
