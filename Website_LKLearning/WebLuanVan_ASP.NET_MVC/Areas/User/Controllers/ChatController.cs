using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class ChatController : BaseController
    {
        public IActionResult Index()
        {

               var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;
            return View();
        }

        // Hiển thị danh sách cuộc trò chuyện của user


        // Hiển thị chi tiết cuộc trò chuyện (tin nhắn)
        public async Task<IActionResult> Chat(int conversationId, int userId)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var messages = await Admin.Models.CXuLy.GetMessagesAsync(conversationId, token);
            ViewBag.ConversationId = conversationId;
            ViewBag.UserId = userId;

            // Lấy tên user hiện tại
            var user = Admin.Models.CXuLy.LayThongTinUser(token);
            ViewBag.SenderName = user?.Fullname ?? "";
            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;
            return View(messages);
        }

        // Gửi tin nhắn mới
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageModel model)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var message = new CMessage
            {
                ConversationID = model.ConversationId,
                SenderID = model.SenderId,
                SenderName = model.SenderName,
                Content = model.Content
            };
            var success = await Admin.Models.CXuLy.SendMessageAsync(message, token);
            if (success)
            {
                return Ok();
            }
            return BadRequest("Không thể gửi tin nhắn.");
        }

        // Lấy hoặc tạo cuộc trò chuyện mới (nếu chưa có)
        [HttpPost]
        public async Task<IActionResult> GetOrCreateConversation()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var user = Admin.Models.CXuLy.LayThongTinUser(token);
            if (user == null)
                return Json(new { error = "Chưa đăng nhập" });
            int userId = user.UserId;
            // Lấy danh sách cuộc trò chuyện
            var conversations = await Admin.Models.CXuLy.UserCreateConversationAsync( token);
            int conversationId = 0;
            if (conversations != null )
            {
                conversationId = conversations.ConversationID;
            }
            else
            {
                // Tạo cuộc trò chuyện mới
                var created = await Admin.Models.CXuLy.UserCreateConversationAsync(token);
                if (created != null)
                    conversationId = created.ConversationID;
            }
            if (conversationId == 0)
                return Json(new { error = "Không thể tạo cuộc trò chuyện" });

            return Json(new { conversationId = conversationId, userId = user.UserId });
        }

    }
}