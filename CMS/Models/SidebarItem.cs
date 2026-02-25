using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourProjectName.Models;

namespace CMS.Models
{
    public class SidebarItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LinkUrl { get; set; } = string.Empty;

        public string? Content { get; set; } = string.Empty;

        // Cột mới thêm
        public string? Category { get; set; } // Cho phép null

        public int SortOrder { get; set; } = 0;

        public bool IsVisible { get; set; } = true;

        // Cột cũ (Foreign Key)
        public int? ContentPageId { get; set; }
        public virtual ContentPage? ContentPage { get; set; }
        // [MỚI - QUAN TRỌNG]
        public int? MenuId { get; set; } // Cho phép null tạm thời

        [ForeignKey("MenuId")]
        public virtual NavigationMenu? Menu { get; set; } // Dây liên kết sang bảng Menu
                                                          // Khóa ngoại trỏ về ChuyenMuc
        public int? ChuyenMucId { get; set; }

        [ForeignKey("ChuyenMucId")]
        public virtual ChuyenMuc? ChuyenMuc { get; set; }

    }
}