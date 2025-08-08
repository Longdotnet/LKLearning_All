using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Controllers
{
    [Area("UpdateProfile")]
    public class UpdateProfileController : BaseController
    {
        [Route("updateprofile")]
        public IActionResult Index()
        {
            // Lấy thông tin người dùng từ Session
            string token = HttpContext.Session.GetString("AuthToken");
            UserProfile x = UpdateProfile.Models.CXuLy.GetUserInformation(token);
            ViewBag.Categories = Admin.Models.CXuLy.getDSCategory() ?? new List<CCategory>();



            return View(x);
        }

        [HttpPost]
        [Route("updateprofile")]
        public  IActionResult Index(UserProfile x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (UpdateProfile.Models.CXuLy.UpdateUserProfile(x,token))
                {
                  
                    TempData["success"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Cập nhật không thành công!";
                    return Content("Sửa không thành công");
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }

        [Route("updateprofile/delete")]
        public IActionResult Delete()
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (UpdateProfile.Models.CXuLy.DeleteUserProfile(token))
                {
                    TempData["success"] = "Xóa tài khoản thành công!";
                    return RedirectToAction("Index", "Dashboard", new { area = "User" });
                }
                else
                {
                    TempData["error"] = "Xóa không thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }
        }
        [Route("updateprofile/changepassword")]
        public IActionResult ChangePassword()
        {
            ViewBag.Categories = Admin.Models.CXuLy.getDSCategory() ?? new List<CCategory>();

            return View();
        }

        [HttpPost]
        [Route("updateprofile/changepassword")]
        public IActionResult ChangePassword(CChangePassword model)
        {
            string token = HttpContext.Session.GetString("AuthToken");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (UpdateProfile.Models.CXuLy.ChangePassword(model,token))
                {
                    TempData["success"] = "Cập nhật mật khẩu thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Mật khẩu hiện tại không đúng hoặc có lỗi xảy ra";
                    return View(model);
                }
            }
            catch
            {
                TempData["error"] = "Đã xảy ra lỗi khi thay đổi mật khẩu";
                return View(model);
            }
        }
    }
    
}
