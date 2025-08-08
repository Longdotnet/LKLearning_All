using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionTypeController : BaseController
    {
        [Route("questiontype")]
        public IActionResult Index(int page)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CQuestionType> dsQuestionType = CXuLy.getDSQuestionType();
            // Phân trang  
            const int pageSize = 5;
            if (page < 1) page = 1;

            int totalItems = dsQuestionType.Count;
            var paginate = new Paginate(totalItems, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = dsQuestionType.Skip(recSkip).Take(pageSize).ToList();

            ViewBag.Paginate = paginate;



            return View(data);
        }
        [Route("questiontype/create")]
        public IActionResult formThemQuestionType(CQuestionType x)
        {
            return View();
        }
        [HttpPost]
        [Route("questiontype/create")]
        public IActionResult ThemQuestionType(CQuestionType x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themQuestionType(x, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("Thêm không thành công");
                }
            }
            catch
            {
                return Content("Thêm không thành công");
            }
        }
        [Route("questiontype/xoaquestiontype/{id}")]
        public IActionResult xoaQuestionType(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaQuestionType(id, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("Xóa không thành công");
                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }

        }
        [Route("questiontype/suaquestiontype/{id}")]
        public IActionResult formSuaQuestionType(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CQuestionType questionType = CXuLy.getQuestionTypeById(id);
            return View(questionType);
        }
        [HttpPost]
        [Route("questiontype/suaquestiontype/{id}")]
        public IActionResult SuaQuestionType(string id, CQuestionType x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.editQuestionType(id, x, token))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("Sửa không thành công");
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }
        [Route("questiontype/viewquestiontype/{id}")]
        public IActionResult viewQuestionType(string id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CQuestionType x = CXuLy.getQuestionTypeById(id);
            return View(x);
        }
    }
}
