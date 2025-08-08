using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PackageController : BaseController
    {
        [Route("package")]
        public IActionResult Index(int page, string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CPackage> dsPackage = CXuLy.getDSPackage().Where(t=>t.PackageName!="FREE").ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsPackage = dsPackage.Where(u =>
                    (u.PackageName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            ViewBag.SearchTerm = searchTerm;

            // Pagination
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsPackage.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsPackage.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        [Route("package/create")]
        public IActionResult formThemPackage()
        {
            ViewBag.AllPackage = CXuLy.getDSPackage()?.Where(p => p.PackageName != "FREE").ToList();

            return View();
        }

        [HttpPost]
        [Route("package/create")]
        public IActionResult ThemPackage(CPackage x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {

                if (CXuLy.themPackage(x, token))
                {
                    Console.WriteLine($"Số lượng package bao gồm: {x.IncludedPackageIds?.Count}");
                    if (x.IncludedPackageIds != null)
                    {
                        Console.WriteLine($"Các package ID: {string.Join(", ", x.IncludedPackageIds)}");
                    }
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

        [Route("package/edit/{id}")]
        public IActionResult formEditPackage(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CPackage package = CXuLy.getPackageById(id);
            ViewBag.AllPackage = CXuLy.getDSPackage()?.Where(p => p.PackageName != "FREE").ToList();
            if (package == null)
            {
                return NotFound();
            }
            return View(package);
        }

        [HttpPost]
        [Route("package/edit/{id}")]
        public IActionResult EditPackage(string id, CPackage x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.updatePackage(id, x, token))
                {
                    Console.WriteLine($"Số lượng package bao gồm: {x.IncludedPackageIds?.Count}");
                    if (x.IncludedPackageIds != null)
                    {
                        Console.WriteLine($"Các package ID: {string.Join(", ", x.IncludedPackageIds)}");
                    }
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

        [Route("package/xoaPackage/{id}")]
        public IActionResult xoaPackage(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaPackage(id, token))
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

        [Route("package/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.xoaPackage(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} gói cước";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }

       
    }
}