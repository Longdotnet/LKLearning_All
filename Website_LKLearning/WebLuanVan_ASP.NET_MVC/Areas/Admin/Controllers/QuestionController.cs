using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;
using Newtonsoft.Json;
using OfficeOpenXml;
using ClosedXML.Excel;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _assemblyAIApiKey;

        public QuestionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _assemblyAIApiKey = configuration["AssemblyAIApiKey"];
        }
      
        [Route("question/ExportDataToExcel")]
        public IActionResult ExportDataToExcel()
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestion> dsQuestion = CXuLy.getDSQuestion();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Question");
                // header row
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "ID chủ đề";
                worksheet.Cell(1, 3).Value = "Tên câu hỏi";
                worksheet.Cell(1, 4).Value = "Thuộc loại";
                worksheet.Cell(1, 5).Value = "Lựa chọn đáp án";
                worksheet.Cell(1, 6).Value = "Đáp án đúng";
                worksheet.Cell(1, 7).Value = "Link hình ảnh";
                worksheet.Cell(1, 8).Value = "Link âm thanh";
                worksheet.Cell(1, 9).Value = "Giải thích";
                worksheet.Cell(1, 10).Value = "Mức độ câu hỏi";
                worksheet.Cell(1, 11).Value = "Mô tả câu hỏi";

                for (int i = 0; i < dsQuestion.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = dsQuestion[i].QuestionId;
                    worksheet.Cell(i + 2, 2).Value = dsQuestion[i].ContentTypeId;
                    worksheet.Cell(i + 2, 3).Value = dsQuestion[i].QuestionText;
                    worksheet.Cell(i + 2, 4).Value = dsQuestion[i].QuestionTypeId;
                    worksheet.Cell(i + 2, 5).Value = dsQuestion[i].AnswerOptions;
                    worksheet.Cell(i + 2, 6).Value = dsQuestion[i].CorrectAnswer;
                    worksheet.Cell(i + 2, 7).Value = dsQuestion[i].ImageUrl;
                    worksheet.Cell(i + 2, 8).Value = dsQuestion[i].AudioUrl;
                    worksheet.Cell(i + 2, 9).Value = dsQuestion[i].Explanation;
                    worksheet.Cell(i + 2, 10).Value = dsQuestion[i].QuestionLevelId;
                    worksheet.Cell(i + 2, 11).Value = dsQuestion[i].QuestionDescription;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "question.xlsx");
                }
            }
        }

        [Route("question")]
        public IActionResult Index(int page, int? contentTypeId, int? questionTypeId, int? questionLevelId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestion> dsQuestion = CXuLy.getDSQuestion();

            if (contentTypeId.HasValue)
            {
                dsQuestion = dsQuestion.Where(q => q.ContentTypeId == contentTypeId.Value).ToList();
            }

            if (questionTypeId.HasValue)
            {
                dsQuestion = dsQuestion.Where(q => q.QuestionTypeId == questionTypeId.Value).ToList();
            }
            if (questionLevelId.HasValue)
            {
                dsQuestion=dsQuestion.Where(q=>q.QuestionLevelId == questionLevelId.Value).ToList();
            }
            List<CQuestionType> questionTypes = CXuLy.getDSQuestionType();
            List<CContentType> contentTypes = CXuLy.getDSContentType();
            List<CQuestionLevel> questionLevels = CXuLy.getDSQuestionLevel();
            ViewBag.QuestionLevels = questionLevels;
            ViewBag.QuestionTypes = questionTypes;
            ViewBag.ContentTypes = contentTypes;
            ViewBag.CurrentQuestionLevelId = questionLevelId;
            ViewBag.CurrentQuestionTypeId = questionTypeId;
            ViewBag.CurrentContentTypeId = contentTypeId;

            const int pageSize = 5;
            if (page < 1)
            {
                page = 1;
            }

            int dem = dsQuestion.Count;
            var paginatenew = new Paginate(dem, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = dsQuestion.Skip(recSkip).Take(pageSize).ToList();
            ViewBag.Paginate = paginatenew;
            return View(data);
        }

        [Route("question/themquestion")]
        public IActionResult formThemQuestion(int? lessonId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var model = new CQuestion();
            List<CQuestionType> questionTypes = CXuLy.getDSQuestionType();
            List<CContentType> contentTypes = CXuLy.getDSContentType();
            List<CQuestionLevel> questionLevels = CXuLy.getDSQuestionLevel();
            ViewBag.QuestionLevels = questionLevels;
            ViewBag.QuestionTypes = questionTypes;
            ViewBag.ContentTypes = contentTypes;

            if (lessonId.HasValue)
            {
                var currentLesson = CXuLy.getDSLesson().FirstOrDefault(l => l.LessonId == lessonId.Value);
                ViewBag.Lesson = new List<CLesson> { currentLesson };
            }
            else
            {
                ViewBag.Lesson = CXuLy.getDSLesson();
            }

            return View(model);
        }

        [Route("question/select-lesson")]
        public IActionResult SelectLessonForQuestions(string QuestionIds)
        {
            ViewBag.QuestionIds = QuestionIds;
            return View();
        }

        [HttpPost]
        [Route("lesson/InsertMultipleToLesson")]
        public IActionResult InsertMultipleToLesson(int lessonId, string questionIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                var ids = questionIds.Split(',').Select(int.Parse).ToList();
                int successCount = 0;

                foreach (var id in ids)
                {
                    if (CXuLy.themQuestionVaoBaiHoc(lessonId, id, token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = $"Đã thêm {successCount} bài học vào khóa học";
                }
                else
                {
                    TempData["error"] = "Không có bài học nào được thêm";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("question/themquestion")]
        public IActionResult ThemQuestion(CQuestion x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (string.IsNullOrEmpty(x.CorrectAnswer) && x.QuestionTypeId != 6  && x.QuestionTypeId !=8) // 6 = Nghe  && 8= Video
                {
                    ModelState.AddModelError("CorrectAnswer", "Correct answer is required");
                    ViewBag.Lesson = CXuLy.getDSLesson();
                    ViewBag.QuestionType = CXuLy.getDSQuestionType();
                    return View("formThemQuestion", x);
                }
                // Xử lý câu hỏi Nghe
                if (x.QuestionTypeId == 6)
                {
                    if (string.IsNullOrEmpty(x.AudioUrl))
                    {
                        ModelState.AddModelError("AudioUrl", "URL Audio là bắt buộc cho câu hỏi Nghe");
                    }
                    if (string.IsNullOrEmpty(x.AnswerOptions))
                    {
                        ModelState.AddModelError("AnswerOptions", "Nội dung transcript là bắt buộc");
                    }
                }

                if (CXuLy.themQuestion(x, token))
                {
                    TempData["success"] = "Thêm câu hỏi thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm câu hỏi không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Thêm không thành công");
            }
        }

        [Route("question/xoaquestion/{id}")]
        public IActionResult xoaQuestion(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaQuestion(id, token))
                {
                    TempData["success"] = "Xóa câu hỏi thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Xóa không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }
        }

        [Route("question/suaquestion/{id}")]
        public IActionResult formSuaQuestion(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CQuestion x = CXuLy.getQuestionById(id);
            ViewBag.Lesson = CXuLy.getDSLesson();
            List<CQuestionType> questionTypes = CXuLy.getDSQuestionType();
            List<CContentType> contentTypes = CXuLy.getDSContentType();
            List<CQuestionLevel> questionLevels = CXuLy.getDSQuestionLevel();
            ViewBag.QuestionLevels = questionLevels;
            ViewBag.QuestionTypes = questionTypes;
            ViewBag.ContentTypes = contentTypes;
            return View(x);
        }

        [HttpPost]
        [Route("question/suaquestion/{id}")]
        public IActionResult SuaQuestion(string id, CQuestion x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.editQuestion(id, x, token))
                {
                    TempData["success"] = "Sửa thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Sửa không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }

        [HttpPost]
        [Route("question/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.xoaQuestion(id.ToString(), token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = $"Xóa thành công {successCount} câu hỏi";
                    return Json(new { success = true, message = $"Xóa thành công:{successCount}/{selectedIds} " });
                }
                else
                {
                    TempData["error"] = "Xóa không thành công";
                    return Json(new { success = false, message = "Xóa không thành công: " });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult SearchLesson(string searchTerm)
        {
            List<CLesson> dsLesson = CXuLy.getDSLesson();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsLesson = dsLesson.Where(t => t.LessonTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = dsLesson.Select(c => new
            {
                id = c.LessonId,
                text = c.LessonTitle,
                description = c.LessonDescription,
                duration = c.Duration
            }).ToList();

            return Json(new
            {
                items = result
            });
        }

     
        public IActionResult SearchQuestionType(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestionType> questiontypes = CXuLy.getDSQuestionType();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                questiontypes = questiontypes.Where(c => c.TypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = questiontypes.Select(c => new
            {
                id = c.QuestionTypeId,
                text = c.TypeName
            }).ToList();

            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
        public IActionResult SearchQuestionLevel(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestionLevel> questionLevels = CXuLy.getDSQuestionLevel();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                questionLevels = questionLevels.Where(c => c.QuestionName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = questionLevels.Select(c => new
            {
                id = c.QuestionLevelId,
                text = c.QuestionName
            }).ToList();

            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }

        public IActionResult SearchContentType(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CContentType> contentTypes = CXuLy.getDSContentType();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                contentTypes = contentTypes.Where(c => c.TypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = contentTypes.Select(c => new
            {
                id = c.ContentTypeId,
                text = c.TypeName
            }).ToList();

            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }

        [HttpGet]
        [Route("question/chitiet/{id}")]
        public IActionResult viewDetail(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CQuestion x = CXuLy.getQuestionById(id);
         
            List<CLesson> dsLesson = CXuLy.getDSLesson();
        
            ViewBag.dsLesson = dsLesson;

            List<CQuestionType> questionTypes = CXuLy.getDSQuestionType();
            List<CContentType> contentTypes = CXuLy.getDSContentType();
            List<CQuestionLevel> questionLevels = CXuLy.getDSQuestionLevel();
            ViewBag.QuestionLevels = questionLevels;
            ViewBag.QuestionTypes = questionTypes;
            ViewBag.ContentTypes = contentTypes;
            return View(x);
        }
        [Route("question/select-examset")]
        public IActionResult SelectExamSetForQuestions(string questionIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var examSets = CXuLy.GetListExamSet(token);
            var ids = questionIds.Split(',').Select(int.Parse).ToList();
            var questions = CXuLy.getDSQuestion().Where(q => ids.Contains(q.QuestionId)).ToList();
            ViewBag.ExamSets = examSets;
            ViewBag.Questions = questions;
            ViewBag.QuestionIds = questionIds;
            return View();
        }
        [HttpPost]
        [Route("question/InsertMultipleToExamSet")]
        public IActionResult InsertMultipleToExamSet(int examSetId, List<CExamSetQuestion> SelectedQuestions)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var dto in SelectedQuestions)
            {
                dto.ExamSetId = examSetId;
                // Gọi WebAPI thêm từng câu hỏi vào bộ đề
                if (CXuLy.AddQuestionToExamSet(dto, token))
                    successCount++;

            }
            if (successCount > 0)
                TempData["success"] = $"Đã thêm {successCount} câu hỏi vào bộ đề";
            else
                TempData["error"] = "Không có câu hỏi nào được thêm";
            return RedirectToAction("Index", "ExamSet", new { area = "Admin" });
        }


        [HttpPost]
        [Route("question/analyze-audio")]
        public async Task<IActionResult> AnalyzeAudio(string audioUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(audioUrl))
                {
                    return Json(new { success = false, message = "URL Audio là bắt buộc" });
                }

                // Gọi API phân tích audio (giả sử sử dụng Deepgram)
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Token 17bb8847e002407a13c105e6c2d79ed74a26022e");
                var requestBody = new { url = audioUrl, model = "nova-2", punctuate = true };
                var response = await httpClient.PostAsync(
                    "https://api.deepgram.com/v1/listen",
                    new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json")
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = $"Lỗi Deepgram: {response.StatusCode}" });
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string transcript = result?.results?.channels[0]?.alternatives[0]?.transcript?.ToString() ?? "";

                if (string.IsNullOrEmpty(transcript))
                {
                    return Json(new { success = false, message = "Không tìm thấy transcript" });
                }

                return Json(new { success = true, transcript });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        
        }
        [HttpPost]
        [Route("question/upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile excelFile)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file Excel!" });
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        var rowCount = worksheet.RowsUsed().Count();

                        var questions = new List<CQuestion>();
                        for (int row = 2; row <= rowCount; row++) // Bắt đầu từ row 2 (bỏ header)
                        {
                            // Kiểm tra giá trị trước khi ép kiểu
                            string contentTypeIdStr = worksheet.Cell(row, 2).Value.ToString().Trim(); // Cột 2: contentTypeId
                            string questionTypeIdStr = worksheet.Cell(row, 4).Value.ToString().Trim(); // Cột 4: questionTypeId
                            string questionLevelIdStr = worksheet.Cell(row, 10).Value.ToString().Trim(); // Cột 4: questionTypeId

                            // Kiểm tra nếu ô rỗng hoặc không phải số
                            if (!int.TryParse(contentTypeIdStr, out int contentTypeId))
                            {
                                return Json(new { success = false, message = $"Lỗi ở hàng {row}, cột contentTypeId: '{contentTypeIdStr}' không phải số." });
                            }
                            if (!int.TryParse(questionTypeIdStr, out int questionTypeId))
                            {
                                return Json(new { success = false, message = $"Lỗi ở hàng {row}, cột questionTypeId: '{questionTypeIdStr}' không phải số." });
                            }

                            if (!int.TryParse(questionLevelIdStr, out int questionLevelId))
                            {
                                return Json(new { success = false, message = $"Lỗi ở hàng {row}, cột questionLevelId: '{questionLevelIdStr}' không phải số." });
                            }
                            var question = new CQuestion
                            {
                                ContentTypeId = contentTypeId, // Cột 2
                                QuestionText = worksheet.Cell(row, 3).Value.ToString(), // Cột 3
                                QuestionTypeId = questionTypeId, // Cột 4
                                AnswerOptions = worksheet.Cell(row, 5).Value.ToString(), // Cột 5
                                CorrectAnswer = worksheet.Cell(row, 6).Value.ToString(), // Cột 6
                                ImageUrl = worksheet.Cell(row, 7).Value.ToString(), // Cột 7
                                AudioUrl = worksheet.Cell(row, 8).Value.ToString(), // Cột 8
                                Explanation = worksheet.Cell(row, 9).Value.ToString(), // Cột 9
                                QuestionLevelId=questionLevelId, // Cột 10
                                QuestionDescription =worksheet.Cell(row,11).Value.ToString() // cột 11
                            };
                            questions.Add(question);
                        }

                        var (success, message) = CXuLy.ImportExcel(excelFile, token);
                        return Json(new { success, message });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xử lý file: " + ex.Message });
            }
        }

    }
}