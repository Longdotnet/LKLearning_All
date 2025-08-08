using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CertificateTypeController : BaseController
    {
        [Route("Admin/CertificateType")]
        public IActionResult Index(int page)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var dsTypes = CXuLy.GetListCertificateTypes(token);
            // Phân trang
            const int pageSize = 5;
            if (page < 1) page = 1;
            int total = dsTypes.Count;
            var paginate = new Paginate(total, page, pageSize);
            int skip = (page - 1) * pageSize;
            var data = dsTypes.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }
        // ================== CertificateType ==================

        // Thêm loại chứng chỉ (GET)
        [HttpGet]
        [Route("Admin/CertificateType/formAddType")]
        public IActionResult formAddType()
        {
            return View();
        }

        // Thêm loại chứng chỉ (POST)
        [HttpPost]
        [Route("Admin/CertificateType/formAddType")]
        public IActionResult AddType(CCertificateType model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.AddCertificateType(model, token))
            {
                TempData["success"] = "Thêm loại chứng chỉ thành công!";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Thêm loại chứng chỉ thất bại!";
            return View(model);
        }

        // Sửa loại chứng chỉ (GET)
        [HttpGet]
        [Route("Admin/CertificateType/formEditType/{id}")]
        public IActionResult formEditType(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var type = CXuLy.GetCertificateTypeById(id, token);
            if (type == null)
            {
                TempData["error"] = "Không tìm thấy loại chứng chỉ!";
                return RedirectToAction("Index");
            }
            return View(type);
        }

        // Sửa loại chứng chỉ (POST)
        [HttpPost]
        [Route("Admin/CertificateType/formEditType/{id}")]
        public IActionResult EditType(int id, CCertificateType model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.UpdateCertificateType(id, model, token))
            {
                TempData["success"] = "Cập nhật loại chứng chỉ thành công!";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Cập nhật loại chứng chỉ thất bại!";
            return View(model);
        }

        // Xóa loại chứng chỉ
        [Route("Admin/CertificateType/DeleteType/{id}")]
        public IActionResult DeleteType(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.DeleteCertificateType(id, token))
            {
                TempData["success"] = "Xóa loại chứng chỉ thành công!";
            }
            else
            {
                TempData["error"] = "Xóa loại chứng chỉ thất bại!";
            }
            return RedirectToAction("Index");
        }
        // Xóa nhiều loại chứng chỉ

        [Route("Admin/CertificateType/DeleteMultipleType")]
        public IActionResult DeleteMultipleType([FromBody] List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int succes = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.DeleteCertificateType(id, token))
                {
                    succes++;
                }
            }
            if (succes > 0)
            {
                TempData["success"] = $"Đã xóa {succes} loại chứng chỉ thành công!";
                return Json(new { success = true, message = $"Đã xóa {succes} loại chứng chỉ thành công!" });

            }
            else
            {
                TempData["error"] = "Xóa loại chứng chỉ thất bại!";
                return Json(new { success = false, message = "Xóa loại chứng chỉ thất bại!" });

            }
        }
    }
}
