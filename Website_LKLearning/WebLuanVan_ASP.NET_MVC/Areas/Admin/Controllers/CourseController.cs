using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Common;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CourseController : BaseController
    {
        [Route("course")]
        public IActionResult Index(int page,int? categoryId,int? levelId, int? packageId)
        {
            string token=HttpContext.Session.GetString("AuthToken");
            List<CCourse> dsCourse = CXuLy.getDSCourse();
            // Áp dụng filter nếu có
            if (categoryId.HasValue)
            {
                dsCourse = dsCourse.Where(q => q.CategoryId == categoryId.Value).ToList();
            }

            if (levelId.HasValue)
            {
                dsCourse = dsCourse.Where(q => q.LevelId == levelId.Value).ToList();
            }
            if(packageId.HasValue){
                dsCourse = dsCourse.Where(q => q.PackageId == packageId.Value).ToList();
            }
            // Lấy danh sách danh mục (Category)
            List<CCategory> categories = CXuLy.getDSCategory(); // Giả sử hàm này trả về danh sách Category
            List<CLevel> levels = CXuLy.getDSLevel();
            List<CPackage> packages = CXuLy.getDSPackage();
            // Truyền danh sách Category cho view
            ViewBag.Categories = categories;
            ViewBag.Levels = levels;
            ViewBag.Packages = packages;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentLevel = levelId;
            ViewBag.CurrentPackage = packageId;
            // phân trang paginate
            // hiển thị bao nhieu items 
            // B1: cài đặt thông số phân trang
            const int pageSize = 5; 
            if (page < 1)
            {
                page = 1; // Đảm bảo trang bắt đầu từ 1
            }
            // B2: Tính toán dữ liệu
            int dem = dsCourse.Count; // Tổng số item có trong ds đó

            var paginatenew = new Paginate(dem, page, pageSize); //Khởi tạo đối tượng phân trang
            // B3: lấy dữ liệu trang hiện tại
            int recSkip = (page-1)* pageSize; // Vĩ trí bắt đầu lấy dữ liệu

            var data = dsCourse
                .Skip(recSkip) // Bỏ qua các item trước đó
                .Take(pageSize) // Lấy số lượng item của hiện tại
                .ToList();
            // B4 Truyền dữ liệu ra View
            ViewBag.Paginate = paginatenew; // Truyền thông tin phân trang
            return View(data);
        }
        [Route("course/xoacourse/{id}")]
        public IActionResult xoaCourse(string id)
        {
            string token= HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.xoaCourse(id, token))
                {
                    TempData["success"] = "Xóa thành công khóa học!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Xóa không thành công khóa học vì còn bài học!";
                    return RedirectToAction("Index");

                }
            }
            catch
            {
                return Content("Xóa không thành công");
            }
        }
        [Route("course/DeleteMultiple")]
        public IActionResult DeleteMultiple(List<int> selectedIds)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                int successCount = 0;
                foreach (var id in selectedIds)
                {
                    if (CXuLy.xoaCourse(id.ToString(), token))
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["success"]= "Xóa thành công";
                    return Json(new { success = true, message = $"Đã xóa thành công {successCount}/{selectedIds.Count} bài học." });
                }
                else
                {
                    TempData["error"] = "Không có bài học nào được xóa.";
                    return Json(new { success = false, message = "Không có bài học nào được xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [Route("course/themcourse")]
        public IActionResult formThemCourse(CCourse x, int categoryId, int levelId, int packageId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            ViewBag.Categories =CXuLy.getDSCategory();
            // Lấy danh sách levels từ database
            ViewBag.Levels = CXuLy.getDSLevel();
            ViewBag.Packages = CXuLy.getDSPackage();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentLevel = levelId;
            ViewBag.CurrentPackage = packageId;
            return View();
        }

        [HttpPost]
        [Route("course/themcourse")]
        public async Task<IActionResult> ThemCourse(CCourse x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.themCourse(x, token))
                {
                    // Gửi thông báo đến tất cả thiết bị đã đăng ký topic "all"
                    var (success, message) = await CXuLy.SendFcmNotificationToTopicAsync(
                        "all",
                        $"Khóa học mới: {x.CourseName} đã được thêm thành công!",
                        "Thông báo từ hệ thống"
                    );

                    if (!success)
                        TempData["warning"] = message;
                    TempData["success"] = "Thêm khóa học thành công và đã gửi thông báo cho tất cả người dùng!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Thêm khóa học không thành công";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [Route("course/suaCourse/{id}")]
        public IActionResult formSuaCourse(string id,int categoryId, int levelId, int packageId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            CCourse x = CXuLy.getCourseById(id);
            ViewBag.Categories = CXuLy.getDSCategory();
            // Lấy danh sách levels từ database
            ViewBag.Levels = CXuLy.getDSLevel();
            ViewBag.Packages = CXuLy.getDSPackage();
            // Lấy giá trị int của vietbag category và level
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentLevel = levelId;
            ViewBag.CurrentPackage = packageId;
            return View(x);
        }
        [HttpPost]
        [Route("course/suaCourse/{id}")]
        public IActionResult SuaCourse(string id, CCourse x)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            try
            {
                if (CXuLy.editCourse(id, x, token))
                {
                    TempData["success"] = "Sửa thành công khóa học!";
                    return RedirectToAction("Index");
                }         
                else
                {
                    TempData["success"] = "Sửa không thành công khóa học!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return Content("Sửa không thành công");
            }
        }
        [HttpGet]
        public IActionResult searchCategory(string searchTerm)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            List<CCategory> categories = CXuLy.getDSCategory();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = categories.Where(c => c.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // dùng id, text, items, totalCount là do Select2 nó quy ước thế
            var result = categories.Select(c => new {   
                id = c.CategoryId, // giá trị được chọn
                text = c.CategoryName // giá trị hiển thị
            }).ToList();

            return Json(new
            {
                items = result, // danh sách kết quả đã chọn
                totalCount = result.Count // tổng kết quả/ dùng cho phân trtang
            });
        }
        [HttpGet]
        public IActionResult searchLevel(string searchTerm)
        {
            string token=HttpContext.Session.GetString("AuthToken");
            List<CLevel> levels = CXuLy.getDSLevel();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                levels = levels.Where(c => c.LevelName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = levels.Select(c => new
            {
                id = c.LevelId,
                text = c.LevelName
            }).ToList();
            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
        [HttpGet]
        public IActionResult searchPackage(string searchTerm)
        {
           
            List<CPackage> packages = CXuLy.getDSPackage();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                packages = packages.Where(c => c.PackageName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var result = packages.Select(c => new
            {
                id = c.PackageId,
                text = c.PackageName
            }).ToList();
            return Json(new
            {
                items = result,
                totalCount = result.Count
            });
        }
        [Route("course/viewQuestion/{id}")]
        public IActionResult viewQuestion(string id, int page)
            {
            string token = HttpContext.Session.GetString("AuthToken");
            CCourse course = CXuLy.getCourseById(id);
            if (course == null)
            {
                TempData["error"] = "Khóa học không tồn tại";
                return RedirectToAction("Index");
            }

            // Initialize ViewBag lists to avoid null references
            ViewBag.Packages = CXuLy.getDSPackage();
            ViewBag.Levels = CXuLy.getDSLevel();
            ViewBag.Categories = CXuLy.getDSCategory();

            // Fetch CourseLessons for the course (already sorted by OrderNo from API)
            List<CCourseLessons> courseLessons = CXuLy.GetCourseLessonByID(id, token) ?? new List<CCourseLessons>();

            // Fetch all lessons
            List<CLesson> allLessons = CXuLy.getDSLesson() ?? new List<CLesson>();

            // Map lessons to maintain API's OrderNo order without re-sorting
            var sortedLessons = courseLessons
                .Join(allLessons,
                      cl => cl.LessonId,
                      l => l.LessonId,
                      (cl, l) => l)
                .ToList();

            // Pagination
            const int pageSize = 5;
            if (page < 1)
            {
                page = 1;
            }
            int totalItems = sortedLessons.Count;
            var paginate = new Paginate(totalItems, page, pageSize);
            int skip = (page - 1) * pageSize;
            var paginatedLessons = sortedLessons.Skip(skip).Take(pageSize).ToList();

            ViewBag.Lessons = paginatedLessons;
            ViewBag.Paginate = paginate;

            // Pass CourseLessons for OrderNo display (optional)
            ViewBag.CourseLessons = courseLessons;

            return View(course);
        }
        [HttpPut]
        [Route("course/SwapLessonOrder")]
        public IActionResult SwapLessonOrder([FromBody] CSwapLessonOrder dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            try
            {
                bool result = CXuLy.SwapLessonOrder(dto, token);
                if (result)
                {
                    return Ok(new { success = true, message = $"Đã hoán đổi vị trí {dto.SourceOrderNo} ↔ {dto.TargetOrderNo} cho khóa học {dto.CourseId}." });
                }
                else

                {
                    return BadRequest(new { success = false, message = "Không thể hoán đổi thứ tự bài học." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("course/UpdateLessonOrder")]
        public IActionResult UpdateLessonOrder([FromBody] CCourseLessons dto)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            try
            {
                bool result = CXuLy.UpdateLessonOrder(dto, token);
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
        [Route("course/xoaLessonFromCourse/{courseId}/{lessonId}")]
        public IActionResult xoaLessonFromCourse(string courseId, string lessonId)
        {
            string token = HttpContext.Session.GetString("AuthToken");
            if (CXuLy.xoaLessonRaKhoiKhoaHoc(courseId, lessonId, token))
            {
                TempData["success"] = "Xóa thành công ra khỏi khóa học";
                return RedirectToAction("viewQuestion", new { id = courseId });
            }
            else
            {
                TempData["error"] = "Xóa không thành công ";
                return RedirectToAction("viewQuestion", new { id = courseId });
            }
        }
    }

    public class FcmService
    {
        private readonly IConfiguration _configuration;

        public FcmService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message)> SendFcmNotificationAsync(string fcmToken, string title, string body)
        {
            try
            {
                string? serviceAccountKeyPath = _configuration["FcmSettings:ServiceAccountKeyPath"];
                string? projectId = _configuration["FcmSettings:ProjectId"];

                if (string.IsNullOrEmpty(serviceAccountKeyPath) || string.IsNullOrEmpty(projectId))
                {
                    return (false, "FCM configuration is missing.");
                }

                var credential = GoogleCredential.FromFile(serviceAccountKeyPath)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var data = new
                    {
                        message = new
                        {
                            token = fcmToken,
                            notification = new
                            {
                                title = title,
                                body = body
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send",
                        content);

                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return (true, "Gửi thông báo thành công");
                    }
                    else
                    {
                        return (false, $"Gửi thông báo thất bại: {response.StatusCode} - {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }
    }
}
