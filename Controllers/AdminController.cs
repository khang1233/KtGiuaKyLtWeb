using KtGiuaKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KtGiuaKy.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: /admin or /admin/dashboard (Câu 10 - Admin Dashboard)
        [HttpGet]
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Total courses
            var totalCourses = await _context.Courses.CountAsync();

            // Total students (users in the Student role)
            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            int totalStudents = 0;
            if (studentRole != null)
            {
                totalStudents = await _context.UserRoles.CountAsync(ur => ur.RoleId == studentRole.Id);
            }
            else
            {
                // Fallback: total users minus admins
                totalStudents = await _userManager.Users.CountAsync();
            }

            // Total enrollments
            var totalEnrollments = await _context.Enrollments.CountAsync();

            // Additional stats to make the dashboard look premium
            var enrollmentsByCategory = await _context.Courses
                .Include(c => c.Category)
                .GroupBy(c => c.Category!.Name)
                .Select(g => new { Category = g.Key, Count = g.SelectMany(c => c.Enrollments).Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var coursesWithCounts = await _context.Courses
                .Select(c => new
                {
                    Name = c.Name,
                    Credits = c.Credits,
                    Lecturer = c.Lecturer,
                    EnrollmentCount = c.Enrollments.Count()
                })
                .OrderByDescending(c => c.EnrollmentCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalEnrollments = totalEnrollments;
            ViewBag.EnrollmentsByCategory = enrollmentsByCategory;
            ViewBag.PopularCourses = coursesWithCounts;

            return View();
        }

        // GET: /admin/courses (Câu 2 - CRUD Course List)
        [HttpGet]
        [Route("courses")]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .ToListAsync();
            return View(courses);
        }

        // GET: /admin/create-course (Câu 2 - Create Course)
        [HttpGet]
        [Route("create-course")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: /admin/create-course (Câu 2 - Create Course)
        [HttpPost]
        [Route("create-course")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    course.Image = "/uploads/" + uniqueFileName;
                }
                else if (string.IsNullOrEmpty(course.Image))
                {
                    // Default fallback image
                    course.Image = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=500&auto=format&fit=crop&q=60";
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm học phần mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /admin/edit-course/{id} (Câu 2 - Edit Course)
        [HttpGet]
        [Route("edit-course/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // POST: /admin/edit-course/{id} (Câu 2 - Edit Course)
        [HttpPost]
        [Route("edit-course/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? imageFile)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Track existing image path in case no new image is uploaded
                    var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        course.Image = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        course.Image = existingCourse.Image;
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật học phần thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /admin/delete-course/{id} (Câu 2 - Delete Course)
        [HttpGet]
        [Route("delete-course/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: /admin/delete-course/{id} (Câu 2 - Delete Course Confirm)
        [HttpPost, ActionName("Delete")]
        [Route("delete-course/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa học phần thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
