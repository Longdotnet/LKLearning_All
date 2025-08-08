using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChatController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("Admin/ViewUser")]
        // Hiển thị danh sách user mà admin đã từng nhắn tin
        public async Task<IActionResult> ChatUsers()
        {
            // Lấy token xác thực từ session
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "LogIn", new { area = "Login" });

            // Lấy thông tin admin hiện tại
            var admin = CXuLy.LayThongTinUser(token);
            if (admin == null)
                return RedirectToAction("Index", "LogIn", new { area = "Login" });

            // Lấy danh sách các cuộc trò chuyện của admin
            var conversations = await CXuLy.GetAdminConversationsAsync(admin.UserId, token);

            // Lấy danh sách user từ các cuộc trò chuyện (loại bỏ trùng lặp)
            var users = conversations
                .GroupBy(c => c.UserID)
                .Select(g => new
                {
                    UserId = g.First().UserID,
                    Fullname = g.First().UserName,
                })
                .ToList();

            ViewBag.AdminId = admin.UserId;
            return View("ChatUsers", users); // Tạo view mới ChatUsers.cshtml
        }
        // Hiển thị khung chat với user đã chọn
        public async Task<IActionResult> ChatWithUser(int userId)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "LogIn", new { area = "Login" });

            var admin = CXuLy.LayThongTinUser(token);
            if (admin == null)
                return RedirectToAction("Index", "LogIn", new { area = "Login" });

            // Lấy danh sách các cuộc trò chuyện của admin
            var conversations = await CXuLy.GetAdminConversationsAsync(admin.UserId, token);
            var users = conversations
                .GroupBy(c => c.UserID)
                .Select(g => new COnlineUserInfo
                {
                    UserId = g.First().UserID,
                    Fullname = g.First().UserName
                })
                .ToList();

            // Tìm conversation giữa admin và user này
            var conversation = await CXuLy.GetConversationBetweenAdminAndUserAsync(admin.UserId, userId, token);
            if (conversation == null)
            {
                conversation = await CXuLy.CreateConversationAsync(new CConversation
                {
                    AdminID = admin.UserId,
                    UserID = userId
                }, token);
            }

            // Lấy danh sách tin nhắn
            var messages = await CXuLy.GetMessagesAsync(conversation.ConversationID, token);

            // Truyền users và user đang chọn qua ViewBag
            ViewBag.Users = users;
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedUserName = users.FirstOrDefault(u => u.UserId == userId)?.Fullname ?? "";
            ViewBag.ConversationId = conversation.ConversationID;
            ViewBag.UserId = admin.UserId;
            ViewBag.SenderName = admin.Fullname;

            return View("Chat", messages); // Model là IEnumerable<CMessage>
        }



        public async Task<IActionResult> Conversations()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var user = CXuLy.LayThongTinUser(token);
            ViewBag.UserId = user.UserId;
            var conversations = await CXuLy.GetAdminConversationsAsync(user.UserId, token);
        
            return View(conversations);
        }

        public async Task<IActionResult> Chat(int conversationId)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var messages = await CXuLy.GetMessagesAsync(conversationId, token);
            ViewBag.ConversationId = conversationId;

            var user = CXuLy.LayThongTinUser(token);
            ViewBag.UserId = user.UserId; // ID Admin
            ViewBag.SenderName = user.Fullname; // Thêm dòng này để JS lấy đúng tên
            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;
            return View(messages);
        }


        [HttpPost]
        public async Task<IActionResult> CreateConversation(int adminId, int userId)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var conversation = new CConversation
            {
                AdminID = adminId,
                UserID = userId
            };
            var result = await CXuLy.CreateConversationAsync(conversation, token);
            if (result != null)
            {
                return RedirectToAction("Chat", new { conversationId = result.ConversationID, adminId });
            }
            return RedirectToAction("Conversations", new { adminId });
        }

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
            var success = await CXuLy.SendMessageAsync(message, token);
            if (success)
            {
                return Ok();
            }
            return BadRequest("Không thể gửi tin nhắn.");
        }


    }
}