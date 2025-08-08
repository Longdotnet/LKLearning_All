using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Base.Controllers;
using WebLuanVan_ASP.NET_MVC.Models;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class CourseController : BaseController
    {
        [Route("User/Courses")]
        public IActionResult Index(int page = 1, int? categoryId = null, int? levelId = null, int? packageId = null, string searchTerm = null)
        {
            // Get all courses
            List<CCourse> dsCourse = CXuLy.getDSCourse() ?? new List<CCourse>();

            // Filter by category, level, package
            if (categoryId.HasValue)
                dsCourse = dsCourse.Where(q => q.CategoryId == categoryId.Value).ToList();
            if (levelId.HasValue)
                dsCourse = dsCourse.Where(q => q.LevelId == levelId.Value).ToList();
            if (packageId.HasValue)
                dsCourse = dsCourse.Where(q => q.PackageId == packageId.Value).ToList();

            // Search by course name
            if (!string.IsNullOrWhiteSpace(searchTerm))
                dsCourse = dsCourse.Where(q => q.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            // Get categories, levels, packages for filter dropdowns
            ViewBag.Categories = CXuLy.getDSCategory() ?? new List<CCategory>();
            ViewBag.Levels = CXuLy.getDSLevel() ?? new List<CLevel>();
            ViewBag.Packages = CXuLy.getDSPackage() ?? new List<CPackage>();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentLevel = levelId;
            ViewBag.CurrentPackage = packageId;
            ViewBag.SearchTerm = searchTerm;

            // Pagination
            const int pageSize = 10;
            if (page < 1) page = 1;
            int totalCount = dsCourse.Count;
            int skip = (page - 1) * pageSize;
            var paginatedCourses = dsCourse.Skip(skip).Take(pageSize).ToList();

            ViewBag.Paginate = new Paginate(totalCount, page, pageSize);

            return View(paginatedCourses);
        }
       
    }
}
