using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExamSetController : BaseController
    {
        // Danh sách bộ đề (có lọc theo khóa học)
        [Route("Admin/ExamSet")]
        public IActionResult Index(int page = 1, int? courseId = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CExamSet> dsExamSet = courseId.HasValue
                ? CXuLy.GetExamSetsByCourse(courseId.Value, token)
                : CXuLy.GetListExamSet(token);

            // Lấy danh sách khóa học cho bộ lọc
            List<CCourse> courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            ViewBag.Courses = courses;
            ViewBag.CurrentCourse = courseId;

            // Phân trang
            const int pageSize = 5;
            if (page < 1) page = 1;
            int total = dsExamSet.Count;
            var paginate = new Paginate(total, page, pageSize);
            int skip = (page - 1) * pageSize;
            var data = dsExamSet.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;
            return View(data);
        }

        // Hiển thị form thêm bộ đề
        [HttpGet]
        [Route("Admin/ExamSet/formThemExamSet")]
        public IActionResult formThemExamSet()
        {
            string token = HttpContext.Session.GetString("AuthToken");
            ViewBag.Courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            return View();
        }

        // Xử lý thêm bộ đề
        [HttpPost]
        [Route("Admin/ExamSet/formThemExamSet")]
        public IActionResult formThemExamSet(CExamSet model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.CreateExamSet(model, token))
            {
                TempData["success"] = "Thêm bộ đề thành công!";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Thêm bộ đề thất bại!";
            ViewBag.Courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            return View(model);
        }

        // Hiển thị form sửa bộ đề
        [HttpGet]
        [Route("Admin/ExamSet/formSuaExamSet/{id}")]
        public IActionResult formSuaExamSet(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var examSet = CXuLy.GetExamSetById(id, token);
            if (examSet == null)
            {
                TempData["error"] = "Không tìm thấy bộ đề!";
                return RedirectToAction("Index");
            }
            ViewBag.Courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            return View(examSet);
        }

        // Xử lý sửa bộ đề
        [HttpPost]
        [Route("Admin/ExamSet/formSuaExamSet/{id}")]
        public IActionResult formSuaExamSet(int id, CExamSet model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.UpdateExamSet(id, model, token))
            {
                TempData["success"] = "Cập nhật bộ đề thành công!";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Cập nhật bộ đề thất bại!";
            ViewBag.Courses = CXuLy.getDSCourse() ?? new List<CCourse>();
            return View(model);
        }

        
        [Route("Admin/ExamSet/xoaExamSet/{id}")]
        public IActionResult xoaExamSet(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.DeleteExamSet(id, token))
            {
                TempData["success"] = "Xóa bộ đề thành công!";
            }
            else
            {
                TempData["error"] = "Xóa bộ đề thất bại!";
            }
            return RedirectToAction("Index");
        }

        // Xem danh sách câu hỏi của bộ đề
        [HttpGet]
        [Route("Admin/ExamSet/viewDetail/{id}")]
        public IActionResult viewDetail(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // lấy danh sách câu hỏi có trong bộ đề đó
            List<CExamSetQuestion> questions = CXuLy.GetQuestionsByExamSet(id, token) ?? new List<CExamSetQuestion>();
            ViewBag.ExamSet = CXuLy.GetExamSetById(id, token);
            ViewBag.ExamSetId = id;
            return View(questions);
        }
        // Xóa nhiều bộ đề
        [HttpPost]
        [Route("Admin/ExamSet/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.DeleteExamSet(id, token))
                        successCount++;
                }

                if (successCount > 0)
                {
                    TempData["success"] = $"Đã xóa thành công {successCount}bộ đề.";
                    return Json(new { success = true, message = $"Đã xóa thành công {successCount} bộ đề." });
                }
                else
                {
                    TempData["error"] = "Không có bộ đề nào được xóa.";
                    return Json(new { success = false, message = $"Đã xóa thất bại." });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi: " + ex.Message;
                return Json(new { success = false, message = $"Đã xóa thất bại." });
            }
        }




        // Xóa câu hỏi khỏi bộ đề
        [HttpPost]
        [Route("Admin/ExamSet/xoaQuestionFromExamSet")]
        public IActionResult xoaQuestionFromExamSet(int examSetId, int questionId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool result = CXuLy.DeleteQuestionFromExamSet(examSetId, questionId, token);
            return Json(new { success = result });
        }

        // Đổi thứ tự câu hỏi trong bộ đề
        [HttpPut]
        [Route("Admin/ExamSet/SwapExamQuestionOrder")]
        public IActionResult SwapExamQuestionOrder([FromBody] CSwapExamQuestionOrder dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool result = CXuLy.SwapExamQuestionOrder(dto, token);
            return Json(new { success = result });
        }
        [HttpPost]
        [Route("Admin/ExamSet/DeleteMultipleQuestionsFromExamSet")]
        public IActionResult DeleteMultipleQuestionsFromExamSet([FromBody] List<CExamSetQuestion> questions)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool result = CXuLy.DeleteMultipleQuestionsFromExamSet(questions, token);
            return Json(new { success = result });
        }
        [HttpPut]
        [Route("Admin/ExamSet/UpdateExamQuestionOrder")]
        public IActionResult UpdateExamQuestionOrder([FromBody] CExamSetQuestion dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool result = CXuLy.UpdateExamQuestionOrder(dto.ExamSetId, dto.QuestionId, dto.QuestionOrder ?? 0, token);
            return Json(new { success = result });
        }

    }
}
