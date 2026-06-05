using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KtGiuaKy.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã người dùng")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mã học phần")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Ngày đăng ký")]
        public DateTime EnrollDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }
}
