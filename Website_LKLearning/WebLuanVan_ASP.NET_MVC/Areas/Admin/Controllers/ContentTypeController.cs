using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContentTypeController : BaseController
    {
        [Route("content-type")]
        public IActionResult Index(int page, string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CContentType> dsContentType = CXuLy.getDSContentType();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsContentType = dsContentType.Where(u =>
                    (u.TypeDescription?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.TypeName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList(); // Fix: Added closing parenthesis for the Where clause  
            }
            ViewBag.SearchTerm = searchTerm;
            // Phân trang  
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsContentType.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsContentType.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }
        [Route("content-type/formThemcontent-type")]
        public IActionResult formThemContentType()
        {
            return View();
        }
        [HttpPost]
        [Route("content-type/formThemcontent-type")]
        public IActionResult ThemContentType(CContentType x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.themContentType(x, token))
            {
                TempData["success"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thêm không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("content-type/formSuaContentType/{id}")]
        public IActionResult formSuaContentType(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CContentType contentType = CXuLy.getContentTypeById(id);
            if (contentType == null)
            {
                return NotFound();
            }
            return View(contentType);
        }
        [HttpPost]
        [Route("content-type/formSuaContentType/{id}")]
        public IActionResult SuaContentType(CContentType x, string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.suaContentType(id, x, token))
            {
                TempData["success"] = "Sửa thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Sửa không thành công";
                return RedirectToAction("Index");

            }
        }
        [Route("content-type/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.xoaContentType(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} chủ đề câu hỏi";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["success"] = $"Xóa không thành công";
                return RedirectToAction("Index");
            }
        }
        [Route("content-type/xoa/{id}")]
        public IActionResult xoaContentType(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.xoaContentType(id, token))
            {
                TempData["success"] = "Xóa thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }
        [Route("content-type/viewDetail/{id}")]
        public IActionResult viewDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CContentType contentType = CXuLy.getContentTypeById(id);
            if (contentType == null)
            {
                return NotFound();
            }
            return View(contentType);
        }
    }
}
