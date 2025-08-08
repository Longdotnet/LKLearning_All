using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LessonController : BaseController  
    {
        [Route("lesson")]
        public IActionResult Index(int page, int? courseId, DateOnly? dateFilter, bool? isActive)
        {
            string? token = HttpContext.Session.GetString("AuthToken");

            // Get all lessons
            var allLessons = CXuLy.getDSLesson().AsQueryable();

         

            // Get only dates that exist in lessons
            ViewBag.AvailableDates = allLessons.Select(l => l.Duration).Distinct().OrderBy(d => d).ToList();

      
         
            if (dateFilter != null)
            {
                allLessons = allLessons.Where(q => q.Duration == dateFilter.Value);
            }

            // Fixed IsActivate filter
            if (isActive.HasValue)
            {
                allLessons = allLessons.Where(q => q.IsActivate == isActive.Value);
            }

            // Store filter values for the view
            ViewBag.CurrentCourse = courseId;
            ViewBag.CurrentDate = dateFilter;
            ViewBag.CurrentActivate = isActive;
            // phân trang
            // B1: cài đặt thông số phân trang
            const int pageSize = 5;
            if (page < 1)
            {
                page = 1; // Đảm bảo trang bắt đầu từ 1
            }
            // B2: Tính toán dữ liệu
            int dem = allLessons.Count(); // Tổng số item có trong ds đó

            var paginatenew = new Paginate(dem, page, pageSize); //Khởi tạo đối tượng phân trang
            // B3: lấy dữ liệu trang hiện tại
            int recSkip = (page - 1) * pageSize; // Vĩ trí bắt đầu lấy dữ liệu

            var data = allLessons
                .Skip(recSkip) // Bỏ qua các item trước đó
                .Take(pageSize) // Lấy số lượng item của hiện tại
                .ToList();
            // B4 Truyền dữ liệu ra View
            ViewBag.Paginate = paginatenew; // Truyền thông tin phân trang

            return View(data);
        }

        [Route("lesson/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.xoaLesson(id.ToString(), token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"] = "Xóa thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Không có bài học nào được xóa.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [Route("lesson/viewquestion/deletemulti")]
        public IActionResult DeleteMultipleQuestion(List<int> selectedIds)
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
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Không có bài học nào được xóa.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult SearchCourse(string searchTerm)
        {
            List<CCourse> dsCourse = CXuLy.getDSCourse();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsCourse = dsCourse.Where(t => t.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = dsCourse.Select(c => new
            {
                id = c.CourseId,
                text = c.CourseName,
                description = c.Description,
                duration = c.DurationInMonths
            }).ToList();

            return Json(new
            {
                items = result
            });
        }
        [Route("lesson/select-course")]
        public IActionResult SelectCourseForLessons(string lessonIds)
        {
            // Lưu lessonIds vào ViewBag để sử dụng trong view
            ViewBag.LessonIds = lessonIds;
            return View();
        }

        [HttpPost]
        [Route("lesson/InsertMultipleToCourse")]
        public IActionResult InsertMultipleToCourse(int courseId, string lessonIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                var ids = lessonIds.Split(',').Select(int.Parse).ToList();
                int successCount = 0;

                foreach (var id in ids)
                {
                    if (CXuLy.themLessonVaoKhoaHoc(courseId, id, token))
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
        [Route("lesson/xoaQuestionRaKhoiBaiHoc/{lessonId}/{questionId}")]
        public IActionResult xoaQuestionRaKhoiBaiHoc(string lessonId, string questionId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaQuestionRaKhoiBaiHoc(lessonId, questionId, token))
                {
                    TempData["success"] = "Xóa thành công";
                    return RedirectToAction("viewQuestion", new { id = lessonId });
                }
                else
                {
                    TempData["error"] = "Xóa không thành công";
                    return RedirectToAction("viewQuestion", new { id = lessonId });
                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }
        }


        [Route("lesson/viewquestion/{id}")]
        public IActionResult viewQuestion(string id, int? questionTypeId, int page)
        {
            string token = HttpContext.Session.GetString("AuthToken");

            List<CQuestion> dsQuestion = CXuLy.GetQuestionsByLessonId(id);

            // Additional filter by question type if provided
            if (questionTypeId.HasValue)
            {
                dsQuestion = dsQuestion.Where(q => q.QuestionTypeId == questionTypeId.Value).ToList();
            }

            List<CQuestionType> dsQuestionType = CXuLy.getDSQuestionType();
            List<CLesson> dsLesson = CXuLy.getDSLesson();
            List<CLessonQuestion> lessonQuestions = CXuLy.GetLessonQuestionByID(id,token);
            // Get current lesson details
            CLesson currentLesson = CXuLy.getLessonById(id);
            ViewBag.LessonId = id;
            ViewBag.QuestionTypes = dsQuestionType;
            ViewBag.Lessons = dsLesson;
            ViewBag.CurrentQuestionTypeId = questionTypeId;
            ViewBag.CurrentLesson = currentLesson;
            ViewBag.LessonQuestions = lessonQuestions;
            // Pagination
            const int pageSize = 5;
            if (page < 1)
            {
                page = 1;
            }
            if(dsQuestion == null || dsQuestion.Count == 0)
            {
                ViewBag.Paginate = new Paginate(0, page, pageSize);
                return View(new List<CQuestion>());
            }

            int dem = dsQuestion.Count;
            var paginatenew = new Paginate(dem, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = dsQuestion
                .Skip(recSkip)
                .Take(pageSize)
                .ToList();
            ViewBag.Paginate = paginatenew;

            return View(data);
        }


        [HttpPut]
        [Route("lesson/SwapQuestionOrder")]
        public IActionResult SwapQuestionOrder([FromBody] CSwapQuestionOrder dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            try
            {
                bool result = CXuLy.SwapQuestionOrder(dto, token);
                if (result)
                {
                    return Ok(new { success = true, message = $"Đã hoán đổi vị trí câu hỏi {dto.SourceOrderNo} ↔ {dto.TargetOrderNo} trong bài học {dto.LessonId}." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Không thể hoán đổi thứ tự câu hỏi." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [Route("lesson/xoaQuestionFromLesson/{lessonId}/{questionId}")]
        public IActionResult xoaQuestionFromLesson(string lessonId, string questionId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.xoaQuestionRaKhoiBaiHoc(lessonId, questionId, token))
            {
                TempData["success"] = "Xóa thành công câu hỏi khỏi bài học";
                return RedirectToAction("viewQuestion", new { id = lessonId });
            }
            else
            {
                TempData["error"] = "Xóa không thành công";
                return RedirectToAction("viewQuestion", new { id = lessonId });
            }
        }

        [HttpPost]
        [Route("lesson/InsertMultipleQuestions")]
        public IActionResult InsertMultipleQuestions(int lessonId, string questionIds)
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
                    TempData["success"] = $"Đã thêm {successCount} câu hỏi vào bài học";
                }
                else
                {
                    TempData["error"] = "Không có câu hỏi nào được thêm";
                }

                return RedirectToAction("viewQuestion", new { id = lessonId });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("viewQuestion", new { id = lessonId });
            }
        }

        [Route("lesson/create")]
        public IActionResult formThemLesson(CLesson x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var courses = CXuLy.getDSCourse();
            // Lấy danh sách khóa học và chuyển đổi thành SelectListItem
            ViewBag.CourseList = courses.Select(c => new SelectListItem {
                Value = c.CourseId.ToString(),
                Text=c.CourseName
            }).ToList();
            return View();
        }
        [Route("lesson/edit")]
        public IActionResult formSuaLesson(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CLesson x = CXuLy.getLessonById(id);

            // Lấy danh sách khóa học và chuyển đổi thành SelectListItem
            var courses = CXuLy.getDSCourse();
            ViewBag.CourseList = courses.Select(c => new SelectListItem
            {
                Value = c.CourseId.ToString(), // Giả sử CCourse có property CourseId
                Text = c.CourseName // Giả sử CCourse có property CourseName
            }).ToList();

            return View(x);
        }
        [HttpPost]
        [Route("lesson/create")]
        public IActionResult ThemLesson(CLesson x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themLesson(x, token))
                {
                    TempData["success"] = "Thêm thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm không thành công";
                    return RedirectToAction("Index");
                }
                 
            }
            catch
            {
                return Content("Thêm không thành công");
            }
        }
        [HttpPost]
        [Route("lesson/edit/{id}")]
        public IActionResult SuaLesson(CLesson x, string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
              if(CXuLy.editLesson( id,x, token))
                {
                    TempData["success"] = "Sửa thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"]= "Sửa không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }

        [Route("lesson/xoalesson/{id}")]
        public IActionResult xoaLesson(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if(CXuLy.xoaLesson(id, token))
                {
                    TempData["success"] = "Xóa thành công";
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
      
        [HttpGet]
        public IActionResult searchDate(string searchTerm)
        {
            List<CLesson> dsLesson = CXuLy.getDSLesson();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                dsLesson = dsLesson
                    .Where(t => t.Duration.ToString("dd-MM-yyyy").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            var result = dsLesson
                .Select(l => new
                {
                    id = l.Duration.ToString("dd-MM-yyyy"),
                    text = l.Duration.ToString("dd-MM-yyyy")
                }).Distinct()
                .ToList();
            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
       
        [HttpPut]
        [Route("lesson/UpdateQuestionOrder")]
        public IActionResult UpdateQuestionOrder([FromBody] CLessonQuestion dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            try
            {
                bool result = CXuLy.UpdateQuestionOrder(dto, token);
                if (result)
                {
                    return Ok(new { success = true, message = $"Cập nhật thứ tự bài học {dto.LessonId} thành {dto.OrderNo}." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Không thể cập nhật thứ tự bài học." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}