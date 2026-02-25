using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourProjectName.Models;

namespace CMS.Models
{
    public class NavigationMenu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên menu không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Gán mặc định để hết lỗi null

        public string? Url { get; set; } // Cho phép null

        public int? ParentId { get; set; }

        // --- CỘT BẠN ĐANG THIẾU ---
        public int DisplayOrder { get; set; } = 0; // Dùng để sắp xếp menu

        public bool IsVisible { get; set; } = true;
        [NotMapped]
        public int PostCount { get; set; }

        [NotMapped]
        public int SidebarCount { get; set; }

        [ForeignKey("ParentId")]
        public virtual NavigationMenu? Parent { get; set; }

        public virtual ICollection<NavigationMenu> Children { get; set; } = new List<NavigationMenu>();
        public virtual ICollection<SidebarItem> SidebarItems { get; set; } = new List<SidebarItem>();
        // --- THÊM MỚI ---
        public int? ContentPageId { get; set; } // Link động (Dùng cho bài viết nội bộ)

        [ForeignKey("ContentPageId")]
        public virtual ContentPage? LinkedPage { get; set; } // Để lấy Slug mới nhất
                                                             // Khóa ngoại trỏ về ChuyenMuc (Dành cho Option 2 - Giao diện Bách Khoa)
        public int? ChuyenMucId { get; set; }

        [ForeignKey("ChuyenMucId")]
        public virtual ChuyenMuc? ChuyenMuc { get; set; }

    }
}
