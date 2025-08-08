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
    public class QuanLyExamController : ControllerBase
    {
        private readonly LuanvantienganhContext db;

        public QuanLyExamController(LuanvantienganhContext context)
        {
            db = context;
        }


        // Kiểm tra điều kiện làm bộ đề cho user hiện tại (theo courseId)
        [Authorize(Roles = "User")]
        [HttpGet("CanTakeExamSetForUser")]
        public async Task<IActionResult> CanTakeExamSetForUser([FromQuery] int courseId)
        {
            // Lấy thông tin user từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Lấy danh sách bài học trong khóa học
            var lessonIds = await db.CourseLessons
                .Where(cl => cl.CourseId == courseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            // Lấy danh sách bài học đã hoàn thành
            var completedLessons = await db.AcademicResults
                .Where(ar => ar.UserId == userId && ar.CourseId == courseId && ar.Status == "Completed")
                .Select(ar => ar.LessonId)
                .ToListAsync();

            bool allCompleted = lessonIds.All(lid => completedLessons.Contains(lid));

            if (!allCompleted)
            {
                return Ok(new
                {
                    CanTakeExam = false,
                    Message = "Bạn cần hoàn thành tất cả bài học trong khóa học trước khi làm bộ đề."
                });
            }

            return Ok(new
            {
                CanTakeExam = true,
                Message = "Bạn có thể làm bộ đề."
            });
        }


        // Lấy ngẫu nhiên 1 bộ đề theo khóa học
        [HttpGet("GetRandomExamSetByCourse")]
        public async Task<IActionResult> GetRandomExamSetByCourse([FromQuery] int courseId)
        {
            var examSets = await db.ExamSets
                .Include(es => es.Course)
                .Where(es => es.CourseId == courseId)
                .Select(es => new ExamSetDTO
                {
                    ExamSetId = es.ExamSetId,
                    CourseId = es.CourseId,
                    CourseName = es.Course.CourseName,
                    Name = es.Name,
                    Description = es.Description,
                    PassingScore = es.PassingScore,
                    CreatedDate = es.CreatedDate,
                    TimeLimitSec = es.TimeLimitSec
                })
                .ToListAsync();

            if (!examSets.Any())
                return NotFound("Không có bộ đề nào cho khóa học này.");

            // Random nếu có nhiều bộ đề
            var random = new Random();
            var randomExamSet = examSets[random.Next(examSets.Count)];

            return Ok(randomExamSet);
        }




        // Lấy danh sách bộ đề theo khóa học
        [HttpGet("GetExamSetsByCourse")]
        public async Task<IActionResult> GetExamSetsByCourse([FromQuery] int courseId)
        {
            var examSets = await db.ExamSets
                .Include(es => es.Course)
                .Where(es => es.CourseId == courseId)
                .Select(es => new ExamSetDTO
                {
                    ExamSetId = es.ExamSetId,
                    CourseId = es.CourseId,
                    CourseName = es.Course.CourseName, 
                    Name = es.Name,
                    Description = es.Description,
                    PassingScore = es.PassingScore,
                    CreatedDate = es.CreatedDate,
                    TimeLimitSec = es.TimeLimitSec
                })
                .ToListAsync();

            if (!examSets.Any())
                return NotFound("Không có bộ đề nào cho khóa học này.");

            return Ok(examSets);
        }

        // Lấy chi tiết bộ đề
        [HttpGet("GetExamSetById/{id}")]
        public async Task<IActionResult> GetExamSetById(int id)
        {
            var examSet = await db.ExamSets
                .Include(es => es.ExamSetQuestions)
                .Include(es=>es.Course)
                .FirstOrDefaultAsync(es => es.ExamSetId == id);

            if (examSet == null)
                return NotFound("Không tìm thấy bộ đề.");

            var examSetDTO = new ExamSetDTO
            {
                ExamSetId = examSet.ExamSetId,
                CourseId = examSet.CourseId,
                CourseName = examSet.Course.CourseName,
                Name = examSet.Name,
                Description = examSet.Description,
                PassingScore = examSet.PassingScore,
                CreatedDate = examSet.CreatedDate,
                TimeLimitSec = examSet.TimeLimitSec
            };

            return Ok(examSetDTO);
        }

        // Lấy danh sách câu hỏi của bộ đề
        [HttpGet("GetQuestionsByExamSet/{examSetId}")]
        public async Task<IActionResult> GetQuestionsByExamSet(int examSetId)
        {
            var questions = await db.ExamSetQuestions
                .Include(q => q.Question)
                .Where(q => q.ExamSetId == examSetId)
                .OrderBy(q => q.QuestionOrder)
                .Select(q => new
                {
                    q.QuestionId,
                    q.Question.ContentType,
                    q.Question.QuestionText,
                    q.Question.QuestionTypeId,
                    q.Question.AnswerOptions,
                    q.Question.CorrectAnswer,
                    q.Question.AudioUrl,
                    q.Question.ImageUrl,
                    q.Question.Explanation,
                    q.Question.QuestionLevelId,
                    q.Question.QuestionDescription,

                    q.QuestionScore,
                   
                    q.QuestionOrder
                })
                .ToListAsync();

            if (!questions.Any())
                return NotFound("Bộ đề không có câu hỏi.");

            return Ok(questions);
        }

        // Kiểm tra điều kiện làm bộ đề (đã hoàn thành khóa học)
        [Authorize(Roles = "User,Admin")]
        [HttpGet("CanTakeExamSet")]
        public async Task<IActionResult> CanTakeExamSet([FromQuery] int userId, [FromQuery] int examSetId)
        {
            var examSet = await db.ExamSets.Include(es => es.Course).FirstOrDefaultAsync(es => es.ExamSetId == examSetId);
            if (examSet == null)
                return NotFound("Không tìm thấy bộ đề.");

            var courseId = examSet.CourseId;

            // Lấy danh sách bài học trong khóa học
            var lessonIds = await db.CourseLessons
                .Where(cl => cl.CourseId == courseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            // Lấy danh sách bài học đã hoàn thành
            var completedLessons = await db.AcademicResults
                .Where(ar => ar.UserId == userId && ar.CourseId == courseId && ar.Status == "Completed")
                .Select(ar => ar.LessonId)
                .ToListAsync();

            bool allCompleted = lessonIds.All(lid => completedLessons.Contains(lid));

            if (!allCompleted)
            {
                return Ok(new
                {
                    CanTakeExam = false,
                    Message = "Bạn cần hoàn thành tất cả bài học trong khóa học trước khi làm bộ đề."
                });
            }

            return Ok(new
            {
                CanTakeExam = true,
                Message = "Bạn có thể làm bộ đề."
            });
        }

        // Bắt đầu làm bài thi cho user hiện tại (lấy userId từ token)
        [Authorize(Roles = "User")]
        [HttpPost("StartExamForUser")]
        public async Task<IActionResult> StartExamForUser([FromBody] UserExamHistoryDTO dto)
        {
            if (dto == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Lấy thông tin user từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Kiểm tra bộ đề tồn tại
            var examSet = await db.ExamSets.FindAsync(dto.ExamSetId);
            if (examSet == null)
                return NotFound("Không tìm thấy bộ đề.");

            // Kiểm tra user tồn tại
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Kiểm tra đã hoàn thành khóa học chưa
            var lessonIds = await db.CourseLessons
                .Where(cl => cl.CourseId == examSet.CourseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            var completedLessons = await db.AcademicResults
                .Where(ar => ar.UserId == userId && ar.CourseId == examSet.CourseId && ar.Status == "Completed")
                .Select(ar => ar.LessonId)
                .ToListAsync();

            bool allCompleted = lessonIds.All(lid => completedLessons.Contains(lid));
            if (!allCompleted)
                return BadRequest("Bạn chưa hoàn thành tất cả bài học trong khóa học.");

            // Tạo lịch sử làm bài
            var history = new UserExamHistory
            {
                UserId = userId,
                ExamSetId = dto.ExamSetId,
                TakenAt = DateTime.Now,
                TotalScore = null,
                IsPassed = null,
                DurationSec = null
            };

            db.UserExamHistories.Add(history);
            await db.SaveChangesAsync();

            var responseDto = new UserExamHistoryDTO
            {
                HistoryId = history.HistoryId,
                UserId = history.UserId,
                ExamSetId = history.ExamSetId,
                TakenAt = history.TakenAt
            };

            return Ok(responseDto);
        }

        // Bắt đầu làm bài thi (ghi nhận lịch sử bắt đầu)
        [Authorize(Roles = "User,Admin")]
        [HttpPost("StartExam")]
        public async Task<IActionResult> StartExam([FromBody] UserExamHistoryDTO dto)
        {
            if (dto == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Kiểm tra bộ đề tồn tại
            var examSet = await db.ExamSets.FindAsync(dto.ExamSetId);
            if (examSet == null)
                return NotFound("Không tìm thấy bộ đề.");

            // Kiểm tra user tồn tại
            var user = await db.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Kiểm tra đã hoàn thành khóa học chưa
            var lessonIds = await db.CourseLessons
                .Where(cl => cl.CourseId == examSet.CourseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            var completedLessons = await db.AcademicResults
                .Where(ar => ar.UserId == dto.UserId && ar.CourseId == examSet.CourseId && ar.Status == "Completed")
                .Select(ar => ar.LessonId)
                .ToListAsync();

            bool allCompleted = lessonIds.All(lid => completedLessons.Contains(lid));
            if (!allCompleted)
                return BadRequest("Bạn chưa hoàn thành tất cả bài học trong khóa học.");

            // Tạo lịch sử làm bài
            var history = new UserExamHistory
            {
                UserId = dto.UserId,
                ExamSetId = dto.ExamSetId,
                TakenAt = DateTime.Now,
                TotalScore = null,
                IsPassed = null,
                DurationSec = null
            };

            db.UserExamHistories.Add(history);
            await db.SaveChangesAsync();

            dto.HistoryId = history.HistoryId;
            dto.TakenAt = history.TakenAt;

            return Ok(dto);
        }

        // Nộp bài thi (lưu kết quả)
        [Authorize(Roles = "User,Admin")]
        [HttpPut("SubmitExam/{historyId}")]
        public async Task<IActionResult> SubmitExam(int historyId, [FromBody] UserExamHistoryDTO dto)
        {
            var history = await db.UserExamHistories.FindAsync(historyId);
            if (history == null)
                return NotFound("Không tìm thấy lịch sử làm bài.");

            // Cập nhật kết quả
            history.TotalScore = dto.TotalScore;
            history.IsPassed = dto.IsPassed;
            history.DurationSec = dto.DurationSec;

            await db.SaveChangesAsync();
            return Ok("Nộp bài thành công.");
        }

        // Lấy lịch sử làm bài của user hiện tại theo courseId (lấy userId từ token)
        [Authorize(Roles = "User")]
        [HttpGet("GetExamHistoryForUser")]
        public async Task<IActionResult> GetExamHistoryForUser([FromQuery] int courseId)
        {
            // Lấy userId từ token
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var histories = await db.UserExamHistories
                .Include(h => h.User)
                .Include(h => h.ExamSet)
                .Where(h => h.UserId == userId && h.ExamSet.CourseId == courseId)
                .OrderByDescending(h => h.TakenAt)
                .Select(h => new UserExamHistoryDTO
                {
                    HistoryId = h.HistoryId,
                    UserId = h.UserId,
                    FullName = h.User.Fullname,
                    ExamSetId = h.ExamSetId,
                    TakenAt = h.TakenAt,
                    TotalScore = h.TotalScore,
                    IsPassed = h.IsPassed,
                    DurationSec = h.DurationSec
                })
                .ToListAsync();

            return Ok(histories);
        }

        // Lấy lịch sử làm bài của người dùng
        [Authorize(Roles = "User,Admin")]
        [HttpGet("GetUserExamHistory")]
        public async Task<IActionResult> GetUserExamHistory([FromQuery] int userId)
        {
            var histories = await db.UserExamHistories
                .Include(h => h.User)
                .Include(h => h.ExamSet)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.TakenAt)
                .Select(h => new UserExamHistoryDTO
                {
                    HistoryId = h.HistoryId,
                    UserId = h.UserId,
                    FullName = h.User.Fullname,

                    ExamSetId = h.ExamSetId,
                    TakenAt = h.TakenAt,
                    TotalScore = h.TotalScore,
                    IsPassed = h.IsPassed,
                    DurationSec = h.DurationSec
                })
                .ToListAsync();

            return Ok(histories);
        }
        // Tạo bộ đề mới (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateExamSet")]
        public async Task<IActionResult> CreateExamSet([FromBody] ExamSetDTO dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");
            var examSet = new ExamSet
            {
                CourseId = dto.CourseId,
                Name = dto.Name,
                Description = dto.Description,
                PassingScore = dto.PassingScore,
                CreatedDate = DateTime.Now,
                TimeLimitSec = dto.TimeLimitSec
            };
            db.ExamSets.Add(examSet);
            await db.SaveChangesAsync();
            dto.ExamSetId = examSet.ExamSetId;
            dto.CreatedDate = examSet.CreatedDate;
            return Ok(dto);
        }
        // Sửa bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateExamSet/{id}")]
        public async Task<IActionResult> UpdateExamSet(int id, [FromBody] ExamSetDTO dto)
        {
            var examSet = await db.ExamSets.FindAsync(id);
            if (examSet == null) return NotFound("Không tìm thấy bộ đề.");
            examSet.Name = dto.Name;
            examSet.Description = dto.Description;
            examSet.PassingScore = dto.PassingScore;
            examSet.TimeLimitSec = dto.TimeLimitSec;
            await db.SaveChangesAsync();
            return Ok("Cập nhật bộ đề thành công.");
        }
        // Xóa bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteExamSet/{id}")]
        public async Task<IActionResult> DeleteExamSet(int id)
        {
            var examSet = await db.ExamSets.FindAsync(id);
            if (examSet == null) return NotFound("Không tìm thấy bộ đề.");
            db.ExamSets.Remove(examSet);
            await db.SaveChangesAsync();
            return Ok("Xóa bộ đề thành công.");
        }
        // Thêm câu hỏi vào bộ đề (chỉ dành cho Admin)
        [HttpPost("AddQuestionToExamSet")]
        public async Task<IActionResult> AddQuestionToExamSet([FromBody] ExamSetQuestionCreateDTO dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");

            // Kiểm tra bộ đề và câu hỏi có tồn tại không
            var examSetExists = await db.ExamSets.AnyAsync(e => e.ExamSetId == dto.ExamSetId);
            var questionExists = await db.Questions.AnyAsync(q => q.QuestionId == dto.QuestionId);
            if (!examSetExists) return NotFound("Bộ đề không tồn tại.");
            if (!questionExists) return NotFound("Câu hỏi không tồn tại.");

            // Tìm bản ghi cũ (nếu có)
            var existing = await db.ExamSetQuestions
                .FirstOrDefaultAsync(x => x.ExamSetId == dto.ExamSetId && x.QuestionId == dto.QuestionId);

            // Lấy danh sách các câu hỏi hiện tại trong bộ đề, sắp xếp theo Order
            var existingQuestions = await db.ExamSetQuestions
                .Where(x => x.ExamSetId == dto.ExamSetId)
                .OrderBy(x => x.QuestionOrder)
                .ToListAsync();

            int newOrder = dto.QuestionOrder > 0 ? dto.QuestionOrder : existingQuestions.Count + (existing == null ? 1 : 0);

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                if (existing == null)
                {
                    // Thêm mới, dồn thứ tự nếu cần
                    foreach (var q in existingQuestions.Where(x => x.QuestionOrder >= newOrder))
                    {
                        q.QuestionOrder++;
                    }

                    var entity = new ExamSetQuestion
                    {
                        ExamSetId = dto.ExamSetId,
                        QuestionId = dto.QuestionId,
                        QuestionScore = dto.QuestionScore,
                        QuestionOrder = newOrder
                    };

                    db.ExamSetQuestions.Add(entity);
                    await db.SaveChangesAsync();
                    await tran.CommitAsync();
                    return Ok(new { message = $"Thêm câu hỏi vào bộ đề thành công tại vị trí {newOrder}." });
                }
                else
                {
                    // Đã có, kiểm tra điểm/thứ tự
                    bool changed = false;

                    if (existing.QuestionScore != dto.QuestionScore)
                    {
                        existing.QuestionScore = dto.QuestionScore;
                        changed = true;
                    }

                    if (existing.QuestionOrder != newOrder)
                    {
                        // Dồn lại thứ tự các câu hỏi khác
                        if (newOrder < existing.QuestionOrder)
                        {
                            // Dời lên: các câu hỏi từ newOrder đến existing.QuestionOrder-1 tăng 1
                            foreach (var q in existingQuestions.Where(x => x.QuestionOrder >= newOrder && x.QuestionOrder < existing.QuestionOrder))
                            {
                                q.QuestionOrder++;
                            }
                        }
                        else if (newOrder > existing.QuestionOrder)
                        {
                            // Dời xuống: các câu hỏi từ existing.QuestionOrder+1 đến newOrder giảm 1
                            foreach (var q in existingQuestions.Where(x => x.QuestionOrder > existing.QuestionOrder && x.QuestionOrder <= newOrder))
                            {
                                q.QuestionOrder--;
                            }
                        }
                        existing.QuestionOrder = newOrder;
                        changed = true;
                    }

                    if (changed)
                    {
                        await db.SaveChangesAsync();
                        await tran.CommitAsync();
                        return Ok(new { message = $"Cập nhật câu hỏi trong bộ đề thành công (điểm/thứ tự mới)." });
                    }
                    else
                    {
                        await tran.RollbackAsync();
                        return Ok(new { message = "Câu hỏi đã tồn tại với điểm số và thứ tự giống nhau, không có gì thay đổi." });
                    }
                }
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi thêm/cập nhật câu hỏi: {ex.Message}");
            }
        }

        // Xóa câu hỏi khỏi bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteQuestionFromExamSet")]
        public async Task<IActionResult> DeleteQuestionFromExamSet([FromQuery] int examSetId, [FromQuery] int questionId)
        {
            var eq = await db.ExamSetQuestions.FirstOrDefaultAsync(x => x.ExamSetId == examSetId && x.QuestionId == questionId);
            if (eq == null) return NotFound("Không tìm thấy câu hỏi trong bộ đề.");
            db.ExamSetQuestions.Remove(eq);
            await db.SaveChangesAsync();
            return Ok("Xóa câu hỏi khỏi bộ đề thành công.");
        }
        // Lấy tất cả bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllExamSets")]
        public async Task<IActionResult> GetAllExamSets()
        {
            var examSets = await db.ExamSets
                .Include(es=>es.Course)
                .Select(es => new ExamSetDTO
                {
                    ExamSetId = es.ExamSetId,
                    CourseId = es.CourseId,
                    CourseName = es.Course.CourseName, 
                    Name = es.Name,
                    Description = es.Description,
                    PassingScore = es.PassingScore,
                    CreatedDate = es.CreatedDate,
                    TimeLimitSec = es.TimeLimitSec
                })
                .ToListAsync();
            return Ok(examSets);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("SwapExamQuestionOrder")]
        public async Task<IActionResult> SwapExamQuestionOrder([FromBody] SwapExamQuestionOrderDTO dto)
        {
            // Kiểm tra đầu vào
            if (dto == null)
                return BadRequest("Dữ liệu không hợp lệ.");
            if (dto.ExamSetId <= 0)
                return BadRequest("ExamSetId không hợp lệ.");
            if (dto.SourceOrderNo <= 0 || dto.TargetOrderNo <= 0)
                return BadRequest("SourceOrderNo hoặc TargetOrderNo không hợp lệ.");
            if (dto.SourceOrderNo == dto.TargetOrderNo)
                return BadRequest("SourceOrderNo và TargetOrderNo không được trùng nhau.");

            // Lấy hai bản ghi cần hoán đổi
            var questions = await db.ExamSetQuestions
                .Where(q => q.ExamSetId == dto.ExamSetId &&
                            (q.QuestionOrder == dto.SourceOrderNo || q.QuestionOrder == dto.TargetOrderNo))
                .ToListAsync();

            if (questions.Count != 2)
                return NotFound("Không tìm thấy đủ hai câu hỏi để hoán đổi.");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                var source = questions.First(q => q.QuestionOrder == dto.SourceOrderNo);
                var target = questions.First(q => q.QuestionOrder == dto.TargetOrderNo);

                // Đặt tạm để tránh trùng
                source.QuestionOrder = -1;
                await db.SaveChangesAsync();

                target.QuestionOrder = dto.SourceOrderNo;
                await db.SaveChangesAsync();

                source.QuestionOrder = dto.TargetOrderNo;
                await db.SaveChangesAsync();

                await tran.CommitAsync();
                return Ok($"Đã hoán đổi vị trí {dto.SourceOrderNo} ↔ {dto.TargetOrderNo} cho ExamSet {dto.ExamSetId}.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi hoán đổi: {ex.Message}");
            }
        }
        // nhằn mục đích lấy danh sách câu hỏi
        [Authorize(Roles = "Admin")]
        [HttpGet("GetQuestionsNotInExamSet")]
        public async Task<IActionResult> GetQuestionsNotInExamSet([FromQuery] int examSetId)
        {
            var questionIdsInSet = await db.ExamSetQuestions
                .Where(q => q.ExamSetId == examSetId)
                .Select(q => q.QuestionId)
                .ToListAsync();
            var questions = await db.Questions
                .Where(q => !questionIdsInSet.Contains(q.QuestionId))
                .ToListAsync();
            return Ok(questions);
        }
        // Cập nhật thứ tự câu hỏi trong bộ đề (chỉ dành cho Admin) Move up move down
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateExamQuestionOrder")]
        public async Task<IActionResult> UpdateExamQuestionOrder([FromBody] ExamSetQuestionDTO dto)
        {
            if (dto == null || dto.QuestionOrder < 1)
                return BadRequest("Dữ liệu không hợp lệ.");

            var eq = await db.ExamSetQuestions
                .FirstOrDefaultAsync(x => x.ExamSetId == dto.ExamSetId && x.QuestionId == dto.QuestionId);
            if (eq == null)
                return NotFound("Không tìm thấy câu hỏi trong bộ đề.");

            var maxOrder = await db.ExamSetQuestions
                .Where(x => x.ExamSetId == dto.ExamSetId)
                .MaxAsync(x => (int?)x.QuestionOrder) ?? 0;

            if (dto.QuestionOrder > maxOrder)
                return BadRequest($"Thứ tự phải từ 1 đến {maxOrder}.");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                int oldOrder = eq.QuestionOrder ?? 0;
                if (oldOrder == dto.QuestionOrder)
                    return Ok("Không cần cập nhật vì thứ tự không thay đổi.");

                eq.QuestionOrder = -1;
                await db.SaveChangesAsync();

                var questionsToShift = await db.ExamSetQuestions
                    .Where(x => x.ExamSetId == dto.ExamSetId && x.QuestionId != dto.QuestionId)
                    .OrderBy(x => x.QuestionOrder)
                    .ToListAsync();

                if (oldOrder < dto.QuestionOrder)
                {
                    foreach (var q in questionsToShift.Where(x => x.QuestionOrder > oldOrder && x.QuestionOrder <= dto.QuestionOrder))
                        q.QuestionOrder--;
                }
                else
                {
                    foreach (var q in questionsToShift.Where(x => x.QuestionOrder >= dto.QuestionOrder && x.QuestionOrder < oldOrder))
                        q.QuestionOrder++;
                }

                eq.QuestionOrder = dto.QuestionOrder;
                await db.SaveChangesAsync();
                await tran.CommitAsync();
                return Ok("Cập nhật thứ tự câu hỏi thành công.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi cập nhật: {ex.Message}");
            }
        }
        // Thêm nhiều câu hỏi vào bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("AddMultipleQuestionsToExamSet")]
        public async Task<IActionResult> AddMultipleQuestionsToExamSet([FromBody] List<ExamSetQuestionDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("Danh sách không hợp lệ.");

            foreach (var dto in dtos)
            {
                if (!await db.Questions.AnyAsync(q => q.QuestionId == dto.QuestionId))
                    return BadRequest($"Câu hỏi {dto.QuestionId} không tồn tại.");
                if (await db.ExamSetQuestions.AnyAsync(x => x.ExamSetId == dto.ExamSetId && x.QuestionId == dto.QuestionId))
                    continue; // Bỏ qua nếu đã có
                db.ExamSetQuestions.Add(new ExamSetQuestion
                {
                    ExamSetId = dto.ExamSetId,
                    QuestionId = dto.QuestionId,
                    QuestionOrder = dto.QuestionOrder
                });
            }
            await db.SaveChangesAsync();
            return Ok("Thêm nhiều câu hỏi thành công.");
        }
        // Xóa nhiều câu hỏi vào bộ đề (chỉ dành cho Admin)
        // Xóa nhiều câu hỏi khỏi bộ đề (chỉ dành cho Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteMultipleQuestionsFromExamSet")]
        public async Task<IActionResult> DeleteMultipleQuestionsFromExamSet([FromBody] List<ExamSetQuestionDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("Danh sách không hợp lệ.");

            var examSetId = dtos.First().ExamSetId;
            var questionIds = dtos.Select(x => x.QuestionId).ToList();

            var questionsToRemove = await db.ExamSetQuestions
                .Where(x => x.ExamSetId == examSetId && questionIds.Contains(x.QuestionId))
                .ToListAsync();

            if (!questionsToRemove.Any())
                return NotFound("Không tìm thấy câu hỏi nào để xóa.");

            db.ExamSetQuestions.RemoveRange(questionsToRemove);
            await db.SaveChangesAsync();

            // Sau khi xóa, cập nhật lại thứ tự các câu hỏi còn lại cho liên tục
            var remainingQuestions = await db.ExamSetQuestions
                .Where(x => x.ExamSetId == examSetId)
                .OrderBy(x => x.QuestionOrder)
                .ToListAsync();

            for (int i = 0; i < remainingQuestions.Count; i++)
            {
                remainingQuestions[i].QuestionOrder = i + 1;
            }
            await db.SaveChangesAsync();

            return Ok("Đã xóa nhiều câu hỏi khỏi bộ đề và cập nhật lại thứ tự.");
        }

        // Update điểm số câu hỏi trong bộ đề (chỉ dành cho Admin)

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateExamQuestionScore")]
        public async Task<IActionResult> UpdateExamQuestionScore([FromBody] ExamSetQuestionDTO dto)
        {
            var eq = await db.ExamSetQuestions
                .FirstOrDefaultAsync(x => x.ExamSetId == dto.ExamSetId && x.QuestionId == dto.QuestionId);
            if (eq == null)
                return NotFound("Không tìm thấy câu hỏi trong bộ đề.");
            eq.QuestionScore = dto.QuestionScore;
            await db.SaveChangesAsync();
            return Ok("Cập nhật điểm số câu hỏi thành công.");
        }



    }
}
