using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI.DTOS;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyAvatarController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyAvatarController(LuanvantienganhContext context)
        {
            db = context;
        }

        //trả về danh sách Avatar 
        // GET: api/QuanLyAvatar/GetListAvatar
        [HttpGet("GetListAvatar")]
        public async Task<IActionResult> GetListAvatar()
        {
            var avatars = await db.Avatars.ToListAsync();

            // Map từ entity sang DTO 
            var avatarDTOs = avatars.Select(c => new AvatarDTO
            {
                AvatarId = c.AvatarId,
                UrlPath = c.UrlPath,
                CreatedAt = c.CreatedAt
            }).ToList();


            return Ok(avatarDTOs);
        }
        //trả về Avatar theo id
        // GET: api/QuanLyAvatar/GetAvatarById/5
        [HttpGet("GetAvatarById/{id}")]
        public async Task<IActionResult> GetAvatarById(int id)
        {
            var avatar = await db.Avatars.FindAsync(id);
            if (avatar == null)
            {
                return NotFound();
            }
            // Map từ entity sang DTO 
            var avatarDTO = new AvatarDTO
            {
                AvatarId = avatar.AvatarId,
                UrlPath = avatar.UrlPath,
                CreatedAt = avatar.CreatedAt
            };
            return Ok(avatarDTO);
        }
        //thêm mới Avatar
        // POST: api/QuanLyAvatar/CreateAvatar
        [HttpPost("CreateAvatar")]
        public async Task<IActionResult> CreateAvatar([FromBody] AvatarDTO avatarDTO)
        {
            if (avatarDTO == null)
            {
                return BadRequest("Avatar data is null");
            }
            var avatar = new Avatar
            {
                UrlPath = avatarDTO.UrlPath,
                CreatedAt = DateTime.Now // Hoặc có thể lấy từ avatarDTO nếu có trường CreatedAt
            };

            db.Avatars.Add(avatar);
            await db.SaveChangesAsync();
            return Ok("Thêm Banner thành công");
        }
        //cập nhật Avatar
        // PUT: api/QuanLyAvatar/UpdateAvatar/{id}
        [HttpPut("UpdateAvatar/{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] AvatarDTO avatarDTO)
        {
            if (avatarDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var existingAvatar = await db.Avatars.FindAsync(id);
            if (existingAvatar == null)
            {
                return NotFound("Không tìm thấy Avatar");
            }
            // Cập nhật các trường cần thiết
            existingAvatar.UrlPath = avatarDTO.UrlPath;
            db.Avatars.Update(existingAvatar);
            await db.SaveChangesAsync();
            return Ok("Cập nhật Avatar thành công");
        }
        //xóa Avatar
        // DELETE: api/QuanLyAvatar/DeleteAvatar/{id}
        [HttpDelete("DeleteAvatar/{id}")]
        public async Task<IActionResult> DeleteAvatar(int id)
        {
            var avatar = await db.Avatars.FindAsync(id);
            if (avatar == null)
            {
                return NotFound("Không tìm thấy Avatar");
            }
            db.Avatars.Remove(avatar);
            await db.SaveChangesAsync();
            return Ok("Xóa Avatar thành công");

        }
    }
}
