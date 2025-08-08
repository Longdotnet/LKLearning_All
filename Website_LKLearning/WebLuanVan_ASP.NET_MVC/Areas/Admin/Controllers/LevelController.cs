using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LevelController : BaseController
    {
        [Route("level")]
        public IActionResult Index(int page, string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CLevel> dsLevel = CXuLy.getDSLevel();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsLevel = dsLevel.Where(u =>
                    (u.LevelName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            // Pagination
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsLevel.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsLevel.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }
        [Route("level/create")]
        public IActionResult formThemLevel(CLevel x)
        {
            return View();
        }
        [HttpPost]
        [Route("level/create")]
        public IActionResult ThemLevel(CLevel x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themLevel(x, token))
                {
                    TempData["success"] = "Thêm thành công cấp độ";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm không thành công cấp độ";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Thêm không thành công");
            }
        }
        [Route("level/xoalevel/{id}")]
        public IActionResult xoaLevel(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaLevel(id, token))
                {
                    TempData["success"] = "Xóa thành công cấp độ";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Xóa không thành công cấp độ";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }
        }
        [Route("level/edit/{id}")]
        public IActionResult formSuaLevel(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CLevel x = CXuLy.getLevelById(id );
            return View(x);
        }
        [HttpPost]
        [Route("level/edit/{id}")]
        public IActionResult SuaLevel(string id, CLevel x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.editLevel(id, x, token))
                {
                    TempData["success"] = "Sửa thành công cấp độ";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Sửa không thành công cấp độ";
                    return RedirectToAction("Index");
                    
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }
        [Route("level/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.xoaLevel(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} cấp độ";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("level/viewDetail/{id}")]
        public IActionResult viewDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CLevel level = CXuLy.getLevelById(id);
            if (level == null)
            {
                return NotFound();
            }
            return View(level);
        }

    }
}
