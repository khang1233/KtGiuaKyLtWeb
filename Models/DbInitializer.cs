using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KtGiuaKy.Models
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Ensure database is created/migrated
            await context.Database.MigrateAsync();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Student" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            var adminUser = await userManager.FindByEmailAsync("admin@course.com");
            if (adminUser == null)
            {
                var admin = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@course.com",
                    EmailConfirmed = true
                };
                var createPowerUser = await userManager.CreateAsync(admin, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 3. Seed Student User
            var studentUser = await userManager.FindByEmailAsync("student@course.com");
            if (studentUser == null)
            {
                var student = new IdentityUser
                {
                    UserName = "student",
                    Email = "student@course.com",
                    EmailConfirmed = true
                };
                var createStudent = await userManager.CreateAsync(student, "Student@123");
                if (createStudent.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                }
            }

            // 4. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Công nghệ thông tin" },
                    new Category { Name = "Kinh tế" },
                    new Category { Name = "Ngoại ngữ" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 5. Seed Courses (at least 8 courses to test pagination of 5 per page)
            if (!await context.Courses.AnyAsync())
            {
                var itCategory = await context.Categories.FirstAsync(c => c.Name == "Công nghệ thông tin");
                var econCategory = await context.Categories.FirstAsync(c => c.Name == "Kinh tế");
                var langCategory = await context.Categories.FirstAsync(c => c.Name == "Ngoại ngữ");

                var courses = new List<Course>
                {
                    new Course
                    {
                        Name = "Lập trình ASP.NET Core MVC",
                        Credits = 3,
                        Lecturer = "ThS. Nguyễn Văn A",
                        Image = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=500&auto=format&fit=crop&q=60",
                        CategoryId = itCategory.Id
                    },
                    new Course
                    {
                        Name = "Phát triển ứng dụng Web động",
                        Credits = 3,
                        Lecturer = "TS. Trần Thị B",
                        Image = "https://images.unsplash.com/photo-1547082299-de196ea013d6?w=500&auto=format&fit=crop&q=60",
                        CategoryId = itCategory.Id
                    },
                    new Course
                    {
                        Name = "Cơ sở dữ liệu nâng cao",
                        Credits = 4,
                        Lecturer = "PGS.TS. Lê Hoàng C",
                        Image = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=500&auto=format&fit=crop&q=60",
                        CategoryId = itCategory.Id
                    },
                    new Course
                    {
                        Name = "Kinh tế vĩ mô",
                        Credits = 2,
                        Lecturer = "ThS. Phạm Thanh D",
                        Image = "https://images.unsplash.com/photo-1590283603385-17ffb3a7f29f?w=500&auto=format&fit=crop&q=60",
                        CategoryId = econCategory.Id
                    },
                    new Course
                    {
                        Name = "Quản trị học đại cương",
                        Credits = 2,
                        Lecturer = "TS. Đỗ Hoàng E",
                        Image = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=500&auto=format&fit=crop&q=60",
                        CategoryId = econCategory.Id
                    },
                    new Course
                    {
                        Name = "Kế toán tài chính doanh nghiệp",
                        Credits = 3,
                        Lecturer = "ThS. Ngô Minh F",
                        Image = "https://images.unsplash.com/photo-1554224155-8d04cb21cd6c?w=500&auto=format&fit=crop&q=60",
                        CategoryId = econCategory.Id
                    },
                    new Course
                    {
                        Name = "Tiếng Anh giao tiếp nâng cao",
                        Credits = 2,
                        Lecturer = "Dr. Sarah Johnson",
                        Image = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?w=500&auto=format&fit=crop&q=60",
                        CategoryId = langCategory.Id
                    },
                    new Course
                    {
                        Name = "Tiếng Trung thương mại cơ bản",
                        Credits = 3,
                        Lecturer = "ThS. Vương Tiểu G",
                        Image = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=500&auto=format&fit=crop&q=60",
                        CategoryId = langCategory.Id
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }
    }
}
