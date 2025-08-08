using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI.DTOS;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực JWT
    public class ConversationsController : ControllerBase
    {
        private readonly LuanvantienganhContext _context;
        private readonly IOnlineUserTracker _tracker;

        public ConversationsController(LuanvantienganhContext context, IOnlineUserTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }
        [HttpGet("online-users-admins")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetOnlineUsersAndAdmins()
        {
            // Lấy tất cả userId và adminId từ database
            var adminRoleId = await _context.Roles.Where(r => r.RoleName == "Admin").Select(r => r.RoleId).FirstOrDefaultAsync();
            var userRoleId = await _context.Roles.Where(r => r.RoleName == "User").Select(r => r.RoleId).FirstOrDefaultAsync();

            var allAdminIds = await _context.Users.Where(u => u.RoleId == adminRoleId).Select(u => u.UserId).ToListAsync();
            var allUserIds = await _context.Users.Where(u => u.RoleId == userRoleId).Select(u => u.UserId).ToListAsync();

            // Lấy danh sách id đang online từ tracker
            var onlineAdminIds = _tracker.GetOnlineAdmins(allAdminIds);
            var onlineUserIds = _tracker.GetOnlineUsers(allUserIds);

            // Lấy thông tin chi tiết user/admin đang online
            var onlineAdmins = await _context.Users
                .Where(u => onlineAdminIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.Fullname, u.Email })
                .ToListAsync();

            var onlineUsers = await _context.Users
                .Where(u => onlineUserIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.Fullname, u.Email })
                .ToListAsync();

            return Ok(new
            {
                OnlineAdmins = onlineAdmins,
                OnlineUsers = onlineUsers
            });
        }
        // Lấy danh sách cuộc trò chuyện của admin
        [HttpGet("admin/{adminId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ConversationDTO>>> GetConversations(int adminId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId != adminId)
            {
                return Unauthorized("Không có quyền truy cập danh sách cuộc trò chuyện.");
            }

            var conversations = await _context.Conversations
                .Where(c => c.AdminID == adminId)
                .Include(c => c.User)
                .Select(c => new ConversationDTO
                {
                    ConversationID = c.ConversationID,
                    AdminID = c.AdminID,
                    AdminName = c.Admin.Fullname,
                    UserID = c.UserID,
                    UserName = c.User.Fullname,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .ToListAsync();
            return Ok(conversations);
        }

        // Lấy danh sách cuộc trò chuyện của user
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<ConversationDTO>>> GetUserConversations(int userId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId != userId)
            {
                return Unauthorized("Không có quyền truy cập danh sách cuộc trò chuyện.");
            }

            var conversations = await _context.Conversations
                .Where(c => c.UserID == userId)
                .Include(c => c.Admin)
                .Select(c => new ConversationDTO
                {
                    ConversationID = c.ConversationID,
                    AdminID = c.AdminID,
                    AdminName = c.Admin.Fullname,
                    UserID = c.UserID,
                    UserName = c.User.Fullname,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .ToListAsync();
            return Ok(conversations);
        }
        // lấy chi tiết cuộc trò chuyện giữa admin và user
        [HttpGet("between")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<ConversationDTO>> GetConversationBetween([FromQuery] int adminId, [FromQuery] int userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Admin)
                .Include(c => c.User)
                .Where(c => c.AdminID == adminId && c.UserID == userId)
                .Select(c => new ConversationDTO
                {
                    ConversationID = c.ConversationID,
                    AdminID = c.AdminID,
                    AdminName = c.Admin.Fullname,
                    UserID = c.UserID,
                    UserName = c.User.Fullname,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync();

            if (conversation == null)
                return NotFound();

            return Ok(conversation);
        }
        [HttpPost("user-create")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ConversationDTO>> UserCreateConversation()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            // Kiểm tra user đã có cuộc trò chuyện đang mở chưa
            var existing = await _context.Conversations
                .FirstOrDefaultAsync(c => c.UserID == currentUserId && c.IsActive);
            if (existing != null)
            {
                var existingDto = new ConversationDTO
                {
                    ConversationID = existing.ConversationID,
                    AdminID = existing.AdminID,
                    AdminName = (await _context.Users.FindAsync(existing.AdminID))?.Fullname,
                    UserID = existing.UserID,
                    UserName = (await _context.Users.FindAsync(existing.UserID))?.Fullname,
                    CreatedAt = existing.CreatedAt,
                    IsActive = existing.IsActive
                };
                return Ok(existingDto);
            }

            // Lấy danh sách adminId
            var adminRoleId = await _context.Roles.Where(r => r.RoleName == "Admin").Select(r => r.RoleId).FirstOrDefaultAsync();
            var adminIds = await _context.Users.Where(u => u.RoleId == adminRoleId).Select(u => u.UserId).ToListAsync();

            // Lọc ra các admin đang online
            var onlineAdminIds = _tracker.GetOnlineAdmins(adminIds);
            if (!onlineAdminIds.Any())
                return BadRequest("Không có admin nào đang online.");

            // Chọn admin online random
            var random = new Random();
            // Chọn một admin online ngẫu nhiên đang online
            var adminId = onlineAdminIds[random.Next(onlineAdminIds.Count)];



            var conversation = new Conversation
            {
                AdminID = adminId,
                UserID = currentUserId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var conversationDTO = new ConversationDTO
            {
                ConversationID = conversation.ConversationID,
                AdminID = conversation.AdminID,
                AdminName = (await _context.Users.FindAsync(conversation.AdminID))?.Fullname,
                UserID = conversation.UserID,
                UserName = (await _context.Users.FindAsync(conversation.UserID))?.Fullname,
                CreatedAt = conversation.CreatedAt,
                IsActive = conversation.IsActive
            };

            return CreatedAtAction(nameof(GetConversation), new { id = conversation.ConversationID }, conversationDTO);
        }
        // TAO CUOC tro chuyen moi boi admin

        // Tạo cuộc trò chuyện mới
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ConversationDTO>> CreateConversation([FromBody] ConversationDTO conversationDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // TÁC DỤNG

            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId != conversationDTO.AdminID)
            {
                return Unauthorized("Không có quyền tạo cuộc trò chuyện.");
            }

            var existing = await _context.Conversations
                .FirstOrDefaultAsync(c => c.AdminID == conversationDTO.AdminID && c.UserID == conversationDTO.UserID);
            if (existing != null)
                return BadRequest("Cuộc trò chuyện đã tồn tại.");

            var conversation = new Conversation
            {
                AdminID = conversationDTO.AdminID,
                UserID = conversationDTO.UserID,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            conversationDTO.ConversationID = conversation.ConversationID;
            conversationDTO.CreatedAt = conversation.CreatedAt;
            conversationDTO.IsActive = conversation.IsActive;
            conversationDTO.AdminName = (await _context.Users.FindAsync(conversation.AdminID)).Fullname;
            conversationDTO.UserName = (await _context.Users.FindAsync(conversation.UserID)).Fullname;

            return CreatedAtAction(nameof(GetConversation), new { id = conversation.ConversationID }, conversationDTO);
        }

        // Lấy chi tiết cuộc trò chuyện
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<ConversationDTO>> GetConversation(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var conversation = await _context.Conversations
                .Include(c => c.Admin)
                .Include(c => c.User)
                .Where(c => c.ConversationID == id && (c.AdminID == currentUserId || c.UserID == currentUserId))
                .Select(c => new ConversationDTO
                {
                    ConversationID = c.ConversationID,
                    AdminID = c.AdminID,
                    AdminName = c.Admin.Fullname,
                    UserID = c.UserID,
                    UserName = c.User.Fullname,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync();

            if (conversation == null) return NotFound("Không tìm thấy cuộc trò chuyện hoặc bạn không có quyền truy cập.");
            return Ok(conversation);
        }
    }
}