using KtGiuaKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KtGiuaKy.Controllers
{
    [Authorize(Roles = "Student")]
    [Route("enroll")]
    public class EnrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /enroll/my-courses (Câu 7 - My Courses)
        [HttpGet]
        [Route("my-courses")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            return View(enrollments);
        }

        // POST: /enroll/course (Câu 6 - Enroll Course)
        [HttpPost]
        [Route("course")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Check if course exists
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Check if already registered
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký học phần này rồi.";
                return RedirectToAction("Index", "Home");
            }

            // Perform enrollment
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đăng ký học phần \"{course.Name}\" thành công!";
            return RedirectToAction("Index", "Home");
        }

        // POST: /enroll/cancel (Câu 6 - Cancel/Unenroll Course)
        [HttpPost]
        [Route("cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int courseId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "Học phần này chưa được đăng ký.";
                return RedirectToAction("Index", "Home");
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Hủy đăng ký học phần \"{enrollment.Course?.Name}\" thành công.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
