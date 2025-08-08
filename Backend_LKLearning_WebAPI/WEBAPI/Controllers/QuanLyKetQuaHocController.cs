using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEBAPI.DTOS;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyKetQuaHocController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyKetQuaHocController(LuanvantienganhContext context)
        {
            db = context;
        }

        // GET: api/QuanLyKetQuaHoc/GetAcademicResults
        // Lấy danh sách kết quả học tập (có thể lọc theo UserID, CourseID, LessonID)
        // Get list of academic results (can filter by UserID, CourseID, LessonID)
        [HttpGet("GetAcademicResults")]
        public async Task<IActionResult> GetAcademicResults([FromQuery] int? userId, [FromQuery] int? courseId, [FromQuery] int? lessonId)
        {
            // Xây dựng truy vấn cơ bản / Build basic query
            var query = db.AcademicResults
                .Include(ar => ar.User)
                .Include(ar => ar.Course)
                .Include(ar => ar.Lesson)
                .AsQueryable();

            // Lọc theo UserID nếu có / Filter by UserID if provided
            if (userId.HasValue)
            {
                query = query.Where(ar => ar.UserId == userId.Value);
            }

            // Lọc theo CourseID nếu có / Filter by CourseID if provided
            if (courseId.HasValue)
            {
                query = query.Where(ar => ar.CourseId == courseId.Value);
            }

            // Lọc theo LessonID nếu có / Filter by LessonID if provided
            if (lessonId.HasValue)
            {
                query = query.Where(ar => ar.LessonId == lessonId.Value);
            }

            // Lấy danh sách và ánh xạ sang DTO / Get list and map to DTO
            var academicResults = await query
                .Select(ar => new AcademicResultDTO
                {
                    AcademicResultId = ar.AcademicResultId,
                    UserId = ar.UserId,
                    FullName = ar.User.Fullname,
                    CourseId = ar.CourseId,
                    CourseName = ar.Course.CourseName,
                    LessonId = ar.LessonId,
                    LessonTitle = ar.Lesson.LessonTitle,
                    Status = ar.Status,
                    TimeSpent = ar.TimeSpent ?? 0,
                    CreatedAt = ar.CreatedAt ?? DateTime.Now,
                    UpdatedAt = ar.UpdatedAt
                })
                .ToListAsync();

            // Trả về danh sách hoặc thông báo nếu không có dữ liệu / Return list or message if empty
            if (!academicResults.Any())
            {
                return NotFound("Không tìm thấy kết quả học tập / No academic results found");
            }

            return Ok(academicResults);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetAcademicResultsForUser")]
        public async Task<IActionResult> GetAcademicResultsForUser([FromQuery] int? courseId, [FromQuery] int? lessonId)
        {
            // Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            // Xây dựng truy vấn
            var query = db.AcademicResults
                .Include(ar => ar.User)
                .Include(ar => ar.Course)
                .Include(ar => ar.Lesson)
                .Where(ar => ar.UserId == userId) // Áp dụng luôn điều kiện userId
                .AsQueryable();

            // Lọc theo courseId nếu có
            if (courseId.HasValue)
            {
                query = query.Where(ar => ar.CourseId == courseId.Value);
            }

            // Lọc theo lessonId nếu có
            if (lessonId.HasValue)
            {
                query = query.Where(ar => ar.LessonId == lessonId.Value);
            }

            // Ánh xạ sang DTO
            var academicResults = await query
                .Select(ar => new AcademicResultDTO
                {
                    AcademicResultId = ar.AcademicResultId,
                    UserId = ar.UserId,
                    FullName = ar.User.Fullname,
                    CourseId = ar.CourseId,
                    CourseName = ar.Course.CourseName,
                    LessonId = ar.LessonId,
                    LessonTitle = ar.Lesson.LessonTitle,
                    Status = ar.Status,
                    TimeSpent = ar.TimeSpent ?? 0,
                    CreatedAt = ar.CreatedAt ?? DateTime.Now,
                    UpdatedAt = ar.UpdatedAt
                })
                .ToListAsync();

            if (!academicResults.Any())
            {
                return NotFound("Không tìm thấy kết quả học tập.");
            }

            return Ok(academicResults);
        }




        // GET: api/QuanLyKetQuaHoc/CheckUserReviewStatus
        // Kiểm tra trạng thái hoàn thành khóa học để nhận xét
        // Check if user has completed all lessons in the course for review
        [Authorize(Roles = "Admin,User")]
        [HttpGet("CheckUserReviewStatus")]
        public async Task<IActionResult> CheckUserReviewStatus([FromQuery] int userId, [FromQuery] int courseId)
        {
            // Kiểm tra UserID và CourseID có tồn tại
            bool userExists = await db.Users.AnyAsync(u => u.UserId == userId);
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!userExists || !courseExists)
            {
                return NotFound("Người dùng hoặc khóa học không tồn tại / User or course not found");
            }

            // Lấy danh sách bài học trong khóa học
            var courseLessons = await db.CourseLessons
                .Where(cl => cl.CourseId == courseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            if (!courseLessons.Any())
            {
                return NotFound("Khóa học không có bài học nào / Course has no lessons");
            }

            // Lấy kết quả học tập của người dùng cho khóa học
            var completedLessons = await db.AcademicResults
                .Where(ar => ar.UserId == userId && ar.CourseId == courseId && ar.Status == "Completed")
                .Select(ar => ar.LessonId)
                .ToListAsync();

            // Kiểm tra xem tất cả bài học đã hoàn thành chưa
            bool allCompleted = courseLessons.All(lessonId => completedLessons.Contains(lessonId));

            if (allCompleted)
            {
                return Ok(new
                {
                    Message = "Bạn đã hoàn thành khóa học. / You have completed the course.",
                    CanReview = true
                });
            }

            return Ok(new
            {
                Message = "Bạn cần hoàn thành hết các bài học để nhận xét. / You need to complete all lessons to review.",
                CanReview = false
            });
        }


        // GET: api/QuanLyKetQuaHoc/CheckCourseProgress?userId={userId}&courseId={courseId}
        // Kiểm tra tiến độ khóa học và thông báo bài học chưa hoàn thành
        // Check course progress and notify about incomplete lessons

        [HttpGet("CheckCourseProgress")]
        public async Task<IActionResult> CheckCourseProgress([FromQuery] int userId, [FromQuery] int courseId)
        {
            // Kiểm tra UserID và CourseID có tồn tại / Check if UserID and CourseID exist
            bool userExists = await db.Users.AnyAsync(u => u.UserId == userId);
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);

            if (!userExists || !courseExists)
            {
                return NotFound("Người dùng hoặc khóa học không tồn tại / User or course not found");
            }

            // Lấy danh sách bài học trong khóa học, sắp xếp theo OrderNo
            // Get list of lessons in the course, sorted by OrderNo
            var courseLessons = await db.CourseLessons
                .Where(cl => cl.CourseId == courseId)
                .Include(cl => cl.Lesson)
                .OrderBy(cl => cl.OrderNo)
                .ToListAsync();

            if (!courseLessons.Any())
            {
                return NotFound("Khóa học không có bài học nào / Course has no lessons");
            }

            // Lấy kết quả học tập của người dùng cho khóa học, lấy bản ghi mới nhất cho mỗi bài học
            var academicResults = await db.AcademicResults
                .Where(ar => ar.UserId == userId && ar.CourseId == courseId)
                .GroupBy(ar => ar.LessonId)
                .Select(g => g.OrderByDescending(ar => ar.CreatedAt).FirstOrDefault())
                .ToListAsync();

            // Tạo danh sách bài học chưa hoàn thành (chỉ bao gồm khi không phải Completed)
            var incompleteLessons = new List<dynamic>();
            foreach (var lesson in courseLessons)
            {
                var academicResult = academicResults.FirstOrDefault(ar => ar != null && ar.LessonId == lesson.LessonId);
                string status = academicResult?.Status ?? "NotStarted";

                if (status != "Completed") // Chỉ thêm khi không phải Completed
                {
                    incompleteLessons.Add(new
                    {
                        LessonId = lesson.LessonId,
                        LessonTitle = lesson.Lesson?.LessonTitle ?? "Unknown",
                        OrderNo = lesson.OrderNo,
                        Status = status,
                        Notification = $"Bạn cần hoàn thiện bài học {lesson.Lesson?.LessonTitle ?? "Unknown"} (Thứ tự {lesson.OrderNo}) hiện đang {status}. / You need to complete lesson {lesson.Lesson?.LessonTitle ?? "Unknown"} (Order {lesson.OrderNo}) currently {status}."
                    });
                }
            }

            // Kiểm tra nếu tất cả bài học đều hoàn thành
            if (!incompleteLessons.Any())
            {
                return Ok(new
                {
                    Message = "Chúc mừng! Bạn đã hoàn thành tất cả bài học trong khóa học. / Congratulations! You have completed all lessons in the course.",
                    Completed = true
                });
            }

            // Xác định bài học tiếp theo (bài học chưa hoàn thành có OrderNo nhỏ nhất)
            var nextLesson = incompleteLessons
                .OrderBy(il => il.OrderNo)
                .FirstOrDefault();

            return Ok(new
            {
                Message = "Bạn cần hoàn thiện các bài học sau đây. / You need to complete the following lessons.",
                IncompleteLessons = incompleteLessons,
                NextLesson = nextLesson != null ? new
                {
                    LessonId = nextLesson.LessonId,
                    LessonTitle = nextLesson.LessonTitle,
                    OrderNo = nextLesson.OrderNo,
                    Status = nextLesson.Status
                } : null
            });
        }

        // GET: api/QuanLyKetQuaHoc/GetAcademicResultById/{id}
        // Lấy chi tiết kết quả học tập theo ID / Get academic result details by ID
        [HttpGet("GetAcademicResultById/{id}")]
        public async Task<IActionResult> GetAcademicResultById(int id)
        {
            // Tìm bản ghi theo ID / Find record by ID
            var academicResult = await db.AcademicResults
                .Include(ar => ar.User)
                .Include(ar => ar.Course)
                .Include(ar => ar.Lesson)
                .Select(ar => new AcademicResultDTO
                {
                    AcademicResultId = ar.AcademicResultId,
                    UserId = ar.UserId,
                    FullName = ar.User.Fullname,
                    CourseId = ar.CourseId,
                    CourseName = ar.Course.CourseName,
                    LessonId = ar.LessonId,
                    LessonTitle = ar.Lesson.LessonTitle,
                    Status = ar.Status,
                    TimeSpent = ar.TimeSpent ?? 0,
                    CreatedAt = ar.CreatedAt ?? DateTime.Now,
                    UpdatedAt = ar.UpdatedAt
                })
                .FirstOrDefaultAsync(ar => ar.AcademicResultId == id);

            // Kiểm tra nếu không tìm thấy / Check if not found
            if (academicResult == null)
            {
                return NotFound("Không tìm thấy kết quả học tập / Academic result not found");
            }

            return Ok(academicResult);
        }

        // POST: api/QuanLyKetQuaHoc/CreateAcademicResult
        // Thêm mới kết quả học tập / Create new academic result

        [HttpPost("CreateAcademicResult")]
        public async Task<IActionResult> CreateAcademicResult([FromBody] AcademicResultDTO academicResultDTO)
        {
            // Kiểm tra dữ liệu đầu vào / Validate input data
            if (academicResultDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ / Invalid data");
            }

            // Kiểm tra UserID, CourseID, LessonID có tồn tại / Check if UserID, CourseID, LessonID exist
            bool userExists = await db.Users.AnyAsync(u => u.UserId == academicResultDTO.UserId);
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == academicResultDTO.CourseId);
            bool lessonExists = await db.Lessons.AnyAsync(l => l.LessonId == academicResultDTO.LessonId);

            if (!userExists || !courseExists || !lessonExists)
            {
                return NotFound("Người dùng, khóa học hoặc bài học không tồn tại / User, course, or lesson not found");
            }

            // Kiểm tra xem bài học có thuộc khóa học không / Check if lesson belongs to course
            bool lessonInCourse = await db.CourseLessons
                .AnyAsync(cl => cl.CourseId == academicResultDTO.CourseId && cl.LessonId == academicResultDTO.LessonId);
            if (!lessonInCourse)
            {
                return BadRequest("Bài học không thuộc khóa học / Lesson does not belong to course");
            }

            // Kiểm tra trạng thái hợp lệ / Validate status
            if (!new[] { "Completed", "InProgress", "Failed" }.Contains(academicResultDTO.Status))
            {
                return BadRequest("Trạng thái không hợp lệ. Phải là 'Completed', 'InProgress' hoặc 'Failed' / Invalid status. Must be 'Completed', 'InProgress', or 'Failed'");
            }

            // Kiểm tra xem kết quả học tập đã tồn tại chưa / Check if academic result already exists
            bool resultExists = await db.AcademicResults
                .AnyAsync(ar => ar.UserId == academicResultDTO.UserId
                             && ar.CourseId == academicResultDTO.CourseId
                             && ar.LessonId == academicResultDTO.LessonId);
            if (resultExists)
            {
                return Conflict("Kết quả học tập đã tồn tại / Academic result already exists");
            }

            // Ánh xạ từ DTO sang Entity / Map DTO to Entity
            var academicResult = new AcademicResult
            {
                UserId = academicResultDTO.UserId,
                CourseId = academicResultDTO.CourseId,
                LessonId = academicResultDTO.LessonId,
                Status = academicResultDTO.Status,
                TimeSpent = academicResultDTO.TimeSpent,
                CreatedAt = DateTime.Now,
                UpdatedAt = academicResultDTO.UpdatedAt
            };

            // Thêm vào database / Add to database
            try
            {
                db.AcademicResults.Add(academicResult);
                await db.SaveChangesAsync();
                academicResultDTO.AcademicResultId = academicResult.AcademicResultId;
                return Ok(academicResultDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi thêm kết quả học tập: {ex.Message} / Error creating academic result: {ex.Message}");
            }
        }

        // PUT: api/QuanLyKetQuaHoc/UpdateAcademicResult/{id}
        // Cập nhật kết quả học tập theo ID / Update academic result by ID
        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateAcademicResult/{id}")]
        public async Task<IActionResult> UpdateAcademicResult(int id, [FromBody] AcademicResultDTO academicResultDTO)
        {
            // Kiểm tra dữ liệu đầu vào / Validate input data
            if (academicResultDTO == null || id != academicResultDTO.AcademicResultId)
            {
                return BadRequest("Dữ liệu không hợp lệ / Invalid data");
            }

            // Tìm bản ghi hiện có / Find existing record
            var existingResult = await db.AcademicResults.FindAsync(id);
            if (existingResult == null)
            {
                return NotFound("Không tìm thấy kết quả học tập / Academic result not found");
            }

            // Kiểm tra trạng thái hợp lệ / Validate status
            if (!new[] { "Completed", "InProgress", "Failed" }.Contains(academicResultDTO.Status))
            {
                return BadRequest("Trạng thái không hợp lệ. Phải là 'Completed', 'InProgress' hoặc 'Failed' / Invalid status. Must be 'Completed', 'InProgress', or 'Failed'");
            }

            // Cập nhật các trường / Update fields
            existingResult.Status = academicResultDTO.Status;
            existingResult.TimeSpent = academicResultDTO.TimeSpent;
            existingResult.UpdatedAt = DateTime.Now;

            // Lưu thay đổi / Save changes
            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật kết quả học tập thành công / Academic result updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi cập nhật kết quả học tập: {ex.Message} / Error updating academic result: {ex.Message}");
            }
        }


        // PUT: api/QuanLyKetQuaHoc/UpdateAcademicResultForUser
        // Cập nhật kết quả học tập dựa trên UserId (từ JWT), CourseId, LessonId, Status
        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateAcademicResultForUser")]
        public async Task<IActionResult> UpdateAcademicResultForUser([FromQuery] int lessonId, [FromQuery] int courseId, [FromQuery] string status)
        {
            // Lấy UserId từ JWT token  
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Không thể xác định người dùng / Unable to identify user");
            }

            // Kiểm tra trạng thái hợp lệ
            if (!new[] { "Completed", "InProgress", "Failed" }.Contains(status))
            {
                return BadRequest("Trạng thái không hợp lệ. Phải là 'Completed', 'InProgress' hoặc 'Failed' / Invalid status. Must be 'Completed', 'InProgress', or 'Failed'");
            }

            // Kiểm tra CourseId và LessonId có tồn tại
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);
            bool lessonExists = await db.Lessons.AnyAsync(l => l.LessonId == lessonId);
            if (!courseExists || !lessonExists)
            {
                return NotFound("Khóa học hoặc bài học không tồn tại / Course or lesson not found");
            }

            // Kiểm tra xem bài học có thuộc khóa học không
            bool lessonInCourse = await db.CourseLessons
                .AnyAsync(cl => cl.CourseId == courseId && cl.LessonId == lessonId);
            if (!lessonInCourse)
            {
                return BadRequest("Bài học không thuộc khóa học / Lesson does not belong to course");
            }

            // Tìm bản ghi AcademicResult
            var existingResult = await db.AcademicResults
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.CourseId == courseId && ar.LessonId == lessonId);
            if (existingResult == null)
            {
                return NotFound("Không tìm thấy kết quả học tập / Academic result not found");
            }

            // Cập nhật trạng thái và UpdatedAt
            existingResult.Status = status;
            existingResult.UpdatedAt = DateTime.Now;

            // Lưu thay đổi
            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật kết quả học tập thành công / Academic result updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Lỗi khi cập nhật kết quả học tập: {ex.Message} / Error updating academic result: {ex.Message}");
            }
        }

        // DELETE: api/QuanLyKetQuaHoc/DeleteAcademicResult/{id}
        // Xóa kết quả học tập theo ID / Delete academic result by ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteAcademicResult/{id}")]
        public async Task<IActionResult> DeleteAcademicResult(int id)
        {
            // Tìm bản ghi / Find record
            var academicResult = await db.AcademicResults.FindAsync(id);
            if (academicResult == null)
            {
                return NotFound("Không tìm thấy kết quả học tập / Academic result not found");
            }

            // Xóa bản ghi / Delete record
            try
            {
                db.AcademicResults.Remove(academicResult);
                await db.SaveChangesAsync();
                return Ok("Xóa kết quả học tập thành công / Academic result deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa kết quả học tập: {ex.Message} / Error deleting academic result: {ex.Message}");
            }
        }
    }
}