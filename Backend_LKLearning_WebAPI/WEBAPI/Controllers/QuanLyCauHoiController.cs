using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WEBAPI.DTOS;
using WEBAPI.Models;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyCauHoiController : ControllerBase
    {


        private readonly LuanvantienganhContext db;


        public QuanLyCauHoiController(LuanvantienganhContext context)
        {
            db = context;
        }

        //==Quản Lý Loại QuestionType trước===
        //lấy danh sách loại câu hỏi
        // GET: api/QuanLyCauHoi/GetListQuestionType
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetListQuestionType")]
        public async Task<IActionResult> GetListQuestionType()
        {
            var questiontype = await db.QuestionTypes.ToListAsync();

            // Map từ entity sang DTO 
            var questiontypeDTOs = questiontype.Select(c => new QuestionTypeDTO
            {
                QuestionTypeId = c.QuestionTypeId,
                TypeName = c.TypeName,
                TypeDescription = c.TypeDescription
            }).ToList();

            return Ok(questiontypeDTOs);
        }

        //tìm bằng id 
        // GET: api/QuanLyCauHoi/GetQuestionTypeById/{id}
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetQuestionTypeById/{id}")]
        public async Task<IActionResult> GetQuestionTypeById(int id)
        {
            var questionType = await db.QuestionTypes.FindAsync(id);
            if (questionType == null)
            {
                return NotFound("Không tìm thấy QuestionType");
            }

            var questionTypeDTO = new QuestionTypeDTO
            {
                QuestionTypeId = questionType.QuestionTypeId,
                TypeName = questionType.TypeName,
                TypeDescription = questionType.TypeDescription
            };

            return Ok(questionTypeDTO);
        }

        //thêm loại 
        // POST: api/QuanLyCauHoi/InsertQuestionType
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertQuestionType")]
        public async Task<IActionResult> InsertQuestionType([FromBody] QuestionTypeDTO questionTypeDTO)
        {
            if (questionTypeDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var questionType = new QuestionType
            {
                TypeName = questionTypeDTO.TypeName,
                TypeDescription = questionTypeDTO.TypeDescription
            };

            try
            {
                db.QuestionTypes.Add(questionType);
                await db.SaveChangesAsync();
                return Ok("Thêm QuestionType thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm QuestionType không thành công");
            }
        }


        //sửa question
        // PUT: api/QuanLyCauHoi/UpdateQuestionType/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateQuestionType/{id}")]
        public async Task<IActionResult> UpdateQuestionType(int id, [FromBody] QuestionTypeDTO questionTypeDTO)
        {
            if (questionTypeDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var existingQuestionType = await db.QuestionTypes.FindAsync(id);
            if (existingQuestionType == null)
            {
                return NotFound("Không tìm thấy QuestionType");
            }

            // Cập nhật các trường
            existingQuestionType.TypeName = questionTypeDTO.TypeName;
            existingQuestionType.TypeDescription = questionTypeDTO.TypeDescription;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa QuestionType thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa QuestionType không thành công");
            }
        }


        // DELETE: api/QuanLyCauHoi/DeleteQuestionType/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteQuestionType/{id}")]
        public async Task<IActionResult> DeleteQuestionType(int id)
        {
            var questionType = await db.QuestionTypes.FindAsync(id);
            if (questionType == null)
            {
                return NotFound("Không tìm thấy QuestionType");
            }

            // Kiểm tra xem có question nào đang sử dụng loại câu hỏi này hay không
            bool hasRelatedQuestions = await db.Questions.AnyAsync(q => q.QuestionTypeId == id);
            if (hasRelatedQuestions)
            {
                return BadRequest("Không thể xóa QuestionType vì có câu hỏi đăng ký sử dụng loại này");
            }

            try
            {
                db.QuestionTypes.Remove(questionType);
                await db.SaveChangesAsync();
                return Ok("Xóa QuestionType thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa QuestionType không thành công");
            }
        }


        //===================== Quản lý nội dung câu hỏi=================
        [HttpGet("GetListQuestionContent")]
        public async Task<IActionResult> GetListQuestionContent()
        {
            var contents = await db.ContentTypes.ToListAsync();
            var contentDTOs = contents.Select(c => new ContentTypeDTO
            {
                ContentTypeId = c.ContentTypeId,
                TypeName = c.TypeName,
                TypeDescription = c.TypeDescription
            }).ToList();
            return Ok(contentDTOs);
        }
        // Lấy nội dung câu hỏi theo ID
        [HttpGet("GetQuestionContentById/{id}")]
        public async Task<IActionResult> GetQuestionContentById(int id)
        {
            var content = await db.ContentTypes.FindAsync(id);
            if (content == null)
            {
                return NotFound("Không tìm thấy nội dung câu hỏi");
            }
            var contentDTO = new ContentTypeDTO
            {
                ContentTypeId = content.ContentTypeId,
                TypeName = content.TypeName,
                TypeDescription = content.TypeDescription
            };
            return Ok(contentDTO);
        }
        // Thêm nội dung câu hỏi mới
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertQuestionContent")]
        public async Task<IActionResult> InsertQuestionContent([FromBody] ContentTypeDTO contentDTO)
        {
            if (contentDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var content = new ContentType
            {
                TypeName = contentDTO.TypeName,
                TypeDescription = contentDTO.TypeDescription
            };
            try
            {
                db.ContentTypes.Add(content);
                await db.SaveChangesAsync();
                return Ok("Thêm nội dung câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm nội dung câu hỏi không thành công");
            }
        }
        // Xóa nội dung câu hỏi
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteQuestionContent/{id}")]
        public async Task<IActionResult> DeleteQuestionContent(int id)
        {
            var content = await db.ContentTypes.FindAsync(id);
            if (content == null)
            {
                return NotFound("Không tìm thấy nội dung câu hỏi");
            }
            try
            {
                db.ContentTypes.Remove(content);
                await db.SaveChangesAsync();
                return Ok("Xóa nội dung câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa nội dung câu hỏi không thành công");
            }
        }
        // Update nội dung câu hỏi
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateQuestionContent/{id}")]
        public async Task<IActionResult> UpdateQuestionContent(int id, [FromBody] ContentTypeDTO contentDTO)
        {
            if (contentDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var existingContent = await db.ContentTypes.FindAsync(id);
            if (existingContent == null)
            {
                return NotFound("Không tìm thấy nội dung câu hỏi");
            }
            // Cập nhật các trường
            existingContent.TypeName = contentDTO.TypeName;
            existingContent.TypeDescription = contentDTO.TypeDescription;
            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật nội dung câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cập nhật nội dung câu hỏi không thành công");
            }
        }



        //=============quản lý câu hỏi ========


        // GET: api/QuanLyCauHoi/GetListQuestion
        //[Authorize(Roles = "User,Admin")]
        [HttpGet("GetListQuestion")]
        public async Task<IActionResult> GetListQuestion()
        {
            var questions = await db.Questions.ToListAsync();

            // Map từ entity sang DTO
            var questionDTOs = questions.Select(q => new QuestionDTO
            {
                QuestionId = q.QuestionId,
                ContentTypeId = q.ContentTypeId,
                QuestionText = q.QuestionText,
                QuestionTypeId = q.QuestionTypeId,
                QuestionLevelId=q.QuestionLevelId,
                AnswerOptions = q.AnswerOptions,
                CorrectAnswer = q.CorrectAnswer,
                ImageUrl = q.ImageUrl,
                AudioUrl = q.AudioUrl,
                Explanation = q.Explanation,
                QuestionDescription=q.QuestionDescription
            }).ToList();
            return Ok(questionDTOs);
        }

        // GET: api/QuanLyCauHoi/GetQuestionById/{id}

        [HttpGet("GetQuestionById/{id}")]
        public async Task<IActionResult> GetQuestionById(int id)
        {
            var question = await db.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound("Không tìm thấy câu hỏi");
            }

            var questionDTO = new QuestionDTO
            {
                QuestionId = question.QuestionId,
                ContentTypeId = question.ContentTypeId,
                QuestionText = question.QuestionText,
                QuestionTypeId = question.QuestionTypeId,
                QuestionLevelId=question.QuestionLevelId,
                AnswerOptions = question.AnswerOptions,
                CorrectAnswer = question.CorrectAnswer,
                ImageUrl = question.ImageUrl,
                AudioUrl = question.AudioUrl,
                Explanation = question.Explanation,
                QuestionDescription = question.QuestionDescription
            };

            return Ok(questionDTO);
        }

        // POST: api/QuanLyCauHoi/InsertQuestion
        [Authorize(Roles = "Admin")]
        [HttpPost("InsertQuestion")]
        public async Task<IActionResult> InsertQuestion([FromBody] QuestionDTO questionDTO)
        {
            if (questionDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }



            // Kiểm tra xem QuestionType có tồn tại không nếu có giá trị
            if (questionDTO.QuestionTypeId != null)
            {
                var questionType = await db.QuestionTypes.FindAsync(questionDTO.QuestionTypeId);
                if (questionType == null)
                {
                    return BadRequest("Mã QuestionType không tồn tại trong hệ thống");
                }
            }

            // Ánh xạ từ DTO sang entity
            var question = new Question
            {
                QuestionText = questionDTO.QuestionText,
                ContentTypeId = questionDTO.ContentTypeId,
                QuestionTypeId = questionDTO.QuestionTypeId,
                QuestionLevelId = questionDTO.QuestionLevelId,
                AnswerOptions = questionDTO.AnswerOptions,
                CorrectAnswer = questionDTO.CorrectAnswer,
                ImageUrl = questionDTO.ImageUrl,
                AudioUrl = questionDTO.AudioUrl,
                Explanation = questionDTO.Explanation,
                QuestionDescription = questionDTO.QuestionDescription
            };

            try
            {
                db.Questions.Add(question);
                await db.SaveChangesAsync();
                return Ok("Thêm câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm câu hỏi không thành công");
            }
        }




        // PUT: api/QuanLyCauHoi/UpdateQuestion/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateQuestion/{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionDTO questionDTO)
        {
            if (questionDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var existingQuestion = await db.Questions.FindAsync(id);
            if (existingQuestion == null)
            {
                return NotFound("Không tìm thấy câu hỏi");
            }



            // Kiểm tra xem QuestionType có tồn tại không nếu có giá trị
            if (questionDTO.QuestionTypeId != null)
            {
                var questionType = await db.QuestionTypes.FindAsync(questionDTO.QuestionTypeId);
                if (questionType == null)
                {
                    return BadRequest("Mã QuestionType không tồn tại trong hệ thống");
                }
            }

            // Cập nhật các trường của câu hỏi

            existingQuestion.QuestionText = questionDTO.QuestionText;
            existingQuestion.ContentTypeId = questionDTO.ContentTypeId;
            existingQuestion.QuestionTypeId = questionDTO.QuestionTypeId;
            existingQuestion.QuestionLevelId = questionDTO.QuestionLevelId;
            existingQuestion.AnswerOptions = questionDTO.AnswerOptions;
            existingQuestion.CorrectAnswer = questionDTO.CorrectAnswer;
            existingQuestion.ImageUrl = questionDTO.ImageUrl;
            existingQuestion.AudioUrl = questionDTO.AudioUrl;
            existingQuestion.Explanation = questionDTO.Explanation;
            existingQuestion.QuestionDescription = questionDTO.QuestionDescription;

            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cập nhật câu hỏi không thành công");
            }
        }



        // DELETE: api/QuanLyCauHoi/DeleteQuestion/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteQuestion/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await db.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound("Không tìm thấy câu hỏi");
            }

            try
            {
                db.Questions.Remove(question);
                await db.SaveChangesAsync();
                return Ok("Xóa câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa câu hỏi không thành công");
            }
        }
        //===================LessonQuestion================================
        [HttpGet("GetLessonQuestionByID/{id}")]

        public async Task<IActionResult> GetLessonQuestionByID(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Bài học không hợp lệ.");
            }

            // Kiểm tra khóa học có tồn tại không
            bool lessonExists = await db.Lessons.AnyAsync(c => c.LessonId == id);
            if (!lessonExists)
            {
                return NotFound("Bài học không tồn tại.");
            }

            // Lấy danh sách CourseLesson của khóa với thứ tự tăng dần của OrderNo
            var lessonQuestions = await db.LessonQuestions
                .Where(cl => cl.LessonId == id)
                .OrderBy(cl => cl.OrderNo)
                .Select(cl => new LessonQuestionDTO
                {
                    LessonId = cl.LessonId,
                    QuestionId = cl.QuestionId,

                    OrderNo = cl.OrderNo
                })
                .ToListAsync();

            return Ok(lessonQuestions);
        }
        [HttpGet("GetQuestionsByLessonId/{lessonId}")]
        public async Task<IActionResult> GetQuestionsByLessonId(int lessonId)
        {
            var courseLessons = await db.LessonQuestions
                .Where(cl => cl.LessonId == lessonId)
                .Include(cl => cl.Question)
                .OrderBy(cl => cl.OrderNo)
                .ToListAsync();

            if (!courseLessons.Any())
            {
                return NotFound("Không tìm thấy câu hỏi cho bài học này");
            }

            var lessonsDTO = courseLessons.Select(l => new QuestionDTO
            {
                QuestionId = l.QuestionId,
                ContentTypeId = l.Question.ContentTypeId,
                QuestionText = l.Question.QuestionText,
                QuestionTypeId = l.Question.QuestionTypeId,
                QuestionLevelId = l.Question.QuestionLevelId,
                AnswerOptions = l.Question.AnswerOptions,
                CorrectAnswer = l.Question.CorrectAnswer,
                ImageUrl = l.Question.ImageUrl,
                AudioUrl = l.Question.AudioUrl,
                Explanation = l.Question.Explanation,
                QuestionDescription = l.Question.QuestionDescription    



            }).ToList();

            return Ok(lessonsDTO);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddQuestionToLesson")]
        public async Task<IActionResult> AddQuestionToLesson([FromBody] LessonQuestionDTO dto)
        {
            if (dto is null || dto.LessonId <= 0 || dto.QuestionId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Kiểm tra khóa học và bài học có tồn tại không
            bool lessonOk = await db.Lessons.AnyAsync(c => c.LessonId == dto.LessonId);
            bool questionOk = await db.Questions.AnyAsync(l => l.QuestionId == dto.QuestionId);

            if (!lessonOk)
                return NotFound("Bài học không tìm thấy");
            if (!questionOk)
                return NotFound("Câu hỏi không tìm thấy");

            // Kiểm tra bài học đã có trong khóa học chưa
            bool dup = await db.LessonQuestions
                .AnyAsync(cl => cl.LessonId == dto.LessonId && cl.QuestionId == dto.QuestionId);
            if (dup)
                return Conflict("Câu hỏi đã có trong bài học");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                // Lấy tất cả các LessonQuestion hiện tại cho LessonId, sắp xếp theo OrderNo
                var existingLessons = await db.LessonQuestions
                    .Where(cl => cl.LessonId == dto.LessonId)
                    .OrderBy(cl => cl.OrderNo)
                    .ToListAsync();

                // Xác định OrderNo cho bản ghi mới
                int newOrderNo = dto.OrderNo.HasValue && dto.OrderNo.Value > 0
                    ? Math.Min(dto.OrderNo.Value, existingLessons.Count + 1)
                    : existingLessons.Count + 1;

                // Cập nhật OrderNo của các bản ghi hiện có trong database
                foreach (var lesson in existingLessons)
                {
                    if (lesson.OrderNo >= newOrderNo)
                    {
                        lesson.OrderNo++;
                    }
                }

                // Thêm bản ghi mới
                var newRow = new LessonQuestion
                {
                    LessonId = dto.LessonId,
                    QuestionId = dto.QuestionId,
                    OrderNo = newOrderNo
                };
                db.LessonQuestions.Add(newRow);

                // Lưu tất cả thay đổi
                await db.SaveChangesAsync();
                await tran.CommitAsync();
                return Ok($"Đã thêm Câu hỏi {dto.QuestionId} vào bài học {dto.LessonId} tại vị trí {newOrderNo}.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi thêm câu hỏi: {ex.Message}");
            }
        }


        // DELETE: api/QuanLyBaiHoc/RemoveQuestionFromLesson
        [Authorize(Roles = "Admin")]
        [HttpDelete("RemoveQuestionFromLesson")]
        public async Task<IActionResult> RemoveQuestionFromLesson(int lessonId, int questionId)
        {
            if (lessonId <= 0 || questionId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // Tìm bản ghi CourseLesson cần xóa
            var lessonquestion = await db.LessonQuestions
                .FirstOrDefaultAsync(cl => cl.LessonId == lessonId && cl.QuestionId == questionId);

            if (lessonquestion is null)
                return NotFound("Câu hỏi  không tồn tại trong bài  học.");

            using var tran = await db.Database.BeginTransactionAsync();
            try
            {
                // Xóa bản ghi
                db.LessonQuestions.Remove(lessonquestion);

                // Lấy tất cả các bài học còn lại trong khóa học, sắp xếp theo OrderNo
                var remainingLessons = await db.LessonQuestions
                    .Where(cl => cl.LessonId == lessonId)
                    .OrderBy(cl => cl.OrderNo)
                    .ToListAsync();

                // Sắp xếp lại OrderNo bắt đầu từ 1
                for (int i = 0; i < remainingLessons.Count; i++)
                {
                    remainingLessons[i].OrderNo = i + 1;
                }

                await db.SaveChangesAsync();
                await tran.CommitAsync();
                return Ok($"Đã xóa câu hỏi {questionId} khỏi bài học {lessonId} và sắp xếp lại thứ tự.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return StatusCode(500, $"Lỗi khi xóa câu hỏi: {ex.Message}");
            }
        }

        // PUT: api/QuanLyBaiHoc/SwapQuestionOrder
        [Authorize(Roles = "Admin")]
        [HttpPut("SwapQuestionOrder")]
        public async Task<IActionResult> SwapQuestionOrder([FromBody] SwapQuestionOrderDTO dto)
        {
            Console.WriteLine($"Received SwapQuestionOrder: {JsonConvert.SerializeObject(dto)}");
            // Kiểm tra đầu vào
            if (dto == null)
                return BadRequest("Dữ liệu DTO không được cung cấp.");
            if (dto.LessonId <= 0)
                return BadRequest("LessonId không hợp lệ.");
            if (dto.SourceOrderNo <= 0 || dto.TargetOrderNo <= 0)
                return BadRequest("SourceOrderNo hoặc TargetOrderNo không hợp lệ.");
            if (dto.SourceOrderNo == dto.TargetOrderNo)
                return BadRequest("SourceOrderNo và TargetOrderNo không được trùng nhau.");

            // Kiểm tra tồn tại hai bản ghi
            var lessons = await db.LessonQuestions
                .Where(cl => cl.LessonId == dto.LessonId &&
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
                    $"Đã hoán đổi vị trí {dto.SourceOrderNo} ↔ {dto.TargetOrderNo} cho Course {dto.LessonId}."
                );
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync(); // hủy khi lỗi
                return StatusCode(500, $"Lỗi khi hoán đổi: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateQuestionOrder")]
        public async Task<IActionResult> UpdateQuestionOrder([FromBody] LessonQuestionDTO dto)
        {
            if (dto == null || dto.OrderNo < 1)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var lessonQuestions = await db.LessonQuestions
                    .FirstOrDefaultAsync(cl => cl.LessonId == dto.LessonId && cl.QuestionId == dto.QuestionId);
                if (lessonQuestions == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy câu hỏi trong bài học." });
                }

                var maxOrderNo = await db.LessonQuestions
                    .Where(cl => cl.LessonId == dto.LessonId)
                    .MaxAsync(cl => (int?)cl.OrderNo) ?? 0;
                if (dto.OrderNo > maxOrderNo)
                {
                    return BadRequest(new { success = false, message = $"Thứ tự phải từ 1 đến {maxOrderNo}." });
                }

                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    int oldOrderNo = lessonQuestions?.OrderNo ?? 0;

                    if (oldOrderNo == dto.OrderNo)
                    {
                        return Ok(new { success = true, message = "Không cần cập nhật vì thứ tự không thay đổi." });
                    }

                    // Step 1: Set the target lesson's OrderNo to a temporary value to avoid conflict
                    lessonQuestions.OrderNo = -1;
                    await db.SaveChangesAsync();

                    // Step 2: Shift other lessons to accommodate the new OrderNo
                    var questionsToShift = await db.LessonQuestions
                        .Where(cl => cl.LessonId == dto.LessonId && cl.QuestionId != dto.QuestionId)
                        .OrderBy(cl => cl.OrderNo)
                        .ToListAsync();

                    if (oldOrderNo < dto.OrderNo)
                    {
                        // Moving down (e.g., from 2 to 4): Shift lessons between oldOrderNo and newOrderNo up
                        foreach (var lesson in questionsToShift.Where(cl => cl.OrderNo > oldOrderNo && cl.OrderNo <= dto.OrderNo))
                        {
                            lesson.OrderNo--;
                        }
                    }
                    else
                    {
                        // Moving up (e.g., from 4 to 2): Shift lessons between newOrderNo and oldOrderNo down
                        foreach (var lesson in questionsToShift.Where(cl => cl.OrderNo >= dto.OrderNo && cl.OrderNo < oldOrderNo))
                        {
                            lesson.OrderNo++;
                        }
                    }

                    // Step 3: Set the target lesson's OrderNo to the new value
                    lessonQuestions.OrderNo = dto.OrderNo.Value;
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = $"Cập nhật thứ tự bài học {dto.QuestionId} thành {dto.OrderNo}." });
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

        [Authorize(Roles = "Admin")]
        [HttpPost("InsertMultipleQuestions")]
        public async Task<IActionResult> InsertMultipleQuestions([FromBody] List<QuestionDTO> questions)
        {
            if (questions == null || !questions.Any())
            {
                return BadRequest("Danh sách câu hỏi không hợp lệ!");
            }

            try
            {
                foreach (var questionDTO in questions)
                {
                    // Kiểm tra khóa ngoại
                    if (questionDTO.ContentTypeId != null)
                    {
                        var contentType = await db.ContentTypes.FindAsync(questionDTO.ContentTypeId);
                        if (contentType == null)
                        {
                            return BadRequest($"ContentTypeId {questionDTO.ContentTypeId} không tồn tại!");
                        }
                    }
                    if (questionDTO.QuestionTypeId != null)
                    {
                        var questionType = await db.QuestionTypes.FindAsync(questionDTO.QuestionTypeId);
                        if (questionType == null)
                        {
                            return BadRequest($"QuestionTypeId {questionDTO.QuestionTypeId} không tồn tại!");
                        }
                    }

                    var question = new Question
                    {
                        ContentTypeId = questionDTO.ContentTypeId,
                        QuestionText = questionDTO.QuestionText,
                        QuestionTypeId = questionDTO.QuestionTypeId,
                        QuestionLevelId = questionDTO.QuestionLevelId,
                        AnswerOptions = questionDTO.AnswerOptions,
                        CorrectAnswer = questionDTO.CorrectAnswer,
                        ImageUrl = questionDTO.ImageUrl,
                        AudioUrl = questionDTO.AudioUrl,
                        Explanation = questionDTO.Explanation,
                        QuestionDescription = questionDTO.QuestionDescription
                    };
                    db.Questions.Add(question);
                }

                await db.SaveChangesAsync();
                return Ok("Nhập nhiều câu hỏi thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }
        // // GET: api/QuanLyCauHoi/GetQuestionLevels
        [HttpGet("GetQuestionLevels")]
        public async Task<IActionResult> GetQuestionLevels()
        {
            var questionLevels = await db.QuestionLevels.ToListAsync();
            // Map từ entity sang DTO
            var questionLevelDTOs = questionLevels.Select(q => new QuestionLevelDTO
            {
                QuestionLevelId = q.QuestionLevelId,
                QuestionName = q.QuestionName

            }).ToList();
            return Ok(questionLevelDTOs);

        }
        // GET: api/QuanLyCauHoi/GetQuestionLevelById/{id}
        [HttpGet("GetQuestionLevelById/{id}")]
        public async Task<IActionResult> GetQuestionLevelById(int id)
        {
            var questionLevel = await db.QuestionLevels.FindAsync(id);
            if (questionLevel == null)
            {
                return NotFound("Không tìm thấy cấp độ câu hỏi");
            }
            var questionLevelDTO = new QuestionLevelDTO
            {
                QuestionLevelId = questionLevel.QuestionLevelId,
                QuestionName = questionLevel.QuestionName
            };
            return Ok(questionLevelDTO);
        }
        // POST: api/QuanLyCauHoi/InsertQuestionLevel
        [Authorize(Roles ="Admin")]
        [HttpPost("InsertQuestionLevel")] 
        public async Task<IActionResult> InsertQuestionLevel([FromBody] QuestionLevelDTO questionLevelDTO)
        {
            if (questionLevelDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var questionLevel = new QuestionLevel
            {
                QuestionName = questionLevelDTO.QuestionName
            };
            try
            {
                db.QuestionLevels.Add(questionLevel);
                await db.SaveChangesAsync();
                return Ok("Thêm cấp độ câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm cấp độ câu hỏi không thành công");
            }
        }
        [Authorize(Roles = "Admin")]
        // PUT: api/QuanLyCauHoi/UpdateQuestionLevel/{id}
        [HttpPut("UpdateQuestionLevel/{id}")]
        public async Task<IActionResult> UpdateQuestionLevel(int id, [FromBody] QuestionLevelDTO questionLevelDTO)
        {
            if (questionLevelDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var existingQuestionLevel = await db.QuestionLevels.FindAsync(id);
            if (existingQuestionLevel == null)
            {
                return NotFound("Không tìm thấy cấp độ câu hỏi");
            }
            // Cập nhật các trường
            existingQuestionLevel.QuestionName = questionLevelDTO.QuestionName;
            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật cấp độ câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cập nhật cấp độ câu hỏi không thành công");
            }
        }
        // DELETE: api/QuanLyCauHoi/DeleteQuestionLevel/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteQuestionLevel/{id}")]
        public async Task<IActionResult> DeleteQuestionLevel(int id)
        {
            var questionLevel = await db.QuestionLevels.FindAsync(id);
            if (questionLevel == null)
            {
                return NotFound("Không tìm thấy cấp độ câu hỏi");
            }
            // Kiểm tra xem có câu hỏi nào đang sử dụng cấp độ này hay không
            bool hasRelatedQuestions = await db.Questions.AnyAsync(q => q.QuestionLevelId == id);
            if (hasRelatedQuestions)
            {
                return BadRequest("Không thể xóa cấp độ câu hỏi vì có câu hỏi đăng ký sử dụng cấp độ này");
            }
            try
            {
                db.QuestionLevels.Remove(questionLevel);
                await db.SaveChangesAsync();
                return Ok("Xóa cấp độ câu hỏi thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Xóa cấp độ câu hỏi không thành công");
            }
        }
    }
}
