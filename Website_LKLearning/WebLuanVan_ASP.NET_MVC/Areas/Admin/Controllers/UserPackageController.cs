using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using System.Linq;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserPackageController : BaseController
    {
        [Route("userpackage")]
        public IActionResult Index(int page = 1, string? searchTerm = null)
        {
            string? token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var userPackages = CXuLy.getDSUserPackage();
            var users = CXuLy.getDSUsers();
            var packages = CXuLy.getDSPackage();

            // Join data to include user and package names
            // mục đích userPackageList 
            var userPackageList = from up in userPackages
                                  join u in users on up.UserId equals u.UserId
                                  join p in packages on up.PackageId equals p.PackageId
                                  where  p.PackageName != "FREE" // Loại bỏ gói FREE
                                  select new CUserPackageViewModel
                                  {
                                      UserId = up.UserId,
                                      PackageId = up.PackageId,
                                      Fullname = $"{u.Fullname ?? "Không xác định"}",
                                      Email=$"{u.Email ??"Chưa có"}",
                                      Phone=$"{u.Phone ?? "Chưa có"}",
                                      PackageName = p.PackageName,
                                      RegistrationDate = up.RegistrationDate,
                                      ExpirationDateDisplay = up.ExpirationDate.HasValue
                                          ? up.ExpirationDate.Value.ToString("dd/MM/yyyy")
                                          : "vĩnh viễn",
                                    
                                  };

            

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                userPackageList = userPackageList.Where(up =>
                    (up.Fullname?.ToLower().Contains(searchTerm) ?? false) ||
                    (up.Phone?.Contains(searchTerm)??false)||
                    (up.Email?.ToLower().Contains(searchTerm)??false)||
                    (up.PackageName?.ToLower().Contains(searchTerm) ?? false));
            }

            // Pagination
            const int pageSize = 5;
            if (page < 1) page = 1;
            int totalItems = userPackageList.Count();
            var paginate = new Paginate(totalItems, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = userPackageList.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            ViewBag.SearchTerm = searchTerm;

            return View(data);
        }
        [HttpGet]
        [Route("Admin/UserPackage/GetUserDetail/{id}")]
        public IActionResult GetUserDetail(int id)
        {
        string token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
        }

        var user = CXuLy.getUserById(id.ToString(), token);
        var userpackages= CXuLy.getPackagesByUserId(id.ToString());  
        ViewBag.UserPackages = userpackages;
        if (user == null)
        {
            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }

            return PartialView("~/Areas/Admin/Views/Shared/_UserDetail.cshtml", user);
        
        }
       
        [HttpGet]
        [Route("Admin/UserPackage/GetPackageDetail/{id}")]
        public IActionResult GetPackageDetail(int id)
    {
        string token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
        }

        var package = CXuLy.getPackageById(id.ToString());
        if (package == null)
        {
            return Json(new { success = false, message = "Không tìm thấy gói cước." });
        }

            return PartialView("~/Areas/Admin/Views/Shared/_PackageDetail.cshtml", package);
        }
        [Route("formUpdateUserPackage/{userId}/{packageId}")]
        public IActionResult formUpdateUserPackage(int userId, int packageId)
        {
            string? token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var users = CXuLy.getDSUsers()?.Where(u => !string.IsNullOrEmpty(u.Fullname) && !string.IsNullOrEmpty(u.Email)).ToList() ?? new List<CUsers>();
            var packages = CXuLy.getDSPackage()?.Where(p => p.PackageName != "FREE").ToList() ?? new List<CPackage>();
            var userPackage = CXuLy.GetPackageByUserIdAndByPackageId(userId.ToString(), packageId.ToString());

            if (userPackage == null)
            {
                TempData["error"] = "Không tìm thấy đăng ký gói cước.";
                return RedirectToAction("Index");
            }

            ViewBag.Users = users;
            ViewBag.Packages = packages;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentPackageId = packageId;
            ViewBag.SelectedUser = CXuLy.getUserById(userId.ToString(), token);
            ViewBag.SelectedPackage = CXuLy.getPackageById(packageId.ToString());
            ViewBag.AuthToken = token;

            return View(userPackage);
        }
        [HttpGet]
        [Route("Admin/UserPackage/GetPackageById/{id}")]
        public IActionResult GetPackageById(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            var package = CXuLy.getPackageById(id.ToString());
            if (package == null)
            {
                return Json(new { success = false, message = "Không tìm thấy gói cước." });
            }

            return Json(new
            {
                success = true,
                packageId = package.PackageId,
                packageName = package.PackageName,
                durationDay = package.DurationDay,
                price = package.Price,
                urlImage = package.UrlImage,
                includedPackageIds = package.IncludedPackageIds
            });
        }
        [HttpPost]
        [Route("formUpdateUserPackage")]
        public async Task<IActionResult> UpdateUserPackage(CUserPackage model, int OriginalUserId, int OriginalPackageId)
        {
            string? token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["error"] = string.Join("; ", errors);
                return RedirectToAction("formUpdateUserPackage", new { userId = OriginalUserId, packageId = OriginalPackageId });
            }

            try
            {
                // Kiểm tra gói cước mới có tồn tại không
                var package = CXuLy.getPackageById(model.PackageId.ToString());
                if (package == null)
                {
                    TempData["error"] = "Gói cước không tồn tại.";
                    return RedirectToAction("formUpdateUserPackage", new { userId = OriginalUserId, packageId = OriginalPackageId });
                }

                // Tính ExpirationDate dựa trên DurationDay
                DateTime? expirationDate = null;
                if (package.DurationDay.HasValue && package.DurationDay.Value > 0)
                {
                    expirationDate = model.RegistrationDate.AddDays(package.DurationDay.Value);
                }

                // Cập nhật thông tin
                var updatedModel = new CUserPackage
                {
                    UserId = OriginalUserId,
                    PackageId = model.PackageId,
                    RegistrationDate = model.RegistrationDate,
                    ExpirationDate = expirationDate
                };

                bool success = CXuLy.updateUserPackage(OriginalUserId, OriginalPackageId, updatedModel, token);
                if (success)
                {
                    TempData["success"] = "Cập nhật đăng ký gói cước thành công!";
                }
                else
                {
                    TempData["error"] = "Cập nhật không thành công. Vui lòng thử lại.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("formUpdateUserPackage", new { userId = OriginalUserId, packageId = OriginalPackageId });
            }
        }


        [Route("formCreateUserPackage")]
        public IActionResult formCreateUserPackage(CUserPackage x, UserPackageKey id)
        {
            string? token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var users = CXuLy.getDSUsers()?.Where(u => !string.IsNullOrEmpty(u.Fullname) && !string.IsNullOrEmpty(u.Email)).ToList() ?? new List<CUsers>();
            var packages = CXuLy.getDSPackage()?.Where(p => p.PackageName != "FREE").ToList() ?? new List<CPackage>();
        
            // Pre-fetch chi tiết
            CUsers? selectedUser = id.UserId > 0 ? CXuLy.getUserById(id.UserId.ToString(), token) : null;
            CPackage? selectedPackage = id.PackageId > 0 ? CXuLy.getPackageById(id.PackageId.ToString()) : null;

           
            ViewBag.Users = users;
            ViewBag.Packages = packages;
      
            ViewBag.CurrentUserId = id.UserId;
            ViewBag.CurrentPackageId = id.PackageId;
            ViewBag.SelectedUser = selectedUser;
            ViewBag.SelectedPackage = selectedPackage;
           
            ViewBag.AuthToken = token;
            return View();
        }

        [HttpPost]
        [Route("formCreateUserPackage")]
        public IActionResult CreateUserPackage(CUserPackage model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction("formCreateUserPackage");
            }

            try
            {
                // Kiểm tra xem userId và packageId có hợp lệ không
                var user = CXuLy.getUserById(model.UserId.ToString(), token);
                var package = CXuLy.getPackageById(model.PackageId.ToString());
                if (user == null || package == null)
                {
                    TempData["error"] = "Người dùng hoặc gói cước không tồn tại.";
                    return RedirectToAction("formCreateUserPackage");
                }

                // Kiểm tra xem người dùng đã đăng ký gói cước này chưa
                var existingUserPackage = CXuLy.getDSUserPackage()
                    ?.FirstOrDefault(up => up.UserId == model.UserId && up.PackageId == model.PackageId);
                if (existingUserPackage != null)
                {
                    TempData["error"] = "Người dùng đã đăng ký gói cước này.";
                    return RedirectToAction("formCreateUserPackage");
                }

                // Gọi API để thêm gói cước
                bool success = CXuLy.themUserPackage(model, token);
                if (success)
                {
                    TempData["success"] = "Thêm gói cước cho người dùng thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm gói cước không thành công. Vui lòng thử lại.";
                    return RedirectToAction("formCreateUserPackage");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("formCreateUserPackage");
            }
        }

        [HttpGet]
        public IActionResult searchUser(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var users = CXuLy.getDSUsers() ?? new List<CUsers>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    (!string.IsNullOrEmpty(u.Fullname) && u.Fullname.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.Phone) && u.Phone.ToLower().Contains(searchTerm))
                ).ToList();
            }

            var result = users.Select(u => new
            {
                id = u.UserId,
                text = $"{u.Fullname ?? "Không xác định"} ({u.Email ?? "Không có email"})"
            }).ToList();

            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }

        [HttpGet]
        public IActionResult searchPackage(string searchTerm)
        {
            var packages = CXuLy.getDSPackage() ?? new List<CPackage>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                packages = packages.Where(p =>
                    !string.IsNullOrEmpty(p.PackageName) && p.PackageName.ToLower().Contains(searchTerm)
                ).ToList();
            }

            var result = packages.Select(p => new
            {
                id = p.PackageId,
                text = p.PackageName ?? "Không xác định"
            }).ToList();

            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }

        [Route("delete")]
        public IActionResult xoaUserPackage(int packageId, int userId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.xoaUserPackage(userId, packageId, token))
            {
                TempData["success"] = "Xóa thành công";
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
            }
            return RedirectToAction("Index");
        }

        [Route("DeleteMultiple")]
        public IActionResult DeleteMultiple([FromBody] List<UserPackageKey> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đăng ký để xóa." });
            }

            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.xoaUserPackage(id.UserId, id.PackageId, token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = $"{successCount} đăng ký đã được xóa thành công.";
                    return Json(new { success = true });
                }
                else
                {
                    TempData["error"] = "Không có đăng ký nào được xóa. Vui lòng kiểm tra lại.";
                    return Json(new { success = false, message = "Không có đăng ký nào được xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}