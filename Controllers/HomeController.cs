using KtGiuaKy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace KtGiuaKy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: / (Home Page)
        public async Task<IActionResult> Index(string? searchString, int? page)
        {
            // 1. Get query
            var coursesQuery = _context.Courses
                .Include(c => c.Category)
                .AsQueryable();

            // 2. Search course by name (Câu 8)
            if (!string.IsNullOrEmpty(searchString))
            {
                coursesQuery = coursesQuery.Where(c => c.Name.Contains(searchString));
                ViewBag.CurrentFilter = searchString;
            }

            // 3. Pagination calculation (Câu 1)
            int pageSize = 5;
            int pageNumber = page ?? 1;
            if (pageNumber < 1) pageNumber = 1;

            int totalCourses = await coursesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCourses / pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var paginatedCourses = await coursesQuery
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Fetch registered course IDs for the logged-in student (Câu 6)
            var enrolledCourseIds = new List<int>();
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    enrolledCourseIds = await _context.Enrollments
                        .Where(e => e.UserId == userId)
                        .Select(e => e.CourseId)
                        .ToListAsync();
                }
            }

            // Pass pagination info to ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.EnrolledCourseIds = enrolledCourseIds;

            return View(paginatedCourses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
