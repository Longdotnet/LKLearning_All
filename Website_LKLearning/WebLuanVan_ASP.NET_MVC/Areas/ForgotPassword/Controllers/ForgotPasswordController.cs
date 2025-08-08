using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Areas.ForgotPassword.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.ForgotPassword.Controllers
{
    [Area("ForgotPassword")]
    public class ForgotPasswordController : BaseController
    {
        // Step 1: Handle OTP request
        [Route("quenmatkhau")]
        public IActionResult ForgotPassword()
        {

            return View();
        }
        [HttpPost]
        [Route("quenmatkhau")]

        public async Task<IActionResult> SendForgotPassword(string Contact, string Method)
        {
            var dto = new CForgotPassword();
            if (Contact.Contains("@"))
            {
                dto.Email = Contact;
                dto.Phone = null;
            }
            else
            {
                dto.Phone = Contact;
                dto.Email = null;
            }
            dto.Method = Method;

            var result = await CXuLy.SendOtpAsync(dto);
            // You can parse result for error/success, here we assume success
            TempData["Contact"] = Contact;
            TempData["Method"] = Method;
            return RedirectToAction("Otp");
        }

        // Step 3: Show OTP input form
        [Route("Otp")]
        public IActionResult Otp()
        {
            ViewBag.Contact = TempData["Contact"];
            ViewBag.Method = TempData["Method"];
            return View();
        }

        // Step 4: Handle OTP verification
        [HttpPost]
        [Route("Otp")]

        public async Task<IActionResult> VerifyOtp(string Contact, string Method, string Otp)
        {
            var dto = new CVerifyOtp();
            if (Contact.Contains("@"))
            {
                dto.Email = Contact;
                dto.Phone = null;
            }
            else
            {
                dto.Phone = Contact;
                dto.Email = null;
            }
            dto.Otp = Otp;

            bool isValid = await CXuLy.VerifyOtpAsync(dto);
            if (!isValid)
            {
                ViewBag.Contact = Contact;
                ViewBag.Method = Method;
                ViewBag.Error = "OTP không hợp lệ hoặc đã hết hạn.";
                return View("Otp");
            }

            // OTP valid, go to reset password
            TempData["Contact"] = Contact;
            TempData["Method"] = Method;
            TempData["Otp"] = Otp;
            return RedirectToAction("ResetPassword");
        }

        // Step 5: Show reset password form
        [Route("resetmatkhau")]
        public IActionResult ResetPassword()
        {
            ViewBag.Contact = TempData["Contact"];
            ViewBag.Method = TempData["Method"];
            ViewBag.Otp = TempData["Otp"];
            return View();
        }

        // Step 6: Handle password reset
        [HttpPost]
        [Route("resetmatkhau")]
        public async Task<IActionResult> ResetPassword(string Contact, string Otp, string NewPassword, string ConfirmPassword)
        {
            var dto = new CResetPassword();
            if (Contact.Contains("@"))
            {
                dto.Email = Contact;
                dto.Phone = null;
            }
            else
            {
                dto.Phone = Contact;
                dto.Email = null;
            }
            dto.Otp = Otp;
            dto.NewPassword = NewPassword;
            dto.ConfirmPassword = ConfirmPassword;

            var result = await CXuLy.ResetPasswordAsync(dto);
            // You can parse result for error/success
            TempData["success"] = result;
            return RedirectToAction("Index", "LogIn", new { area = "Login" });
        }
    }
}
