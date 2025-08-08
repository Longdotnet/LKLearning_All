using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentMethodController : BaseController
    {
        [Route("paymentmethod")]
        public IActionResult Index(int page = 1, string searchTerm = "")
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var dsPaymentMethod = CXuLy.getDSPaymentMethod(token) ?? new List<CPaymentMethod>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsPaymentMethod = dsPaymentMethod
                    .Where(pm => (pm.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            ViewBag.SearchTerm = searchTerm;

            // Pagination
            const int pageSize = 5;
            if (page < 1) page = 1;
            int totalItems = dsPaymentMethod.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsPaymentMethod.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        [Route("paymentmethod/create")]
        public IActionResult formThemPaymentMethod()
        {
            return View();
        }

        [HttpPost]
        [Route("paymentmethod/create")]
        public IActionResult ThemPaymentMethod(CPaymentMethod x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.AddPaymentMethod(x, token))
                {
                    TempData["success"] = "Thêm phương thức thanh toán thành công";
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

        [Route("paymentmethod/edit/{id}")]
        public IActionResult formEditPaymentMethod(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var dsPaymentMethod = CXuLy.getDSPaymentMethod(token) ?? new List<CPaymentMethod>();
            var x = dsPaymentMethod.FirstOrDefault(pm => pm.PaymentMethodId == id);
            if (x == null)
            {
                return NotFound();
            }
            return View(x);
        }

        [HttpPost]
        [Route("paymentmethod/edit/{id}")]
        public IActionResult EditPaymentMethod(int id, CPaymentMethod x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            x.PaymentMethodId = id;
            try
            {
                if (CXuLy.UpdatePaymentMethod(x, token))
                {
                    TempData["success"] = "Sửa phương thức thanh toán thành công";
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

        [Route("paymentmethod/delete/{id}")]
        public IActionResult DeletePaymentMethod(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.DeletePaymentMethod(id.ToString(), token))
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

        [Route("paymentmethod/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.DeletePaymentMethod(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} phương thức";
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
