using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyCertificateController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyCertificateController(LuanvantienganhContext context)
        {
            db = context;
        }
        // Lấy danh sách chứng chỉ của user hiện tại (lấy userId từ token)
        [Authorize(Roles = "User")]
        [HttpGet("GetListCertificatesForUser")]
        public async Task<IActionResult> GetListCertificatesForUser()
        {
            // Lấy userId từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var certificates = await db.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .Include(c => c.CertificateType)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var certificateDTOs = certificates.Select(c => new CertificateDTO
            {
                CertificateId = c.CertificateId,
                UserId = c.UserId,
                Fullname = c.User?.Fullname,
                CourseId = c.CourseId,
                CourseName = c.Course?.CourseName,
                VerificationCode = c.VerificationCode,
                ImageUrl = c.ImageUrl,
                Signature = c.Signature,
                CreatedAt = c.CreatedAt,
                ExpirationDate = c.ExpirationDate,
                CertificateTypeId = c.CertificateTypeId,
                CertificateTypeName = c.CertificateType?.TypeName
            }).ToList();

            return Ok(certificateDTOs);
        }
        // Lấy danh sách chứng chỉ của user hiện tại theo courseId (lấy userId từ token)
        [Authorize(Roles = "User")]
        [HttpGet("GetListCertificatesCourseIdForUser")]
        public async Task<IActionResult> GetListCertificatesCourseIdForUser([FromQuery] int courseId)
        {
            // Lấy userId từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var certificates = await db.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .Include(c => c.CertificateType)
                .Where(c => c.UserId == userId && c.CourseId == courseId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var certificateDTOs = certificates.Select(c => new CertificateDTO
            {
                CertificateId = c.CertificateId,
                UserId = c.UserId,
                Fullname = c.User?.Fullname,
                CourseId = c.CourseId,
                CourseName = c.Course?.CourseName,
                VerificationCode = c.VerificationCode,
                ImageUrl = c.ImageUrl,
                Signature = c.Signature,
                CreatedAt = c.CreatedAt,
                ExpirationDate = c.ExpirationDate,
                CertificateTypeId = c.CertificateTypeId,
                CertificateTypeName = c.CertificateType?.TypeName
            }).ToList();

            return Ok(certificateDTOs);
        }


        //trả về danh sách Certificate  
        // GET: api/QuanLyCertificate/GetListCertificates
        [HttpGet("GetListCertificates")]
        public async Task<IActionResult> GetListCertificates()
        {
            var certificates = await db.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .Include(c => c.CertificateType)
                .ToListAsync();

            var certificateDTOs = certificates.Select(c => new CertificateDTO
            {
                CertificateId = c.CertificateId,
                UserId = c.UserId,
                Fullname = c.User?.Fullname,
                CourseId = c.CourseId,
                CourseName = c.Course?.CourseName,
                VerificationCode = c.VerificationCode,
                ImageUrl = c.ImageUrl,
                Signature = c.Signature,
                CreatedAt = c.CreatedAt,
                ExpirationDate = c.ExpirationDate,
                CertificateTypeId = c.CertificateTypeId,
                CertificateTypeName = c.CertificateType?.TypeName
            }).ToList();

            return Ok(certificateDTOs);
        }

        //trả về Certificate theo id
        // GET: api/QuanLyCertificate/GetCertificateById/5
        [HttpGet("GetCertificateById/{id}")]
        public async Task<IActionResult> GetCertificateById(int id)
        {
            var certificate = await db.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .Include(c => c.CertificateType)
                .FirstOrDefaultAsync(c => c.CertificateId == id);
                
            if (certificate == null)
            {
                return NotFound();
            }
         
            // Map từ entity sang DTO 
            var certificateDTO = new CertificateDTO
            {
                CertificateId = certificate.CertificateId,
                UserId = certificate.UserId,
                Fullname = certificate.User?.Fullname,
                CourseId = certificate.CourseId,
                CourseName = certificate.Course?.CourseName,
                VerificationCode = certificate.VerificationCode,
                ImageUrl = certificate.ImageUrl,
                Signature = certificate.Signature,
                CreatedAt = certificate.CreatedAt,
                ExpirationDate = certificate.ExpirationDate,
                CertificateTypeId = certificate.CertificateTypeId,
                CertificateTypeName = certificate.CertificateType?.TypeName
            };
            return Ok(certificateDTO);
        }
        // xóa Certificates 
        // DELETE: api/QuanLyCertificate/DeleteCertificate/5
        [HttpDelete("DeleteCertificate/{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var certificate = await db.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            db.Certificates.Remove(certificate);
            await db.SaveChangesAsync();
            return Ok("Xóa Certificate thành công");
        }
        // trả về danh sách CertificateType 
        // GET: api/QuanLyCertificate/GetListCertificateTypes
        [HttpGet("GetListCertificateTypes")]
        public async Task<IActionResult> GetListCertificateTypes()
        {
            var certificateTypes = await db.CertificateTypes.ToListAsync();
            // Map từ entity sang DTO 
            var certificateTypeDTOs = certificateTypes.Select(ct => new CertificateTypeDTO
            {
                CertificateTypeId = ct.CertificateTypeId,
                TypeName = ct.TypeName,

            }).ToList();
            return Ok(certificateTypeDTOs);
        }
        // trả về CertificateType theo id
        // GET: api/QuanLyCertificate/GetCertificateTypeById/5
        [HttpGet("GetCertificateTypeById/{id}")]
        public async Task<IActionResult> GetCertificateTypeById(int id)
        {
            var certificateType = await db.CertificateTypes.FindAsync(id);
            if (certificateType == null)
            {
                return NotFound();
            }
            // Map từ entity sang DTO 
            var certificateTypeDTO = new CertificateTypeDTO
            {
                CertificateTypeId = certificateType.CertificateTypeId,
                TypeName = certificateType.TypeName,
            };
            return Ok(certificateTypeDTO);
        }
        // Thêm mới CertificateType
        // POST: api/QuanLyCertificate/AddCertificateType
        [HttpPost("AddCertificateType")]
        public async Task<IActionResult> AddCertificateType([FromBody] CertificateTypeDTO CertificateTypeDTO)
        {
            if (CertificateTypeDTO == null)
            {
                return BadRequest("CertificateType không được để trống");
            }
            var certificateType = new CertificateType
            {
                TypeName = CertificateTypeDTO.TypeName,
            };
            db.CertificateTypes.Add(certificateType);
            await db.SaveChangesAsync();
            return Ok("Thêm CertificateType thành công");
        }
        // Cập nhật CertificateType
        // PUT: api/QuanLyCertificate/UpdateCertificateType/{id}
        [HttpPut("UpdateCertificateType/{id}")]
        public async Task<IActionResult> UpdateCertificateType(int id, [FromBody] CertificateTypeDTO certificateTypeDTO)
        {
            if (certificateTypeDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var existingCertificateType = await db.CertificateTypes.FindAsync(id);
            if (existingCertificateType == null)
            {
                return NotFound("Không tìm thấy CertificateType");
            }
            existingCertificateType.TypeName = certificateTypeDTO.TypeName;
            db.CertificateTypes.Update(existingCertificateType);
            await db.SaveChangesAsync();
            return Ok("Cập nhật CertificateType thành công");
        }
        // xóa CertificateType
        // DELETE: api/QuanLyCertificate/DeleteCertificateType/5
        [HttpDelete("DeleteCertificateType/{id}")]
        public async Task<IActionResult> DeleteCertificateType(int id)
        {
            var certificateType = await db.CertificateTypes.FindAsync(id);
            if (certificateType == null)
            {
                return NotFound();
            }
            db.CertificateTypes.Remove(certificateType);
            await db.SaveChangesAsync();
            return Ok("Xóa CertificateType thành công");

        }
    }
}
