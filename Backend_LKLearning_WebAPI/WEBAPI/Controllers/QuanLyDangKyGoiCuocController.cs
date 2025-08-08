using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEBAPI.DTOS;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyDangKyGoiCuocController : ControllerBase
    {
        private readonly LuanvantienganhContext db;


        public QuanLyDangKyGoiCuocController(LuanvantienganhContext context)
        {
            db = context;
        }


        // GET: api/QuanLyDangKyGoiCuoc/GetUserPackageRegistrationsForUser
        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetUserPackageRegistrationsForUser")]
        public async Task<IActionResult> GetUserPackageRegistrationsForUser()
        {
            // Lấy userId từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Lấy danh sách các đăng ký gói cước của user từ database
            var registrations = await db.UserPackageRegistrations
                .Include(r => r.Package) // Bao gồm thông tin gói cước
                .Where(r => r.UserId == userId)
                .ToListAsync();

            // Chuyển đổi sang DTO để trả về, bao gồm xử lý hiển thị "vĩnh viễn" cho ExpirationDate nếu là null
            List<UserPackageRegistrationDTO> registrationDTOs = registrations
                .Select(reg => new UserPackageRegistrationDTO
                {
                    PackageId = reg.PackageId,
                    UserId = reg.UserId,
                    PackageName = reg.Package.PackageName, // Thêm tên gói cước
                    RegistrationDate = reg.RegistrationDate,
                    ExpirationDate = reg.ExpirationDate  // Sẽ xử lý thành "vĩnh viễn" thông qua property ExpirationDateDisplay
                })
                .ToList();

            return Ok(registrationDTOs);
        }


        // GET: api/QuanLyDangKyGoiCuoc/GetUserPackageRegistrations

        [HttpGet("GetUserPackageRegistrations")]
        public async Task<IActionResult> GetUserPackageRegistrations()
        {
            // Lấy danh sách các đăng ký gói cước từ database
            var registrations = await db.UserPackageRegistrations.ToListAsync();

            // Chuyển đổi sang DTO để trả về, bao gồm xử lý hiển thị "vĩnh viễn" cho ExpirationDate nếu là null
            List<UserPackageRegistrationDTO> registrationDTOs = registrations
                .Select(reg => new UserPackageRegistrationDTO
                {
                    PackageId = reg.PackageId,
                    UserId = reg.UserId,
                    RegistrationDate = reg.RegistrationDate,
                    ExpirationDate = reg.ExpirationDate  // Sẽ xử lý thành "vĩnh viễn" thông qua property ExpirationDateDisplay
                })
                .ToList();

            return Ok(registrationDTOs);
        }
        // POST: api/QuanLyDangKyGoiCuoc/Create
        // Tạo mới đăng ký gói cước, tự động tính ExpirationDate dựa vào DurationMonths của Package
   
        [Authorize(Roles = "User,Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateUserPackageRegistration([FromBody] UserPackageRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra xem đăng ký đã tồn tại chưa (dựa theo UserId và PackageId)
            bool exists = await db.UserPackageRegistrations
                .AnyAsync(r => r.UserId == dto.UserId && r.PackageId == dto.PackageId);
            if (exists)
            {
                return BadRequest("Đăng ký gói cước đã tồn tại.");
            }

            // Lấy thông tin gói cước dựa theo PackageId từ bảng Packages
            var package = await db.Packages.FirstOrDefaultAsync(p => p.PackageId == dto.PackageId);
            if (package == null)
            {
                return BadRequest("Gói cước không tồn tại.");
            }

            // Lấy thời gian đăng ký là thời điểm hiện tại
            DateTime registrationDate = DateTime.Now;
            DateTime? expirationDate = null;

            // Nếu package có DurationMonths (khác null) và lớn hơn 0 thì tính ExpirationDate
            if (package.DurationDay.HasValue && package.DurationDay.Value > 0)
            {
                expirationDate = registrationDate.AddDays(package.DurationDay.Value);
            }
            // Nếu DurationMonths không có giá trị hoặc <= 0 thì giữ ExpirationDate là null (nghĩa là "vĩnh viễn")

            var registration = new UserPackageRegistration
            {
                PackageId = dto.PackageId,
                UserId = dto.UserId,
                RegistrationDate = registrationDate,
                ExpirationDate = expirationDate
            };

            db.UserPackageRegistrations.Add(registration);
            await db.SaveChangesAsync();

            return Ok("Tạo đăng ký gói cước thành công.");
        }


        // DELETE: api/QuanLyDangKyGoiCuoc/Delete/{userId}/{packageId}
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("Delete/{userId:int}/{packageId:int}")]
        public async Task<IActionResult> DeleteUserPackageRegistration(int userId, int packageId)
        {
            var registration = await db.UserPackageRegistrations
                                      .FirstOrDefaultAsync(r => r.UserId == userId && r.PackageId == packageId);

            if (registration == null)
                return NotFound("Không tìm thấy đăng ký để xóa.");

            db.UserPackageRegistrations.Remove(registration);
            await db.SaveChangesAsync();
            return Ok("Xóa đăng ký gói cước thành công.");
        }
        // UPDATE: api/QuanLyDangKyGoiCuoc/Update/{userId}/{packageId}
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{userId:int}/{packageId:int}")]
        public async Task<IActionResult> UpdateUserPackageRegistration(int userId, int packageId, [FromBody] UserPackageRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, errors });
            }

            // Kiểm tra người dùng tồn tại
            var userExists = await db.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại." });
            }

            // Kiểm tra đăng ký hiện tại
            var registration = await db.UserPackageRegistrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PackageId == packageId);
            if (registration == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đăng ký để cập nhật." });
            }

            // Kiểm tra gói cước mới
            var newPackage = await db.Packages.FindAsync(dto.PackageId);
            if (newPackage == null)
            {
                return BadRequest(new { success = false, message = "Gói cước mới không tồn tại." });
            }

            // Nếu PackageId thay đổi, kiểm tra xung đột
            if (packageId != dto.PackageId)
            {
                var exists = await db.UserPackageRegistrations
                    .AnyAsync(r => r.UserId == userId && r.PackageId == dto.PackageId);
                if (exists)
                {
                    return Conflict(new { success = false, message = "Người dùng đã đăng ký gói cước này rồi." });
                }

                // Xóa đăng ký cũ và tạo mới
                db.UserPackageRegistrations.Remove(registration);
                var newRegistration = new UserPackageRegistration
                {
                    UserId = userId,
                    PackageId = dto.PackageId,
                    RegistrationDate = dto.RegistrationDate,
                    ExpirationDate = newPackage.DurationDay.HasValue && newPackage.DurationDay.Value > 0
                        ? dto.RegistrationDate.AddDays(newPackage.DurationDay.Value)
                        : null
                };
                db.UserPackageRegistrations.Add(newRegistration);
            }
            else
            {
                // Cập nhật thông tin không thay đổi khóa chính
                registration.RegistrationDate = dto.RegistrationDate;
                registration.ExpirationDate = newPackage.DurationDay.HasValue && newPackage.DurationDay.Value > 0
                    ? dto.RegistrationDate.AddDays(newPackage.DurationDay.Value)
                    : null;
            }

            await db.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật đăng ký gói cước thành công." });
        }
        // Get api/QuanLyDangKyGoiCuoc/GetPackageByUser/{userId}

        [HttpGet("GetPackageByUser/{userId}")]
        public async Task<IActionResult> GetPackageByUser(int userId)
        {
            // Kiểm tra xem người dùng với id truyền vào có tồn tại không
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID = {userId}");
            }
            // Lấy tất cả các đăng ký gói cước của người dùng lấy tất cả trừ free xét dựa vào Package.PackageName
            var registrations = await db.UserPackageRegistrations
                .Include(r => r.Package) // Bao gồm thông tin gói cước
                .Where(r => r.UserId == userId && r.Package.PackageName != "FREE")
                .ToListAsync();
            // Map từ entity sang DTO (giả sử bạn đã tạo UserPackageRegistrationDTO tương ứng)
            var registrationDTOs = registrations.Select(r => new UserPackageRegistrationDTO
            {
                PackageId = r.PackageId,
                UserId = r.UserId,
                PackageName = r.Package.PackageName, // Thêm tên gói cước
                RegistrationDate = r.RegistrationDate,
                ExpirationDate = r.ExpirationDate,
            }).ToList();
            return Ok(registrationDTOs);
        }
        [HttpGet("GetPackageByUserIdAndByPackageId/{userId}/{packageId}")]
        public async Task<IActionResult> GetPackageByUserIdAndByPackageId(int userId, int packageId)
        {
            // Kiểm tra xem người dùng với id truyền vào có tồn tại không
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID = {userId}");
            }
            // Lấy đăng ký gói cước của người dùng theo PackageId
            var registration = await db.UserPackageRegistrations
                .Include(r => r.Package) // Bao gồm thông tin gói cước
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PackageId == packageId);
            if (registration == null)
            {
                return NotFound($"Không tìm thấy đăng ký gói cước cho người dùng với ID = {userId} và PackageId = {packageId}");
            }
            // Chuyển đổi sang DTO
            var registrationDTO = new UserPackageRegistrationDTO
            {
                PackageId = registration.PackageId,
                UserId = registration.UserId,
                PackageName = registration.Package.PackageName, // Thêm tên gói cước
                RegistrationDate = registration.RegistrationDate,
                ExpirationDate = registration.ExpirationDate,
            };
            return Ok(registrationDTO);
        }


    }
}
