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
    public class QuanLyReviewController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyReviewController(LuanvantienganhContext context)
        {
            db = context;
        }


        [Authorize(Roles = "User")]
        [HttpPost("CreateReviewCourseIDforuser")]
        public async Task<IActionResult> CreateReviewCourseIDforuser([FromBody] ReviewDTO reviewDTO)
        {
            // Validate input
            if (reviewDTO == null || reviewDTO.CourseId <= 0 || reviewDTO.Rating < 1 || reviewDTO.Rating > 5)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Kiểm tra khóa học có tồn tại không
            var course = await db.Courses.FirstOrDefaultAsync(c => c.CourseId == reviewDTO.CourseId);
            if (course == null)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            // Đếm tổng số bài học trong khóa học
            int totalLessons = await db.CourseLessons
                .CountAsync(cl => cl.CourseId == reviewDTO.CourseId);
            if (totalLessons == 0)
            {
                return BadRequest("Khóa học không có bài học nào.");
            }

            // Đếm số bài học đã hoàn thành
            int completedLessons = await db.AcademicResults
                .CountAsync(ar => ar.UserId == userId
                               && ar.CourseId == reviewDTO.CourseId
                               && ar.Status == "Completed");

            // So sánh số bài học đã hoàn thành với tổng số bài học
            if (completedLessons != totalLessons)
            {
                return BadRequest("Bạn phải hoàn thành tất cả bài học trong khóa học trước khi viết hoặc cập nhật đánh giá.");
            }

            // Kiểm tra xem người dùng đã có đánh giá chưa
            var existingReview = await db.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId
                                       && r.CourseId == reviewDTO.CourseId
                                       && r.ReviewType == "1");

            try
            {
                if (existingReview != null)
                {
                    // Cập nhật đánh giá hiện có
                    existingReview.Rating = reviewDTO.Rating;
                    existingReview.Comment = reviewDTO.Comment;
                    existingReview.ReviewType = "1";
                    existingReview.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Tạo đánh giá mới
                    var review = new Review
                    {
                        UserId = userId,
                        CourseId = reviewDTO.CourseId,
                        LessonId = null, // Set to null for course review
                        ReviewType = "1", // Default value for course review
                        Rating = reviewDTO.Rating,
                        Comment = reviewDTO.Comment,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Reviews.Add(review);
                }

                await db.SaveChangesAsync();
                return Ok("Đánh giá của bạn đã được ghi nhận .");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo/cập nhật đánh giá: {ex.Message}");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("CheckUserReviewStatus/{courseId}")]
        public async Task<IActionResult> CheckUserReviewStatus(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("CourseId không hợp lệ.");
            }

            var course = await db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            int totalLessons = await db.CourseLessons
                .CountAsync(cl => cl.CourseId == courseId);
            if (totalLessons == 0)
            {
                return BadRequest("Khóa học không có bài học nào.");
            }

            int completedLessons = await db.AcademicResults
                .CountAsync(ar => ar.UserId == userId
                               && ar.CourseId == courseId
                               && ar.Status == "Completed");

            if (completedLessons != totalLessons)
            {
                return Ok("Bạn cần hoàn thành hết các bài học có trong khóa học này.");
            }

            bool alreadyReviewed = await db.Reviews
                .AnyAsync(r => r.UserId == userId
                            && r.CourseId == courseId
                            && r.ReviewType == "1");

            if (alreadyReviewed)
            {
                return Ok("Bạn đã viết nhận xét cho khóa học này. Bạn có thể chỉnh sửa nhận xét.");
            }

            return Ok("Bạn đã hoàn thành tất cả bài học.");
        }

        //giới hạn cho user mỗi ngày chỉ được viết góp ý 1 lần duy nhất 
        [Authorize(Roles = "User")]
        [HttpPost("CreateReportLessonforuser")]
        public async Task<IActionResult> CreateReportLessonforuser([FromBody] ReviewDTO reportDTO)
        {
            // Validate input
            if (reportDTO == null || reportDTO.CourseId <= 0 || reportDTO.LessonId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            // Kiểm tra tồn tại khóa/bài
            if (!await db.Courses.AnyAsync(c => c.CourseId == reportDTO.CourseId) ||
                !await db.Lessons.AnyAsync(l => l.LessonId == reportDTO.LessonId))
            {
                return NotFound("Khóa học hoặc bài học không tồn tại.");
            }

            // Kiểm tra đã hoàn thành bài hay chưa
            var academicResult = await db.AcademicResults
                .FirstOrDefaultAsync(ar =>
                    ar.UserId == userId &&
                    ar.CourseId == reportDTO.CourseId &&
                    ar.LessonId == reportDTO.LessonId);

            if (academicResult == null || academicResult.Status != "Completed")
                return BadRequest("Bạn phải hoàn thành bài học này trước khi viết báo cáo.");

            // ——— Bước mới: Giới hạn 1 lần/ngày ———
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            bool hasReportedToday = await db.Reviews.AnyAsync(r =>
                r.UserId == userId &&
                r.CourseId == reportDTO.CourseId &&
                r.LessonId == reportDTO.LessonId &&
                r.ReviewType == "2" &&
                r.CreatedAt >= today && r.CreatedAt < tomorrow
            );

            if (hasReportedToday)
                return Conflict("Bạn chỉ được tạo 1 báo cáo cho bài học này mỗi ngày.");

            // Tạo báo cáo mới
            var review = new Review
            {
                UserId = userId,
                CourseId = reportDTO.CourseId,
                LessonId = reportDTO.LessonId,
                ReviewType = "2",
                Rating = 1,
                Comment = reportDTO.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                return Ok("Báo cáo bài học đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Lỗi khi tạo báo cáo: {ex.Message}");
            }
        }


        [Authorize(Roles = "User")]
        [HttpGet("GetUserReportsByCourseAndLesson")]
        public async Task<IActionResult> GetUserReportsByCourseAndLesson(int courseId, int lessonId)
        {
            if (courseId <= 0 || lessonId <= 0)
                return BadRequest("CourseId và LessonId phải là số nguyên dương.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Không lấy được thông tin người dùng từ token.");

            if (!await db.Courses.AnyAsync(c => c.CourseId == courseId) ||
                !await db.Lessons.AnyAsync(l => l.LessonId == lessonId))
                return NotFound("Khóa học hoặc bài học không tồn tại.");

            try
            {
                var reports = await db.Reviews
                    .Where(r =>
                        r.UserId == userId &&
                        r.CourseId == courseId &&
                        r.LessonId == lessonId &&
                        r.ReviewType == "2")
                    .OrderByDescending(r => r.CreatedAt)    // ← sắp xếp mới nhất lên đầu
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.UserId,
                        r.CourseId,
                        r.LessonId,
                        r.Comment,
                        r.CreatedAt,
                        r.UpdatedAt
                    })
                    .ToListAsync();

                if (reports.Count == 0)
                    return NotFound("Không tìm thấy báo cáo nào của người dùng cho khóa học và bài học này.");

                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Lỗi khi lấy báo cáo: {ex.Message}");
            }
        }



































        //===================Review======================================================   
        [Authorize(Roles = "User,Admin")]
        [HttpPost("CreateReviewCourseID")]
        public async Task<IActionResult> CreateReviewCourseID([FromBody] ReviewDTO reviewDTO)
        {
            // Validate input
            if (reviewDTO == null || reviewDTO.CourseId <= 0 || reviewDTO.Rating < 1 || reviewDTO.Rating > 5)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            // Get userId from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Determine UserId to use
            int userId = tokenUserId;
            if (User.IsInRole("Admin"))
            {
                if (reviewDTO.UserId <= 0)
                {
                    return BadRequest("Admin phải cung cấp UserId hợp lệ.");
                }
                // Verify the provided UserId exists
                bool userExists = await db.Users.AnyAsync(u => u.UserId == reviewDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Người dùng được chọn không tồn tại.");
                }
                userId = reviewDTO.UserId;
            }
            else if (reviewDTO.UserId != tokenUserId)
            {
                return Forbid("Người dùng không phải admin không thể tạo báo cáo cho người khác.");
            }

            // Check if course exists
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == reviewDTO.CourseId);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            // Step 1: Count total lessons in the course using CourseLesson
            int totalLessons = await db.CourseLessons
                .CountAsync(cl => cl.CourseId == reviewDTO.CourseId);
            if (totalLessons == 0)
            {
                return BadRequest("Khóa học không có bài học nào.");
            }

            // Step 2: Count completed lessons in AcademicResult
            int completedLessons = await db.AcademicResults
                .CountAsync(ar => ar.UserId == userId
                               && ar.CourseId == reviewDTO.CourseId
                               && ar.Status == "Completed");

            // Step 3: Compare total lessons with completed lessons (only for non-admin)
            if (!User.IsInRole("Admin") && completedLessons != totalLessons)
            {
                return BadRequest("Bạn phải hoàn thành tất cả bài học trong khóa học trước khi viết đánh giá.");
            }

            // Check if user has already reviewed this course
            bool alreadyReviewed = await db.Reviews
                .AnyAsync(r => r.UserId == userId && r.CourseId == reviewDTO.CourseId && r.ReviewType == "1");
            if (alreadyReviewed)
            {
                return Conflict("Bạn đã viết đánh giá cho khóa học này rồi.");
            }

            // Create the review
            var review = new Review
            {
                UserId = userId,
                CourseId = reviewDTO.CourseId,
                LessonId = null, // Set to null for course review
                ReviewType = "1", // Default value for course review
                Rating = reviewDTO.Rating,
                Comment = reviewDTO.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                return Ok("Đánh giá khóa học đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo đánh giá: {ex.Message}");
            }
        }

        [HttpGet("GetDSReport")]
        public async Task<IActionResult> GetDSReport()
        {
            try
            {
                var reports = await db.Reviews
                    .Where(r => r.ReviewType == "2" || r.ReviewType == "3")
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        CourseId = r.CourseId,
                        LessonId = r.LessonId,
                        ReviewType = r.ReviewType,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault(),
                        CourseName = db.Courses.Where(u => u.CourseId == r.CourseId).Select(u => u.CourseName).FirstOrDefault(),
                        LessonName = db.Lessons.Where(l => l.LessonId == r.LessonId).Select(l => l.LessonTitle).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("GetDSReviewByCourse")]
        public async Task<IActionResult> GetDSReviewByCourse()
        {
            try
            {
                var reviews = await db.Reviews
                    .Where(r => r.ReviewType == "1")
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        UserId = r.UserId,
                        CourseId = r.CourseId,
                        LessonId = r.LessonId,
                        ReviewType = r.ReviewType,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault(),
                        CourseName = db.Courses.Where(u => u.CourseId == r.CourseId).Select(u => u.CourseName).FirstOrDefault(),
                        LessonName = db.Lessons.Where(l => l.LessonId == r.LessonId).Select(l => l.LessonTitle).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetReviewsByCourseId/{courseId}")]
        public async Task<IActionResult> GetReviewsByCourseId(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("CourseId không hợp lệ.");
            }

            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            var reviews = await db.Reviews
                .Where(r => r.CourseId == courseId && r.ReviewType == "1")
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    LessonId = r.LessonId,
                    ReviewType = r.ReviewType,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault(),
                    UrlAvatar = db.Avatars.Where(a => a.AvatarId == db.Users.Where(u => u.UserId == r.UserId).Select(u => u.AvatarId).FirstOrDefault()).Select(a => a.UrlPath).FirstOrDefault(),




                })
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return NotFound("Không tìm thấy đánh giá nào cho khóa học này.");
            }

            return Ok(reviews);
        }

        //==========Report=======================
        [Authorize(Roles = "User,Admin")]
        [HttpPost("CreateReportLesson")]
        public async Task<IActionResult> CreateReportLesson([FromBody] ReviewDTO reportDTO)
        {
            // Validate input
            if (reportDTO == null || reportDTO.CourseId <= 0 || reportDTO.LessonId <= 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Get userId from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Determine UserId to use
            int userId = tokenUserId;
            if (User.IsInRole("Admin"))
            {
                if (reportDTO.UserId <= 0)
                {
                    return BadRequest("Admin phải cung cấp UserId hợp lệ.");
                }
                // Verify the provided UserId exists
                bool userExists = await db.Users.AnyAsync(u => u.UserId == reportDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Người dùng được chọn không tồn tại.");
                }
                userId = reportDTO.UserId;
            }
            else if (reportDTO.UserId != tokenUserId)
            {
                return Forbid("Người dùng không phải admin không thể tạo báo cáo cho người khác.");
            }

            // Check if course and lesson exist
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == reportDTO.CourseId);
            bool lessonExists = await db.Lessons.AnyAsync(l => l.LessonId == reportDTO.LessonId);
            if (!courseExists || !lessonExists)
            {
                return NotFound("Khóa học hoặc bài học không tồn tại.");
            }

            // Check if the lesson is completed in AcademicResult (for non-admins)
            if (!User.IsInRole("Admin"))
            {
                var academicResult = await db.AcademicResults
                    .FirstOrDefaultAsync(ar => ar.UserId == userId
                                            && ar.CourseId == reportDTO.CourseId
                                            && ar.LessonId == reportDTO.LessonId);
                if (academicResult == null || academicResult.Status != "Completed")
                {
                    return BadRequest("Bạn phải hoàn thành bài học này trước khi viết báo cáo.");
                }
            }

            // Create the report
            var review = new Review
            {
                UserId = userId,
                CourseId = reportDTO.CourseId,
                LessonId = reportDTO.LessonId,
                ReviewType = "2",
                Rating = 1,
                Comment = reportDTO.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                return Ok("Báo cáo bài học đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo báo cáo: {ex.Message}");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("CreateReportCourse")]
        public async Task<IActionResult> CreateReportCourse([FromBody] ReviewDTO reportDTO)
        {
            // Validate input
            if (reportDTO == null || reportDTO.CourseId <= 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Get userId from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Determine UserId to use
            int userId = tokenUserId;
            if (User.IsInRole("Admin"))
            {
                if (reportDTO.UserId <= 0)
                {
                    return BadRequest("Admin phải cung cấp UserId hợp lệ.");
                }
                // Verify the provided UserId exists
                bool userExists = await db.Users.AnyAsync(u => u.UserId == reportDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Người dùng được chọn không tồn tại.");
                }
                userId = reportDTO.UserId;
            }
            else if (reportDTO.UserId != tokenUserId)
            {
                return Forbid("Người dùng không phải admin không thể tạo báo cáo cho người khác.");
            }

            // Check if course exists
            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == reportDTO.CourseId);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            // Check completion status (for non-admins)
            if (!User.IsInRole("Admin"))
            {
                int totalLessons = await db.CourseLessons
                    .CountAsync(cl => cl.CourseId == reportDTO.CourseId);
                if (totalLessons == 0)
                {
                    return BadRequest("Khóa học không có bài học nào.");
                }

                int completedLessons = await db.AcademicResults
                    .CountAsync(ar => ar.UserId == userId
                                   && ar.CourseId == reportDTO.CourseId
                                   && ar.Status == "Completed");

                if (completedLessons != totalLessons)
                {
                    return BadRequest("Bạn phải hoàn thành tất cả bài học trong khóa học trước khi viết báo cáo.");
                }
            }

            // Create the report
            var review = new Review
            {
                UserId = userId,
                CourseId = reportDTO.CourseId,
                LessonId = null,
                ReviewType = "3",
                Rating = 1,
                Comment = reportDTO.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                db.Reviews.Add(review);
                await db.SaveChangesAsync();
                return Ok("Báo cáo khóa học đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo báo cáo: {ex.Message}");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetById/{reviewId}")]
        public async Task<IActionResult> GetById(int reviewId)
        {
            if (reviewId <= 0)
                return BadRequest("ReviewId không hợp lệ.");

            var review = await db.Reviews
                .Where(r => r.ReviewId == reviewId)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    LessonId = r.LessonId,
                    ReviewType = r.ReviewType,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault(),
                    CourseName = db.Courses.Where(c => c.CourseId == r.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                    LessonName = db.Lessons.Where(l => l.LessonId == r.LessonId).Select(l => l.LessonTitle).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (review == null)
                return NotFound("Không tìm thấy đánh giá/báo cáo.");

            return Ok(review);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("UpdateReview/{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] ReviewDTO dto)
        {
            if (dto == null || reviewId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            var review = await db.Reviews.FindAsync(reviewId);
            if (review == null)
                return NotFound("Không tìm thấy đánh giá/báo cáo.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && review.UserId != userId)
                return Forbid("Bạn không có quyền sửa đánh giá/báo cáo này.");

            review.Comment = dto.Comment;
            review.Rating = dto.Rating;
            review.UpdatedAt = DateTime.Now;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("DeleteReview/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            if (reviewId <= 0)
                return BadRequest("ReviewId không hợp lệ.");

            var review = await db.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
                return NotFound("Không tìm thấy đánh giá/báo cáo.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && review.UserId != userId)
                return Forbid("Bạn không có quyền xóa đánh giá/báo cáo này.");

            try
            {
                db.Reviews.Remove(review);
                await db.SaveChangesAsync();
                return Ok("Đánh giá/báo cáo đã được xóa thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa: {ex.Message}");
            }
        }

        [HttpGet("GetDSReviewByCoursePaged")]
        public async Task<IActionResult> GetDSReviewByCoursePaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Page hoặc pageSize không hợp lệ.");

            var query = db.Reviews.Where(r => r.ReviewType == "1");
            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    LessonId = r.LessonId,
                    ReviewType = r.ReviewType,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault(),
                    CourseName = db.Courses.Where(u => u.CourseId == r.CourseId).Select(u => u.CourseName).FirstOrDefault(),
                    LessonName = db.Lessons.Where(l => l.LessonId == r.LessonId).Select(l => l.LessonTitle).FirstOrDefault()
                })
                .ToListAsync();

            var response = new
            {
                Data = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetReportsByLessonId/{lessonId}")]
        public async Task<IActionResult> GetReportsByLessonId(int lessonId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (lessonId <= 0 || page < 1 || pageSize < 1)
            {
                return BadRequest("LessonId, page hoặc pageSize không hợp lệ.");
            }

            bool lessonExists = await db.Lessons.AnyAsync(l => l.LessonId == lessonId);
            if (!lessonExists)
            {
                return NotFound("Bài học không tồn tại.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var isAdmin = User.IsInRole("Admin");

            var query = db.Reviews
                .Where(r => r.LessonId == lessonId && r.ReviewType == "2");

            if (!isAdmin)
            {
                query = query.Where(r => r.UserId == userId);
            }

            var totalCount = await query.CountAsync();

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    LessonId = r.LessonId,
                    ReviewType = r.ReviewType,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault()
                })
                .ToListAsync();

            if (reports == null || !reports.Any())
            {
                return NotFound("Không tìm thấy báo cáo nào cho bài học này.");
            }

            var response = new
            {
                Data = reports,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetReportsByCourseId/{courseId}")]
        public async Task<IActionResult> GetReportsByCourseId(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (courseId <= 0 || page < 1 || pageSize < 1)
            {
                return BadRequest("CourseId, page hoặc pageSize không hợp lệ.");
            }

            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var isAdmin = User.IsInRole("Admin");

            var query = db.Reviews
                .Where(r => r.CourseId == courseId && r.ReviewType == "3");

            if (!isAdmin)
            {
                query = query.Where(r => r.UserId == userId);
            }

            var totalCount = await query.CountAsync();

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    LessonId = r.LessonId,
                    ReviewType = r.ReviewType,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Name = db.Users.Where(u => u.UserId == r.UserId).Select(u => u.Fullname).FirstOrDefault()
                })
                .ToListAsync();

            if (reports == null || !reports.Any())
            {
                return NotFound("Không tìm thấy báo cáo nào cho khóa học này.");
            }

            var response = new
            {
                Data = reports,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetReviewStats/{courseId}")]
        public async Task<IActionResult> GetReviewStats(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("CourseId không hợp lệ.");
            }

            bool courseExists = await db.Courses.AnyAsync(c => c.CourseId == courseId);
            if (!courseExists)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            var reviews = await db.Reviews
                .Where(r => r.CourseId == courseId && r.ReviewType == "1")
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return NotFound("Không tìm thấy đánh giá nào cho khóa học này.");
            }

            var stats = new
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Average(r => r.Rating),
                RatingDistribution = reviews
                    .GroupBy(r => r.Rating)
                    .Select(g => new
                    {
                        Rating = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Rating)
                    .ToList()
            };

            return Ok(stats);
        }
    }
}
