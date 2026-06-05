using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KtGiuaKy.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên học phần không được để trống")]
        [Display(Name = "Tên học phần")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh minh họa")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Số tín chỉ không được để trống")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        [Display(Name = "Số tín chỉ")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Tên giảng viên không được để trống")]
        [Display(Name = "Giảng viên phụ trách")]
        public string Lecturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục không được để trống")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
