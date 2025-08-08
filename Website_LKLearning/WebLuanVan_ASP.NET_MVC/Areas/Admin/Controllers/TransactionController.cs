using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TransactionController : BaseController
    {
        [Route("transaction")]
        public IActionResult Index(int page, int? packageId, DateTime? createdAt, int? paymentMethod, string searchTerm)
        {
            string? token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                // Handle missing token, e.g., redirect to login or return unauthorized
                return RedirectToAction("Login", "Account");
            }

            List<COrders> dsTransaction = CXuLy.getDSOrders(token);
            List<CUsers> dsUsers = CXuLy.getDSUsers();

            if (packageId.HasValue)
            {
                dsTransaction = dsTransaction.Where(t => t.PackageId == packageId.Value).ToList();
            }
            if (paymentMethod.HasValue)
            {
                dsTransaction = dsTransaction.Where(t => t.PaymentMethodId == paymentMethod.Value).ToList();
            }
            if (createdAt.HasValue)
            {
                dsTransaction = dsTransaction.Where(t => t.CreatedAt.Date == createdAt.Value.Date).ToList();
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsTransaction = dsTransaction.Where(order =>
                {
                    var user = dsUsers.FirstOrDefault(u => u.UserId == order.UserId);
                    return (user?.Fullname?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (user?.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (user?.Phone?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false);
                }).ToList();
            }
            // Get only dates that exist 
            ViewBag.AvailableDates = dsTransaction
                    .Select(l => DateOnly.FromDateTime(l.CreatedAt))
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();            // lấy danh sách hiển thị để chon filter 
            ViewBag.Packages= CXuLy.getDSPackage()?.Where(t => t.PackageName != "FREE").ToList();
            ViewBag.PaymentMethods = CXuLy.getDSPaymentMethod(token)?.ToList();
            ViewBag.Users=CXuLy.getDSUsers()?.ToList();
            // filter
            ViewBag.CurrentPackages = packageId;
            ViewBag.CurrentPaymentMethods = paymentMethod;
            ViewBag.CurrentDate = createdAt.HasValue ? DateOnly.FromDateTime(createdAt.Value) : (DateOnly?)null;
            ViewBag.SearchTerm = searchTerm;

            const int pageSize = 5;
            if (page < 1)
            {
                page = 1;
            }

            int dem = dsTransaction.Count;
            var paginatenew = new Paginate(dem, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = dsTransaction.Skip(recSkip).Take(pageSize).ToList();
            ViewBag.Paginate = paginatenew;
            return View(data);
        }
        [Route("transaction/xoaTransaction/{id}")]
        public IActionResult xoaTransaction(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.deleteOrders(id, token))
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
                return Content("Xóa không thành công");
            }
        }
        [HttpPost]
        [Route("transaction/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<string> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // print selectedIds
            if (selectedIds == null || selectedIds.Count == 0)
            {
                TempData["error"] = "Không có bài học nào được chọn để xóa.";
                return RedirectToAction("Index");
            }
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.deleteOrders(id, token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = "Xóa thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Không có bài học nào được xóa.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult searchPackage(string searchTerm)
        {

            List<CPackage> packages = CXuLy.getDSPackage();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                packages = packages.Where(c => c.PackageName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = packages.Select(c => new
            {
                id = c.PackageId,
                text = c.PackageName
            }).ToList();
            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
        [HttpGet]
        public IActionResult searchPaymentMethod(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CPaymentMethod> paymentMethods = CXuLy.getDSPaymentMethod(token);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                paymentMethods = paymentMethods.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = paymentMethods.Select(c => new
            {
                id = c.PaymentMethodId,
                text = c.Name
            }).ToList();
            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
    }
}
