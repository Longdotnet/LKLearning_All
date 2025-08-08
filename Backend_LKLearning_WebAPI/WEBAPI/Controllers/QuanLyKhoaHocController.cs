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
    public class QuanLyKhoaHocController : ControllerBase
    {
        private readonly LuanvantienganhContext db;


        public QuanLyKhoaHocController(LuanvantienganhContext context)
        {
            db = context;
        }

        //==========đầu tiên là về category========= 

        //trả về danh sách các loại 
        // GET: api/QuanLyKhoaHoc/GetListCategories
      
        [HttpGet("GetListCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await db.Categories.ToListAsync();

            // Map từ entity sang DTO 
            var categoryDTOs = categories.Select(c => new CategoriesDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description

            }).ToList();

            return Ok(categoryDTOs);
        }

        //Trả về Loại Theo ID 
        //Get:api/QuanLyKhoaHoc/GetCategoryID
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetCategoryID")]
        public async Task<IActionResult> GetCategoryID(int id)
        {
            // Tìm Category theo id
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("Không tìm thấy Category");
            }

            // Map từ entity sang DTO 
            var categoryDTOs = new CategoriesDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
            };


            return Ok(categoryDTOs);


        }

        //thêm loại
        // POST: api/QuanLyKhoaHoc/Categories
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertCategories")]
        public async Task<IActionResult> AddCategory([FromBody] CategoriesDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Ánh xạ từ DTO sang Entity
            var category = new Category
            {
                CategoryName = categoryDTO.CategoryName,
                Description = categoryDTO.Description
            };

            try
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm không thành công");
            }
        }

        //Update Loại
        // PUT: api/QuanLyKhoaHoc/Categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("updateCategories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoriesDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Tìm Category theo id (được truyền qua URL)
            var existingCategory = await db.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound("Không tìm thấy Category");
            }

            // Cập nhật các trường (không sử dụng field CategoryId từ DTO)
            existingCategory.CategoryName = categoryDTO.CategoryName;
            existingCategory.Description = categoryDTO.Description;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa không thành công");
            }
        }

        //xóa loại
        // DELETE: api/QuanLyKhoaHoc/Categories/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteCategories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Tìm Category theo id
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("Không tìm thấy Category");
            }

            // Kiểm tra xem có Course nào liên kết với Category này không
            bool hasRelatedCourses = await db.Courses.AnyAsync(c => c.CategoryId == id);
            if (hasRelatedCourses)
            {
                return BadRequest("Không thể xóa Category vì có Course liên kết");
            }

            try
            {
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return Ok("Xóa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa không thành công");
            }
        }


        //==========tiếp theo là về Level========= 

        //Get danh sách level
        // GET: api/QuanLyKhoaHoc/Levels
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetListLevels")]
        public async Task<IActionResult> GetLevels()
        {
            var levels = await db.Levels.ToListAsync();

            // Ánh xạ từ entity sang DTO
            var levelDTOs = levels.Select(l => new LevelDTO
            {
                LevelId = l.LevelId,
                LevelName = l.LevelName,
                Description = l.Description
            }).ToList();

            return Ok(levelDTOs);
        }


        //Get về Level Theo ID 
        //Get:api/QuanLyKhoaHoc/GetCategoryID
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetLevelID/{id}")]
        public async Task<IActionResult> GetlevelID(int id)
        {
            // Tìm Category theo id
            var level = await db.Levels.FindAsync(id);
            if (level == null)
            {
                return NotFound("Không tìm thấy Category");
            }
            var leveldto = new LevelDTO
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                Description = level.Description
            };
            return Ok(leveldto);


        }

        //thêm danh sách level
        // POST: api/QuanLyKhoaHoc/Levels
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertLevels")]
        public async Task<IActionResult> AddLevel([FromBody] LevelDTO levelDTO)
        {
            if (levelDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Ánh xạ từ DTO sang Entity
            var level = new Level
            {
                LevelName = levelDTO.LevelName,
                Description = levelDTO.Description
            };

            try
            {
                db.Levels.Add(level);
                await db.SaveChangesAsync();
                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm không thành công");
            }
        }


        //Update Level
        // PUT: api/QuanLyKhoaHoc/Levels/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateLevels/{id}")]
        public async Task<IActionResult> UpdateLevel(int id, [FromBody] LevelDTO levelDTO)
        {
            if (levelDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var existingLevel = await db.Levels.FindAsync(id);
            if (existingLevel == null)
            {
                return NotFound("Không tìm thấy Level");
            }

            // Cập nhật các trường - không thay đổi Id
            existingLevel.LevelName = levelDTO.LevelName;
            existingLevel.Description = levelDTO.Description;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa không thành công");
            }
        }



        // DELETE: api/QuanLyKhoaHoc/Levels/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteLevels/{id}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            var level = await db.Levels.FindAsync(id);
            if (level == null)
            {
                return NotFound("Không tìm thấy Level");
            }

            // Kiểm tra xem có Course nào liên kết với Level này không
            bool hasRelatedCourses = await db.Courses.AnyAsync(c => c.LevelId == id);
            if (hasRelatedCourses)
            {
                return BadRequest("Không thể xóa Level vì có Course liên kết");
            }

            try
            {
                db.Levels.Remove(level);
                await db.SaveChangesAsync();
                return Ok("Xóa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa không thành công");
            }
        }
     

       
        //=======Course===============
        //Get List Danh Sách Khóa Học

        // GET: api/QuanLyKhoaHoc/GetListCourse/
        //[AllowAnonymous] // KHÔNG cần đăng nhập để lấy token
        [HttpGet("GetListCourse")]
        public async Task<IActionResult> GetListCourse()
        {
            var Courses = await db.Courses.ToListAsync();

            // Ánh xạ từ entity sang DTO
            var coursesDTO = Courses.Select(l => new CourseDTO
            {
                CourseId = l.CourseId,
                CourseName = l.CourseName,
                Description = l.Description,
                DurationInMonths = l.DurationInMonths,
                LevelId = l.LevelId,
                UrlImage = l.Img,
                CategoryId = l.CategoryId,
                PackageId = l.PackageId,
                CertificateDurationDays = l.CertificateDurationDays

            }).ToList();

            return Ok(coursesDTO);
        }

        //GET:api/QuanLyKhoaHoc/GetCourseID
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetCourseID")]
        public async Task<IActionResult> GetCourseID(int id)
        {
            var Courses = await db.Courses.FindAsync(id);

            if (Courses == null)
            {
                return BadRequest("Course này không tồn tại trong hệ thống !");
            }
            var coursedto = new CourseDTO
            {
                CourseId = Courses.CourseId,
                CourseName = Courses.CourseName,
                Description = Courses.Description,
                DurationInMonths = Courses.DurationInMonths,
                LevelId = Courses.LevelId,
                UrlImage = Courses.Img,
                CategoryId = Courses.CategoryId,
                PackageId = Courses.PackageId,
                CertificateDurationDays = Courses.CertificateDurationDays
            };


            return Ok(coursedto);
        }


        //thêm danh sách Course
        // POST: api/QuanLyKhoaHoc/Course
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertCourse")]
        public async Task<IActionResult> AddCourse([FromBody] CourseDTO courseDTO)
        {
            if (courseDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            //kiểm tra xem category thêm vào có tồn tại không 
            if (await db.Categories.FindAsync(courseDTO.CategoryId) == null)
            {
                return BadRequest("Mã category không tồn tại trong hệ thống");
            }

            //kiểm tra xem level thêm vào có tồn tại không
            if (await db.Levels.FindAsync(courseDTO.LevelId) == null)
            {
                return BadRequest("Mã level không tồn tại trong hệ thống");
            }

            //kiểm tra xem package thêm vào có tồn tại trong không
            if (await db.Packages.FindAsync(courseDTO.PackageId) == null)
            {
                return BadRequest("Mã Package không tồn tại trong hệ thống");
            }
            // Ánh xạ từ DTO sang Entity
            var course = new Course
            {
                CourseName = courseDTO.CourseName,
                Description = courseDTO.Description,
                DurationInMonths = courseDTO.DurationInMonths,
                LevelId = courseDTO.LevelId,
                Img = courseDTO.UrlImage,
                CategoryId = courseDTO.CategoryId,
                PackageId = courseDTO.PackageId,
                CertificateDurationDays = courseDTO.CertificateDurationDays
            };
            try
            {
                db.Courses.Add(course);
                await db.SaveChangesAsync();
                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm không thành công");
            }
        }


        //update course
        // PUT: api/QuanLyKhoaHoc/UpdateCourse/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateCourse/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseDTO courseDTO)
        {
            if (courseDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Tìm Course cần sửa theo id từ URL
            var existingCourse = await db.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound("Course không tồn tại trong hệ thống");
            }

            // Kiểm tra xem Category được update có tồn tại không
            if (await db.Categories.FindAsync(courseDTO.CategoryId) == null)
            {
                return BadRequest("Mã category không tồn tại trong hệ thống");
            }

            // Kiểm tra xem Level được update có tồn tại không
            if (await db.Levels.FindAsync(courseDTO.LevelId) == null)
            {
                return BadRequest("Mã level không tồn tại trong hệ thống");
            }
            //kiểm tra xem package thêm vào có tồn tại trong không
            if (await db.Packages.FindAsync(courseDTO.PackageId) == null)
            {
                return BadRequest("Mã Package không tồn tại trong hệ thống");
            }


            // Cập nhật các trường của Course (không thay đổi id)
            existingCourse.CourseName = courseDTO.CourseName;
            existingCourse.Description = courseDTO.Description;
            existingCourse.DurationInMonths = courseDTO.DurationInMonths;
            existingCourse.LevelId = courseDTO.LevelId;
            existingCourse.Img = courseDTO.UrlImage;
            existingCourse.CategoryId = courseDTO.CategoryId;
            existingCourse.PackageId = courseDTO.PackageId;
            existingCourse.CertificateDurationDays = courseDTO.CertificateDurationDays;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa không thành công");
            }
        }
   
        //Delete Course
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteCourse/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            // Tìm Course theo id
            var course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound("Không tìm thấy Course");
            }
            try
            {
                db.Courses.Remove(course);
                await db.SaveChangesAsync();
                return Ok("Xóa thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa không thành công");
            }
        }
    } 
}
