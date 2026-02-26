using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourProjectName.Models;
using Microsoft.AspNetCore.Http; // BẮT BUỘC ĐỂ DÙNG IFormFile
namespace CMS.Models
{
    public class ContentPage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trang")]
        [Display(Name = "Tiêu đề trang")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đường dẫn (Slug)")]
        [Display(Name = "Đường dẫn (Slug)")]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        // ← THÊM DÒNG NÀY - QUAN TRỌNG!
        [Required(ErrorMessage = "Vui lòng chọn chuyên mục")]
        [Display(Name = "Chuyên mục")]
        [StringLength(50)]
        public string Category { get; set; } = "general";
        // VD: "gioi-thieu", "san-pham", "tin-tuc", "lien-he"

        [Display(Name = "Hiển thị")]
        public bool IsVisible { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ← THÊM DÒNG NÀY (Optional nhưng nên có)


        // Navigation property
        public virtual ICollection<SidebarItem>? SidebarItems { get; set; }
        public bool HasSidebar { get; set; } = true;
        // Khóa ngoại trỏ về ChuyenMuc (Cho phép null với trang tĩnh 1-1)
        public int? ChuyenMucId { get; set; }

        [ForeignKey("ChuyenMucId")]
        public virtual ChuyenMuc? ChuyenMuc { get; set; }
        [Display(Name = "Ảnh đại diện")]
        [StringLength(500)]
        public string? Thumbnail { get; set; }
        [NotMapped]
        [Display(Name = "Chọn ảnh tải lên")]
        public IFormFile? ThumbnailFile { get; set; }

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;
    }
}