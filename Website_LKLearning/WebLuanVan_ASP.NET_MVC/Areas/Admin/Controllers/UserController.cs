using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : BaseController
    {
        [Route("Admin/user")]
        public IActionResult Index(int page, int? roleId, string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CUsers> ds = CXuLy.getDSUsers();

            // Áp dụng filter nếu có  
            if (roleId.HasValue)
            {
                ds = ds.Where(u => u.RoleId == roleId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ds = ds.Where(u =>
                    (u.Fullname?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Phone?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList(); // Fix: Added closing parenthesis for the Where clause  
            }

            // Lấy danh sách roles (giả sử có hàm getDSRole)  

            ViewBag.CurrentRole = roleId;
            ViewBag.SearchTerm = searchTerm;

            // Phân trang  
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = ds.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = ds.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        [Route("Admin/user/DeleteMultiple")]
        [HttpPost]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.xoaUserByAdmin(id.ToString(), token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = $"Đã xóa thành công {successCount} người dùng";
                }
                else
                {
                    TempData["error"] = "Không có người dùng nào được xóa.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [Route("Admin/user/xoauser/{id}")]
        public IActionResult xoaUser(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaUserByAdmin(id, token))
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
        [Route("Admin/user/formThemUser")]
        public IActionResult formThemUser(int roleId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            ViewBag.Roles = CXuLy.getRole(token);
            ViewBag.CurrentRole = roleId;
            return View();
        }
        [HttpPost]
        [Route("Admin/user/formThemUser")]
        public IActionResult ThemUser(CUsers x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themUser(x, token))
                {
                    TempData["success"] = "Thêm người dùng thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm người dùng không thành công";
                    return RedirectToAction("formThemUser");
                }
            }
            catch
            {
                TempData["error"] = "Thêm người dùng không thành công";
                return RedirectToAction("formThemUser");
            }
        }
        [Route("Admin/user/formSuaUser/{id}")]
        public IActionResult formSuaUser(int roleId, string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CUsers user = CXuLy.getUserById(id, token);

            // Kiểm tra nếu user hoặc roles là null
            if (user == null || CXuLy.getRole(token) == null)
            {
                TempData["error"] = "Không tìm thấy dữ liệu";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = CXuLy.getRole(token); // Đảm bảo gán giá trị
            ViewBag.CurrentRole = roleId;
            return View(user); // Truyền model user vào view
        }
        [HttpPost]
        [Route("Admin/user/formSuaUser/{id}")]
        public IActionResult SuaUser(string id, CUsers x)
        {
            string token = HttpContext.Session.GetString("AuthToken");


            try
            {
                if (CXuLy.editUserById(id, x, token))
                {
                    TempData["success"] = "Sửa người dùng thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Sửa người dùng không thành công";
                    return RedirectToAction("formSuaUser", new { id = x.UserId });
                }
            }
            catch
            {
                TempData["error"] = "Sửa người dùng không thành công";
                return RedirectToAction("formSuaUser", new { id = x.UserId });
            }
        }
        [Route("Admin/user/viewUserDetail/{id}")]
        public IActionResult viewUserDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            ViewBag.Roles = CXuLy.getRole(token);
            CUsers user = CXuLy.getUserById(id, token);
            // Lấy danh sách khóa học, đơn hàng, hoạt động
            List<COrders> userOrders = CXuLy.GetDSOrderByUserId(token);
            ViewBag.UserOrders = userOrders;
            // Lấy danh sách gói cước để hiển thị tên gói / Fetch packages to display package names
            ViewBag.Packages = CXuLy.getDSPackage()?.ToList();

            // Lấy danh sách phương thức thanh toán / Fetch payment methods
            ViewBag.PaymentMethods = CXuLy.getDSPaymentMethod(token)?.ToList();
            // Kiểm tra nếu user là null
            if (user == null)
            {
                TempData["error"] = "Không tìm thấy người dùng";
                return RedirectToAction("Index");
            }
       

            return View(user); // Truyền model user vào view
        }
    }
}
