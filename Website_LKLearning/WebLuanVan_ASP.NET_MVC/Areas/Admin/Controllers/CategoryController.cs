using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : BaseController
    {
        [Route("category")]
        public IActionResult Index(int page, string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CCategory> dsCategory = CXuLy.getDSCategory();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsCategory = dsCategory.Where(u =>
                    (u.CategoryName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            ViewBag.SearchTerm = searchTerm;

            // Pagination
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsCategory.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsCategory.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        [Route("category/create")]
        public IActionResult formThemCategory()
        {
            return View();
        }

        [HttpPost]
        [Route("category/create")]
        public IActionResult ThemCategory(CCategory x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themCategory(x, token))
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
            catch
            {
                TempData["error"] = "Thêm không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("category/edit/{id}")]
        public IActionResult formEditCategory(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CCategory x = CXuLy.getCategoryById(id);
            if (x == null)
            {
                return NotFound();
            }
            return View(x);
        }

        [HttpPost]
        [Route("category/edit/{id}")]
        public IActionResult EditCategory(string id, CCategory x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.editCategory(id, x, token))
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
            catch
            {
                TempData["error"] = "Sửa không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("category/xoacategory/{id}")]
        public IActionResult xoaCategory(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaCategory(id, token))
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
            catch
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("category/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.xoaCategory(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} danh mục";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }

        [Route("category/viewDetail/{id}")]
        public IActionResult viewDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CCategory category = CXuLy.getCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}