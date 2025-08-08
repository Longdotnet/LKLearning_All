using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI.DTOS;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyBannerController : ControllerBase
    {
        private readonly LuanvantienganhContext db;
        public QuanLyBannerController(LuanvantienganhContext context)
        {
            db = context;
        }
        //trả về danh sách banner
        // GET: api/QuanLyBanner/GetListBanners
    
        [HttpGet("GetListBanners")]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await db.Banners.ToListAsync();
            return Ok(banners);
        }


        // Thêm mới banner
        // POST: api/QuanLyBanner/AddBanner
        [Authorize(Roles = "Admin")]
        [HttpPost("AddBanner")]
        public async Task<IActionResult> AddBanner([FromBody] Banner banner)
        {
            if (banner == null)
            {
                return BadRequest("Banner không được để trống");
            }
            db.Banners.Add(banner);
            await db.SaveChangesAsync();
            return Ok("Thêm Banner thành công");

        }
        // Cập nhật banner
        // PUT: api/QuanLyBanner/UpdateBanner/{id}
        [HttpPut("UpdateBanner/{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] Banner banner)
        {
            if (banner == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var existingLesson = await db.Banners.FindAsync(id);
            if (existingLesson == null)
            {
                return NotFound("Không tìm thấy Banner");
            }

        

            // Cập nhật các trường (không thay đổi ID)
            existingLesson.BannerId = banner.BannerId;
            existingLesson.BannerTitle = banner.BannerTitle;
            existingLesson.BannerSubtitle = banner.BannerSubtitle;
            existingLesson.BannerDescription = banner.BannerDescription;
            existingLesson.BannerImageUrl = banner.BannerImageUrl;
            existingLesson.LinkUrl = banner.LinkUrl;
            existingLesson.IsActive = banner.IsActive;
            existingLesson.UpdatedAt = DateTime.Now; // Cập nhật thời gian sửa đổi

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa banner thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa banner không thành công");
            }
        }
        // lấy banner id cụ thể
        // // GET: api/QuanLyBanner/GetBannerById/{id}
        [HttpGet("GetBannerById/{id}")]
        public async Task<IActionResult> GetBannerById(int id)
        {
            var banner = await db.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound("Không tìm thấy banner");
            }
            return Ok(banner);
        }
        // Xóa banner
        // DELETE: api/QuanLyBanner/DeleteBanner/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteBanner/{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var question = await db.Banners.FindAsync(id);
            if (question == null)
            {
                return NotFound("Không tìm thấy banner");
            }

            try
            {
                db.Banners.Remove(question);
                await db.SaveChangesAsync();
                return Ok("Xóa Banner thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa Banner không thành công");
            }

        }
    }
}
