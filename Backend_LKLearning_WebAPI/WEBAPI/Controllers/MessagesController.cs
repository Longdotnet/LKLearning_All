using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.DTOS;
using WEBAPI.Hubs;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly LuanvantienganhContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IOnlineUserTracker _tracker;

        public MessagesController(LuanvantienganhContext context, IHubContext<ChatHub> hubContext, IOnlineUserTracker tracker)
        {
            _context = context;
            _hubContext = hubContext;
            _tracker = tracker;
        }

        // Lấy danh sách tin nhắn trong cuộc trò chuyện
        [HttpGet("conversation/{conversationId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessages(int conversationId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var conversation = await _context.Conversations
                .Where(c => c.ConversationID == conversationId && (c.AdminID == currentUserId || c.UserID == currentUserId))
                .FirstOrDefaultAsync();
            if (conversation == null)
                return NotFound("Không tìm thấy cuộc trò chuyện hoặc bạn không có quyền truy cập.");

            var messages = await _context.Messages
                .Where(m => m.ConversationID == conversationId)
                .Include(m => m.Sender)
                .Select(m => new MessageDTO
                {
                    MessageID = m.MessageID,
                    ConversationID = m.ConversationID,
                    SenderID = m.SenderID,
                    SenderName = m.Sender.Fullname,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Cập nhật trạng thái IsRead cho tin nhắn
            foreach (var message in messages.Where(m => m.SenderID != currentUserId && !m.IsRead))
            {
                var dbMessage = await _context.Messages.FindAsync(message.MessageID);
                dbMessage.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return Ok(messages);
        }

        // Gửi tin nhắn

        [HttpPost("send")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] MessageDTO messageDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId != messageDTO.SenderID)
            {
                return Unauthorized("Không có quyền gửi tin nhắn.");
            }

            var conversation = await _context.Conversations.FindAsync(messageDTO.ConversationID);
            if (conversation == null ||
                (conversation.AdminID != currentUserId && conversation.UserID != currentUserId))
                return NotFound("Không tìm thấy cuộc trò chuyện hoặc bạn không có quyền truy cập.");

            // Kiểm tra admin hiện tại có online không
            if (!_tracker.IsOnline(conversation.AdminID))
            {
                // Tìm admin online khác
                var adminRoleId = await _context.Roles.Where(r => r.RoleName == "Admin").Select(r => r.RoleId).FirstOrDefaultAsync();
                var adminIds = await _context.Users.Where(u => u.RoleId == adminRoleId).Select(u => u.UserId).ToListAsync();
                var onlineAdminIds = _tracker.GetOnlineAdmins(adminIds).Where(id => id != conversation.AdminID).ToList();

                if (onlineAdminIds.Any())
                {
                    // Chuyển hội thoại sang admin online mới
                    var newAdminId = onlineAdminIds.First(); // hoặc random nếu muốn
                    conversation.AdminID = newAdminId;
                    await _context.SaveChangesAsync();
                }
                // Nếu không có admin online khác, vẫn lưu tin nhắn như bình thường
            }

            var message = new Message
            {
                ConversationID = messageDTO.ConversationID,
                SenderID = messageDTO.SenderID,
                Content = messageDTO.Content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            messageDTO.MessageID = message.MessageID;
            messageDTO.SentAt = message.SentAt;
            messageDTO.IsRead = message.IsRead;
            messageDTO.SenderName = (await _context.Users.FindAsync(message.SenderID)).Fullname;

            // Gửi tin nhắn qua SignalR
            await _hubContext.Clients.Group($"Conversation_{message.ConversationID}")
                .SendAsync("ReceiveMessage", message.SenderID, messageDTO.SenderName, message.Content, message.SentAt);

            return CreatedAtAction(nameof(GetMessages), new { conversationId = message.ConversationID }, messageDTO);
        }

    }
}