using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionLevelController : BaseController
    {
        [Route("QuestionLevel/Index")]
        public IActionResult Index(int page)
        {
            List<CQuestionLevel> dsLevel = CXuLy.getDSQuestionLevel();
            if (dsLevel == null)
            {
                ViewBag.Error = "Không lấy được danh sách mức độ câu hỏi";
                return View();
            }
            // Phân trang  
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsLevel.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsLevel.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;

            return View(data);
        }
        [Route("QuestionLevel/formCreate")]
        public IActionResult formCreate(CQuestionLevel x)
        {
            return View();
        }
        [HttpPost]
        [Route("QuestionLevel/formCreate")]
        public IActionResult Create(CQuestionLevel x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themQuestionLevel(x, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Thêm không thành công";
                    return View("formCreate");
                }
            }
            catch
            {
                ViewBag.Error = "Thêm không thành công";
                return View("formCreate");
            }

        }
        [Route("QuestionLevel/Delete/{id}")]
        public IActionResult Delete(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaQuestionLevel(id, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Xóa không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ViewBag.Error = "Xóa không thành công";
                return RedirectToAction("Index");
            }
        }
        [Route("QuestionLevel/formEdit/{id}")]
        public IActionResult formEdit(string id)
        {
            CQuestionLevel? x = CXuLy.getQuestionLevelById(id);
            if (x == null)
            {
                ViewBag.Error = "Không tìm thấy mức độ câu hỏi";
                return RedirectToAction("Index");
            }
            return View(x);
        }
        [HttpPost]
        [Route("QuestionLevel/formEdit/{id}")]
        public IActionResult Edit(CQuestionLevel x, string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.suaQuestionLevel(id, x, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Cập nhật không thành công";
                    return View("formEdit", x);
                }
            }
            catch
            {
                ViewBag.Error = "Cập nhật không thành công";
                return View("formEdit", x);
            }
        }
        [Route("QuestionLevel/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            int successCount = 0;
            foreach (var id in selectedIds)
            {
                if (CXuLy.xoaQuestionLevel(id.ToString(), token))
                {
                    successCount++;
                }
            }
            if (successCount > 0)
            {
                TempData["success"] = $"Xóa thành công {successCount} cấp độ câu hỏi";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["success"] = $"Xóa không thành công";
                return RedirectToAction("Index");
            }
        }
    }
}
