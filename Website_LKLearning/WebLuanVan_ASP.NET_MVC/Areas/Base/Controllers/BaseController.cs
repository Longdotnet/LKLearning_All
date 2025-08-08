using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers
{
    public class BaseController : Controller
    {
        protected UserProfile GetCurrentUser()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var userProfile = CXuLy.GetUserInformation(token);
                return userProfile;
            }
            catch
            {
                // Log error if needed
                return null;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.UserProfile = GetCurrentUser();
            base.OnActionExecuting(context);
        }
    }
}
