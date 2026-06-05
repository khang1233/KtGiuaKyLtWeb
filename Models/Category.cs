using System.ComponentModel.DataAnnotations;

namespace KtGiuaKy.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
