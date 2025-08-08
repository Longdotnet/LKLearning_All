using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Models;
using Newtonsoft.Json;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VocabularyController : BaseController
    {
        // Trang danh sách từ vựng, có lọc và phân trang
        [Route("Admin/Vocabulary")]
        public IActionResult Index(int page = 1, int? categoryId = null, string searchTerm = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var vocabularies = CXuLy.GetListVocabulary(token);

            // Lọc theo loại từ vựng
            if (categoryId.HasValue)
            {
                var mappings = CXuLy.GetListVocabularyCategoryMapping(token)
                    .Where(m => m.VocabularyCategoryId == categoryId.Value)
                    .Select(m => m.VocabularyId)
                    .ToList();
                vocabularies = vocabularies.Where(v => mappings.Contains(v.VocabularyId)).ToList();
            }

            // Tìm kiếm theo từ
            if (!string.IsNullOrEmpty(searchTerm))
            {
                vocabularies = vocabularies
                    .Where(v => v.Word.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Phân trang
            const int pageSize = 10;
            int total = vocabularies.Count;
            int skip = (page - 1) * pageSize;
            var data = vocabularies.Skip(skip).Take(pageSize).ToList();

            // Truyền dữ liệu cho dropdown filter
            ViewBag.Categories = CXuLy.GetListVocabularyCategory(token);
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Paginate = new Paginate(total, page, pageSize);

            return View(data);
        }

        // Hiển thị form tạo mới từ vựng
        [HttpGet]
        [Route("Admin/Vocabulary/Create")]
        public IActionResult formCreateVocabulary()
        {
            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;
            return View("formCreateVocabulary");
        }

        // Xử lý tạo mới từ vựng
        [HttpPost]
        [Route("Admin/Vocabulary/Create")]
        public IActionResult CreateVocabulary(CVocabulary model)
        {
            string token = HttpContext.Session.GetString("AuthToken");

            // Lấy meanings từ form (MeaningsJson)
            var meaningsJson = Request.Form["MeaningsJson"];
            if (!string.IsNullOrEmpty(meaningsJson))
            {
                try
                {
                    var meanings = JsonConvert.DeserializeObject<List<CVocabularyMeaning>>(meaningsJson);
                    model.Meanings = meanings;
                }
                catch
                {
                    ModelState.AddModelError("", "Dữ liệu nghĩa không hợp lệ.");
                }
            }

            // Lấy lại các trường từ form để đảm bảo lấy giá trị người dùng chỉnh sửa
            model.Word = Request.Form["Word"];
            model.Pronunciation = Request.Form["Pronunciation"];
            model.AudioUrlUk = Request.Form["AudioUrlUk"];
            model.AudioUrlUs = Request.Form["AudioUrlUs"];

            if (ModelState.IsValid)
            {
                // Kiểm tra từ đã tồn tại (theo Word, không phân biệt hoa thường)
                var vocabularies = CXuLy.GetListVocabulary(token);
                var existing = vocabularies.FirstOrDefault(v => v.Word.Equals(model.Word, StringComparison.OrdinalIgnoreCase));
                bool success;
                if (existing != null)
                {
                    // Nếu đã có, cập nhật
                    model.VocabularyId = existing.VocabularyId;
                    success = CXuLy.UpdateVocabulary(model, token);
                }
                else
                {
                    // Nếu chưa có, thêm mới
                    success = CXuLy.CreateVocabulary(model, token);
                }
                if (success)
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Thêm/cập nhật từ vựng thất bại.");
            }
            return View("formCreateVocabulary", model);
        }

        // Hiển thị form sửa từ vựng
        [HttpGet]
        [Route("Admin/Vocabulary/EditVocabulary/{id}")]
        public IActionResult formEditVocabulary(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var vocab = CXuLy.GetVocabularyById(id, token);
            if (vocab == null) return NotFound();

            var meanings = CXuLy.GetMeaningsByVocabularyId(id, token);
            vocab.Meanings = meanings;

            var baseUrl = HttpContext.RequestServices.GetService<IConfiguration>()["ApiSettings:BaseUrl"];
            ViewBag.ApiBaseUrl = baseUrl;

            return View(vocab);
        }

        // Xử lý cập nhật từ vựng
        [HttpPost]
        [Route("Admin/Vocabulary/EditVocabulary/{id}")]
        public IActionResult EditVocabulary(CVocabulary model)
        {
            string token = HttpContext.Session.GetString("AuthToken");

            var meaningsJson = Request.Form["MeaningsJson"];
            if (!string.IsNullOrEmpty(meaningsJson))
            {
                try
                {
                    var meanings = JsonConvert.DeserializeObject<List<CVocabularyMeaning>>(meaningsJson);
                    model.Meanings = meanings;
                }
                catch
                {
                    ModelState.AddModelError("", "Dữ liệu nghĩa không hợp lệ.");
                }
            }

            model.Word = Request.Form["Word"];
            model.Pronunciation = Request.Form["Pronunciation"];
            model.AudioUrlUk = Request.Form["AudioUrlUk"];
            model.AudioUrlUs = Request.Form["AudioUrlUs"];

            if (ModelState.IsValid)
            {
                bool success = CXuLy.UpdateVocabulary(model, token);
                if (success)
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Cập nhật từ vựng thất bại.");
            }
            return View(model);
        }

        // Xóa từ vựng
        [HttpGet]
        [Route("Admin/Vocabulary/DeleteVocabulary/{id}")]
        public IActionResult DeleteVocabulary(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool success = CXuLy.DeleteVocabulary(id, token);
            return RedirectToAction("Index");
        }

        // Xem chi tiết từ vựng
        [Route("Admin/Details/{id}")]
        public IActionResult Details(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var vocabulary = CXuLy.GetVocabularyById(id, token);
            var meanings = CXuLy.GetMeaningsByVocabularyId(id, token);
            ViewBag.Meanings = meanings;
            return View(vocabulary);
        }

        // Danh sách danh mục từ vựng
        [Route("Admin/Vocabulary/Categories")]
        public IActionResult Categories(int page = 1, int pageSize = 8, string searchTerm = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var categories = CXuLy.GetListVocabularyCategory(token);

            if (!string.IsNullOrEmpty(searchTerm))
                categories = categories.Where(c => c.VocabularyCategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            int total = categories.Count;
            var data = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Paginate = new Paginate(total, page, pageSize);
            return View(data);
        }

        // Danh sách liên kết từ vựng & danh mục
        [Route("Admin/Vocabulary/CategoryMappings")]
        public IActionResult CategoryMappings(int page = 1, int pageSize = 5, int? categoryId = null, int? vocabularyId = null)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var mappings = CXuLy.GetListVocabularyCategoryMapping(token);
            var vocabularies = CXuLy.GetListVocabulary(token);
            var categories = CXuLy.GetListVocabularyCategory(token);

            if (categoryId.HasValue)
                mappings = mappings.Where(m => m.VocabularyCategoryId == categoryId.Value).ToList();
            if (vocabularyId.HasValue)
                mappings = mappings.Where(m => m.VocabularyId == vocabularyId.Value).ToList();

            int total = mappings.Count;
            var data = mappings.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Vocabularies = vocabularies;
            ViewBag.Categories = categories;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentVocabularyId = vocabularyId;
            ViewBag.Paginate = new Paginate(total, page, pageSize);
            return View(data);
        }

        // Hiển thị form thêm liên kết từ vựng & danh mục
        [HttpGet]
        [Route("Admin/Vocabulary/AddMapping")]
        public IActionResult formAddMapping()
        {
            string token = HttpContext.Session.GetString("AuthToken");
            ViewBag.Vocabularies = CXuLy.GetListVocabulary(token);
            ViewBag.Categories = CXuLy.GetListVocabularyCategory(token);
            return View(new CVocabularyCategoryMapping());
        }

        // Xử lý thêm liên kết
        [HttpPost]
        [Route("Admin/Vocabulary/AddMapping")]
        public IActionResult AddMapping(CVocabularyCategoryMapping model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool success = CXuLy.AddVocabularyCategoryMapping(model, token);
            if (success)
                return RedirectToAction("CategoryMappings");
            ModelState.AddModelError("", "Thêm liên kết thất bại hoặc đã tồn tại.");
            ViewBag.Vocabularies = CXuLy.GetListVocabulary(token);
            ViewBag.Categories = CXuLy.GetListVocabularyCategory(token);
            return RedirectToAction("CategoryMappings");
        }

        // Hiển thị form sửa liên kết
        [HttpGet]
        [Route("Admin/Vocabulary/EditMapping")]
        public IActionResult formEditMapping(int vocabularyId, int categoryId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // Sử dụng constructor để khởi tạo đối tượng, giúp code ngắn gọn
            var mapping = new CVocabularyCategoryMapping(vocabularyId, categoryId);
            ViewBag.Vocabularies = CXuLy.GetListVocabulary(token);
            ViewBag.Categories = CXuLy.GetListVocabularyCategory(token);
            return View(mapping);
        }

        // Xử lý cập nhật liên kết
        [HttpPost]
        [Route("Admin/Vocabulary/EditMapping")]
        public IActionResult EditMapping(CVocabularyCategoryMapping model, int newCategoryId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // CHƯA TỐI ƯU: Không truyền đúng dữ liệu mới (newCategoryId) vào model, cần cập nhật lại model.VocabularyCategoryId = newCategoryId trước khi gọi CXuLy
            model.VocabularyCategoryId = newCategoryId;
            bool success = CXuLy.UpdateVocabularyCategoryMapping(model, token);
            if (success)
                return RedirectToAction("CategoryMappings");
            ModelState.AddModelError("", "Cập nhật liên kết thất bại hoặc đã tồn tại.");
            ViewBag.Vocabularies = CXuLy.GetListVocabulary(token);
            ViewBag.Categories = CXuLy.GetListVocabularyCategory(token);
            return RedirectToAction("CategoryMappings");
        }

        // Xóa liên kết
        [HttpGet]
        [Route("Admin/Vocabulary/DeleteMapping")]
        public IActionResult DeleteMapping(int vocabularyId, int categoryId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            // CHƯA TỐI ƯU: Hàm CXuLy.DeleteVocabularyCategoryMapping chưa nhận đủ thông tin để xác định mapping cần xóa, nên cần truyền model hoặc DTO chứa cả vocabularyId và categoryId
            var mapping = new CVocabularyCategoryMapping(vocabularyId, categoryId);
            bool success = CXuLy.DeleteVocabularyCategoryMapping(mapping.VocabularyId,mapping.VocabularyCategoryId,DateTime.Now,token);
            return RedirectToAction("CategoryMappings");
        }
        // Hiển thị form thêm danh mục
        [HttpGet]
        [Route("Admin/Vocabulary/AddCategory")]
        public IActionResult formAddCategory()
        {
            return View(new CVocabularyCategory());
        }

        // Xử lý thêm danh mục
        [HttpPost]
        [Route("Admin/Vocabulary/AddCategory")]
        public IActionResult AddCategory(CVocabularyCategory model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool success = CXuLy.AddVocabularyCategory(model, token);
            if (success)
                return RedirectToAction("Categories");
            ModelState.AddModelError("", "Thêm danh mục thất bại hoặc đã tồn tại.");
            return RedirectToAction("Categories");
        }

        // Hiển thị form sửa danh mục
        [HttpGet]
        [Route("Admin/Vocabulary/EditCategory")]
        public IActionResult formEditCategory(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            var category = CXuLy.GetListVocabularyCategory(token).FirstOrDefault(c => c.VocabularyCategoryId == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // Xử lý sửa danh mục
        [HttpPost]
        [Route("Admin/Vocabulary/EditCategory/{id}")]
        public IActionResult EditCategory(int id,CVocabularyCategory model)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool success = CXuLy.UpdateVocabularyCategory(id, model, token);
            if (success)
                return RedirectToAction("Categories");
            ModelState.AddModelError("", "Cập nhật danh mục thất bại.");
            return RedirectToAction("Categories");
        }

        // Xóa danh mục
        [HttpGet]
        [Route("Admin/Vocabulary/DeleteCategory")]
        public IActionResult DeleteCategory(int id)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            bool success = CXuLy.DeleteVocabularyCategory(id, token);
            return RedirectToAction("Categories");
        }

    }
}
