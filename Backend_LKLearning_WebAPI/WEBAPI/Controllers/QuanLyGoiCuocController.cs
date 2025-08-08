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
    public class QuanLyGoiCuocController : ControllerBase
    {
        private readonly LuanvantienganhContext db;


        public QuanLyGoiCuocController(LuanvantienganhContext context)
        {
            db = context;
        }

        //=========================package===========================
        //trả về danh sách các gói cước
        // GET: api/QuanLyGoiCuoc/GetListPackages
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetListPackages")]
        public async Task<IActionResult> Getpackages()
        {
           
            var packages = await db.Packages.Include(p=>p.PackageInclusionParentPackages).ThenInclude(a=>a.IncludedPackage).ToListAsync();

            // Map từ entity sang DTO 
            var packagedtos = packages.Select(c => new PackageDTO
            {
                PackageId = c.PackageId,
                PackageName = c.PackageName,
                Price = c.Price,
                DurationDay=c.DurationDay,
                UrlImage=c.UrlImage,
                IncludedPackageIds=c.PackageInclusionParentPackages.Select(ip => ip.IncludedPackage.PackageId).ToList()

            }).ToList();

            return Ok(packagedtos);
        }




        //tìm theo id
        // GET: api/QuanLyGoiCuoc/GetPackageById/{id}
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetPackageById/{id}")]
        public async Task<IActionResult> GetPackageById(int id)
        {
            var package = await db.Packages.Include(p=>p.PackageInclusionParentPackages).ThenInclude(pi=>pi.IncludedPackage).FirstOrDefaultAsync(p=>p.PackageId==id);
            if (package == null)
            {
                return NotFound($"Không tìm thấy package với id = {id}");
            }
            var packageDto = new PackageDTO
            {
                PackageId = package.PackageId,
                PackageName = package.PackageName,
                Price = package.Price,
                DurationDay = package.DurationDay,
                UrlImage = package.UrlImage,
                IncludedPackageIds = package.PackageInclusionParentPackages.Select(ip => ip.IncludedPackage.PackageId).ToList()

            };

            return Ok(packageDto);
        }


        // GET: api/QuanLyGoiCuoc/GetCoursesByPackage/{packageId}
       // [Authorize(Roles = "User,Admin")]
        [HttpGet("GetCoursesByPackage/{packageId}")]
        public async Task<IActionResult> GetCoursesByPackage(int packageId)
        {
            // Kiểm tra xem gói cước với id truyền vào có tồn tại không
            var package = await db.Packages.FindAsync(packageId);
            if (package == null)
            {
                return NotFound($"Không tìm thấy Package với ID = {packageId}");
            }

            // Lấy tất cả các khóa học có PackageId = packageId
            var courses = await db.Courses
                .Where(c => c.PackageId == packageId)
                .ToListAsync();

            // Map từ entity sang DTO (giả sử bạn đã tạo CourseDTO tương ứng)
            var courseDTOs = courses.Select(c => new CourseDTO
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Description = c.Description,
                DurationInMonths = c.DurationInMonths,
                LevelId = c.LevelId,
                UrlImage = c.Img,
                CategoryId = c.CategoryId,
                PackageId = c.PackageId,
            }).ToList();

            return Ok(courseDTOs);
        }


        // Thêm gói cước mới
        // POST: api/QuanLyGoiCuoc/InsertPackage
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertPackage")]
        public async Task<IActionResult> InsertPackage([FromBody] PackageDTO packageDTO)
        {
            if (packageDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Tạo package mới
            var package = new Package
            {
                PackageName = packageDTO.PackageName,
                Price = packageDTO.Price,
                DurationDay = packageDTO.DurationDay,
                UrlImage = packageDTO.UrlImage
            };

            try
            {
                // Thêm package vào database trước để có PackageId
                db.Packages.Add(package);
                await db.SaveChangesAsync();

                // Xử lý các package bao gồm
                if (packageDTO.IncludedPackageIds != null && packageDTO.IncludedPackageIds.Any())
                {
                    foreach (var includedId in packageDTO.IncludedPackageIds)
                    {
                        db.PackageInclusions.Add(new PackageInclusion
                        {
                            ParentPackageId = package.PackageId,
                            IncludedPackageId = includedId
                        });
                    }
                    await db.SaveChangesAsync();
                }

                return Ok(new
                {
                    Message = "Thêm Package thành công",
                    PackageId = package.PackageId
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Lỗi khi thêm package: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm Package không thành công");
            }
        }


        //cập nhật gói cước
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdatePackage/{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] PackageDTO packageDTO)
        {
            if (packageDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var package = await db.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound($"Không tìm thấy Package với ID = {id}");
            }



            // Kiểm tra xem tên gói cước đã tồn tại trong hệ thống (ngoại trừ bản ghi hiện tại) hay chưa
            bool isNameExist = await db.Packages
                .AnyAsync(p => p.PackageName == packageDTO.PackageName && p.PackageId != id);
            if (isNameExist)
            {
                return BadRequest("Tên gói cước đã tồn tại.");
            }

            // Cập nhật các trường, không sử dụng PackageId từ body
            package.PackageName = packageDTO.PackageName;
            package.Price = packageDTO.Price;
            package.DurationDay = packageDTO.DurationDay;
            package.UrlImage = packageDTO.UrlImage;

            try
            {
                // Xóa tất cả các package inclusions hiện có
                db.PackageInclusions.RemoveRange(db.PackageInclusions.Where(pi => pi.ParentPackageId == id));


                // Thêm lại các package inclusions mới
                if (packageDTO.IncludedPackageIds != null && packageDTO.IncludedPackageIds.Any())
                {
                    foreach (var includedId in packageDTO.IncludedPackageIds)
                    {
                        db.PackageInclusions.Add(new PackageInclusion
                        {
                            ParentPackageId = package.PackageId,
                            IncludedPackageId = includedId
                        });
                    }
                }

                await db.SaveChangesAsync();
                return Ok("Cập nhật Package thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cập nhật Package không thành công");
            }
        }

        //xóa gói cước
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeletePackage/{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await db.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound($"Không tìm thấy Package với ID = {id}");
            }

            // Kiểm tra xem có khóa học nào đăng ký gói này không
            bool hasCourseRegistration = await db.Courses.AnyAsync(c => c.PackageId == id);
            if (hasCourseRegistration)
            {
                return BadRequest("Không thể xoá Package vì đã có khóa học đăng ký gói này.");
            }

            // Kiểm tra xem có người dùng nào đăng ký gói này không
            bool hasUserRegistration = await db.UserPackageRegistrations.AnyAsync(u => u.PackageId == id);
            if (hasUserRegistration)
            {
                return BadRequest("Không thể xoá Package vì đã có người dùng đăng ký gói này.");
            }

            try
            {
                db.Packages.Remove(package);
                await db.SaveChangesAsync();
                return Ok("Xoá Package thành công.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xoá Package không thành công");
            }
        }
     

    }
}
