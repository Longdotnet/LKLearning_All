using System;
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
    public class QuanLyBaiHocController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyBaiHocController(LuanvantienganhContext context)
        {
            db = context;
        }

        //trả về danh sách bài học 
        // GET: api/QuanLyBaiHoc/GetListLesson
        [HttpGet("GetListLesson")]
        public async Task<IActionResult> GetListLesson()
        {
            var lesson = await db.Lessons.ToListAsync();

            // Map từ entity sang DTO 
            var lessonDTOs = lesson.Select(c => new LessonDTO
            {
                LessonId = c.LessonId,
                LessonTitle = c.LessonTitle,
                LessonContent = c.LessonContent,
                LessonDescription = c.LessonDescription,
                Duration = c.Duration,
                DurationMinute = c.DurationMinute,
                IsActivate = c.IsActivate,
                UrlImageLesson = c.UrlImageLesson
            }).ToList();

            return Ok(lessonDTOs);
        }

        //========================================
        // GET: api/QuanLyBaiHoc/GetLessonById/{id}
        // Trả về chi tiết của 1 bài học dựa theo id
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetLessonById/{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound("Không tìm thấy bài học");
            }

            var lessonDTO = new LessonDTO
            {
                LessonId = lesson.LessonId,
                LessonTitle = lesson.LessonTitle,
                LessonContent = lesson.LessonContent,
                LessonDescription = lesson.LessonDescription,
                Duration = lesson.Duration,
                DurationMinute = lesson.DurationMinute,
                IsActivate = lesson.IsActivate,
                UrlImageLesson = lesson.UrlImageLesson
            };

            return Ok(lessonDTO);
        }

        // POST: api/QuanLyBaiHoc/InsertLesson
        // Thêm mới bài học
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertLesson")]
        public async Task<IActionResult> InsertLesson([FromBody] LessonDTO lessonDTO)
        {
            if (lessonDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Ánh xạ từ DTO sang Entity
            var lesson = new Lesson
            {
                LessonTitle = lessonDTO.LessonTitle,
                LessonContent = lessonDTO.LessonContent,
                LessonDescription = lessonDTO.LessonDescription,
                Duration = DateOnly.FromDateTime(DateTime.Now),
                DurationMinute = lessonDTO.DurationMinute,
                IsActivate = lessonDTO.IsActivate,
                UrlImageLesson = lessonDTO.UrlImageLesson

            };

            try
            {
                db.Lessons.Add(lesson);
                await db.SaveChangesAsync();
                return Ok("Thêm bài học thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm bài học không thành công");
            }
        }

        // PUT: api/QuanLyBaiHoc/UpdateLesson/{id}
        // Cập nhật dữ liệu bài học theo id
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateLesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonDTO lessonDTO)
        {
            if (lessonDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var existingLesson = await db.Lessons.FindAsync(id);
            if (existingLesson == null)
            {
                return NotFound("Không tìm thấy bài học");
            }

            // Cập nhật các trường (không thay đổi ID)
            existingLesson.LessonTitle = lessonDTO.LessonTitle;
            existingLesson.LessonContent = lessonDTO.LessonContent;
            existingLesson.LessonDescription = lessonDTO.LessonDescription;
            existingLesson.Duration = lessonDTO.Duration;
            existingLesson.DurationMinute = lessonDTO.DurationMinute;
            existingLesson.IsActivate = lessonDTO.IsActivate;
            existingLesson.UrlImageLesson = lessonDTO.UrlImageLesson;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa bài học thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa bài học không thành công");
            }
        }

        // DELETE: api/QuanLyBaiHoc/DeleteLesson/{id}
        // Xóa bài học theo id
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteLesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound("Không tìm thấy bài học");
            }

            try
            {
                db.Lessons.Remove(lesson);
                await db.SaveChangesAsync();
                return Ok("Xóa bài học thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa bài học không thành công");
            }
        }

        //======================Phần CourseLesson==========================

        // GET: api/QuanLyBaiHoc/GetCourseLessonByID/{id}
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetCourseLessonByID/{id}")]
        public async Task<IActionResult> GetCourseLessonByID(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Khóa học không hợp lệ.");
            }

            // Kiểm tra khóa học có tồn tại không
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == id);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            // Lấy danh sách CourseLesson của khóa với thứ tự tăng dần của OrderNo
            var courseLessons = await db.CourseLessons
                .Where(cl => cl.CourseId == id)
                .OrderBy(cl => cl.OrderNo)
                .Select(cl => new CourseLessonsDTO
                {
                    CourseId = cl.CourseId,
                    LessonId = cl.LessonId,
                    OrderNo = cl.OrderNo
                })
                .ToListAsync();

            return Ok(courseLessons);
        }

        [HttpGet("GetLessonsByCourseId/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            var courseLessons = await db.CourseLessons
                .Where(cl => cl.CourseId == courseId)
                .Include(cl => cl.Lesson)
                .OrderBy(cl => cl.OrderNo)
                .ToListAsync();

            if (!courseLessons.Any())
            {
                return NotFound("Không tìm thấy bài học cho khóa học này");
            }

            var lessonsDTO = courseLessons.Select(cl => new LessonDTO
            {
                LessonId = cl.Lesson.LessonId,
                LessonTitle = cl.Lesson.LessonTitle,
                LessonContent = cl.Lesson.LessonContent,
                LessonDescription = cl.Lesson.LessonDescription,
                Duration = cl.Lesson.Duration,
                DurationMinute = cl.Lesson.DurationMinute,
                IsActivate = cl.Lesson.IsActivate,
                UrlImageLesson = cl.Lesson.UrlImageLesson
            }).ToList();

            return Ok(lessonsDTO);
        }

        // POST api/QuanLyBaiHoc/AddLessonToCourse
        [Authorize(Roles ="Admin")]
        [HttpPost("AddLessonToCourse")]
        public async Task<IActionResult> AddLessonToCourse([FromBody] CourseLessonsDTO dto)
        {
            if (dto is null || dto.CourseId <= 0 || dto.LessonId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Kiểm tra khóa học và bài học có tồn tại không
            bool courseOk = await db.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            bool lessonOk = await db.Lessons.AnyAsync(l => l.LessonId == dto.LessonId);

            if (!courseOk)
                return NotFound("Khóa học không tìm thấy");
            if (!lessonOk)
                return NotFound("Bài học không tìm thấy");

            // Kiểm tra bài học đã có trong khóa học chưa
            bool dup = await db.CourseLessons
                .AnyAsync(cl => cl.CourseId == dto.CourseId && cl.LessonId == dto.LessonId);
            if (dup)
                return Conflict("Bài học đã có trong khóa học");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                // Lấy tất cả các bài học hiện tại trong khóa học, sắp xếp theo OrderNo
                var existingLessons = await db.CourseLessons
                    .Where(cl => cl.CourseId == dto.CourseId)
                    .OrderBy(cl => cl.OrderNo)
                    .ToListAsync();

                // Xác định OrderNo cho bài học mới
                int newOrderNo;
                if (dto.OrderNo.HasValue
                && dto.OrderNo.Value > 0)
                {
                    newOrderNo = dto.OrderNo.Value;
                }
                else
                {
                    newOrderNo = existingLessons.Count + 1; // Thêm vào cuối
                }

                // Chèn bài học mới và sắp xếp lại OrderNo
                var newRow = new CourseLesson
                {
                    CourseId = dto.CourseId,
                    LessonId = dto.LessonId,
                    OrderNo = newOrderNo
                };

                // Cập nhật OrderNo của các bài học hiện có
                for (int i = 0; i < existingLessons.Count; i++)
                {
                    if (existingLessons[i].OrderNo >= newOrderNo)
                    {
                        existingLessons[i].OrderNo++;
                    }
                }

                db.CourseLessons.Add(newRow);

                // Sắp xếp lại toàn bộ OrderNo để đảm bảo liên tục
                existingLessons = await db.CourseLessons
                    .Where(cl => cl.CourseId == dto.CourseId)
                    .OrderBy(cl => cl.OrderNo)
                    .ToListAsync();

                for (int i = 0; i < existingLessons.Count; i++)
                {
                    existingLessons[i].OrderNo = i + 1;
                }

                await db.SaveChangesAsync();
                await tran.CommitAsync();
                return Ok($"Đã thêm Lesson {dto.LessonId} vào Course {dto.CourseId} tại vị trí {newOrderNo}.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi thêm bài học: {ex.Message}");
            }
        }

        // DELETE: api/QuanLyBaiHoc/RemoveLessonFromCourse/{courseId}/{lessonId}
        // Xóa Lesson khỏi Course + sắp xếp lại OrderNo
        [Authorize(Roles ="Admin")]
        [HttpDelete("RemoveLessonFromCourse/{courseId}/{lessonId}")]
        public async Task<IActionResult> RemoveLessonFromCourse(int courseId, int lessonId)
        {
            if (courseId <= 0 || lessonId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Tìm bản ghi CourseLesson cần xóa
            var courseLesson = await db.CourseLessons
                .FirstOrDefaultAsync(cl => cl.CourseId == courseId && cl.LessonId == lessonId);

            if (courseLesson is null)
                return NotFound("Bài học không tồn tại trong khóa học.");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                // Xóa bản ghi
                db.CourseLessons.Remove(courseLesson);

                // Lấy tất cả các bài học còn lại trong khóa học, sắp xếp theo OrderNo
                var remainingLessons = await db.CourseLessons
                    .Where(cl => cl.CourseId == courseId)
                    .OrderBy(cl => cl.OrderNo)
                    .ToListAsync();

                // Sắp xếp lại OrderNo bắt đầu từ 1
                for (int i = 0; i < remainingLessons.Count; i++)
                {
                    remainingLessons[i].OrderNo = i + 1;
                }

                await db.SaveChangesAsync();
                await tran.CommitAsync();
                return Ok($"Đã xóa Lesson {lessonId} khỏi Course {courseId} và sắp xếp lại thứ tự.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi xóa bài học: {ex.Message}");
            }
        }

        //update thứ tự của bài học 
        // PUT: api/QuanLyBaiHoc/SwapLessonOrder
        [Authorize(Roles = "Admin")]
        [HttpPut("SwapLessonOrder")]
        public async Task<IActionResult> SwapLessonOrder([FromBody] SwapLessonOrderDTO dto)
        {
            // Kiểm tra đầu vào
            if (dto == null)
                return BadRequest("Dữ liệu DTO không được cung cấp.");
            if (dto.CourseId <= 0)
                return BadRequest("CourseId không hợp lệ.");
            if (dto.SourceOrderNo <= 0 || dto.TargetOrderNo <= 0)
                return BadRequest("SourceOrderNo hoặc TargetOrderNo không hợp lệ.");
            if (dto.SourceOrderNo == dto.TargetOrderNo)
                return BadRequest("SourceOrderNo và TargetOrderNo không được trùng nhau.");

            // Kiểm tra tồn tại hai bản ghi
            var lessons = await db.CourseLessons
                .Where(cl => cl.CourseId == dto.CourseId &&
                             (cl.OrderNo == dto.SourceOrderNo || cl.OrderNo == dto.TargetOrderNo))
                .ToListAsync();

            if (lessons.Count != 2)
                return NotFound("Không tìm thấy đủ hai bài học để hoán đổi.");

            // Sử dụng giao dịch
            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                // Lấy hai bản ghi
                var sourceLesson = lessons.First(cl => cl.OrderNo == dto.SourceOrderNo);
                var targetLesson = lessons.First(cl => cl.OrderNo == dto.TargetOrderNo);

                // Bước 1: Đặt SourceOrderNo thành giá trị tạm (-1)
                sourceLesson.OrderNo = -1;
                await db.SaveChangesAsync();

                // Bước 2: Đặt TargetOrderNo thành SourceOrderNo
                targetLesson.OrderNo = dto.SourceOrderNo;
                await db.SaveChangesAsync();

                // Bước 3: Đặt giá trị tạm (-1) thành TargetOrderNo
                sourceLesson.OrderNo = dto.TargetOrderNo;
                await db.SaveChangesAsync();
                // tác dụng xác nhận tất cả thay đổi
                await tran.CommitAsync();
                return Ok(
                    $"Đã hoán đổi vị trí {dto.SourceOrderNo} ↔ {dto.TargetOrderNo} cho Course {dto.CourseId}."
                );
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync(); // hủy khi lỗi
                return StatusCode(500, $"Lỗi khi hoán đổi: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateLessonOrder")]
        public async Task<IActionResult> UpdateLessonOrder([FromBody] CourseLessonsDTO dto)
        {
            if (dto == null || dto.OrderNo < 1)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var courseLesson = await db.CourseLessons
                    .FirstOrDefaultAsync(cl => cl.CourseId == dto.CourseId && cl.LessonId == dto.LessonId);
                if (courseLesson == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy bài học trong khóa học." });
                }

                var maxOrderNo = await db.CourseLessons
                    .Where(cl => cl.CourseId == dto.CourseId)
                    .MaxAsync(cl => (int?)cl.OrderNo) ?? 0;
                if (dto.OrderNo > maxOrderNo)
                {
                    return BadRequest(new { success = false, message = $"Thứ tự phải từ 1 đến {maxOrderNo}." });
                }

                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    int oldOrderNo = courseLesson?.OrderNo ?? 0;

                    if (oldOrderNo == dto.OrderNo)
                    {
                        return Ok(new { success = true, message = "Không cần cập nhật vì thứ tự không thay đổi." });
                    }

                    // Step 1: Set the target lesson's OrderNo to a temporary value to avoid conflict
                    courseLesson.OrderNo = -1;
                    await db.SaveChangesAsync();

                    // Step 2: Shift other lessons to accommodate the new OrderNo
                    var lessonsToShift = await db.CourseLessons
                        .Where(cl => cl.CourseId == dto.CourseId && cl.LessonId != dto.LessonId)
                        .OrderBy(cl => cl.OrderNo)
                        .ToListAsync();

                    if (oldOrderNo < dto.OrderNo)
                    {
                        // Moving down (e.g., from 2 to 4): Shift lessons between oldOrderNo and newOrderNo up
                        foreach (var lesson in lessonsToShift.Where(cl => cl.OrderNo > oldOrderNo && cl.OrderNo <= dto.OrderNo))
                        {
                            lesson.OrderNo--;
                        }
                    }
                    else
                    {
                        // Moving up (e.g., from 4 to 2): Shift lessons between newOrderNo and oldOrderNo down
                        foreach (var lesson in lessonsToShift.Where(cl => cl.OrderNo >= dto.OrderNo && cl.OrderNo < oldOrderNo))
                        {
                            lesson.OrderNo++;
                        }
                    }

                    // Step 3: Set the target lesson's OrderNo to the new value
                    courseLesson.OrderNo = dto.OrderNo.Value;
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = $"Cập nhật thứ tự bài học {dto.LessonId} thành {dto.OrderNo}." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

    }
}