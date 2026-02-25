using CMS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Models // Đổi lại theo tên project của bạn
{
    public class ChuyenMuc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chuyên mục không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } // Ví dụ: Tin tức, Hoạt động chung

        [MaxLength(255)]
        public string Slug { get; set; } // Ví dụ: tin-tuc, hoat-dong-chung

        // Navigation Properties (Để EF Core biết 1 Chuyên Mục ôm những cái gì)
        public virtual ICollection<ContentPage> ContentPages { get; set; } = new List<ContentPage>();
        public virtual ICollection<NavigationMenu> Menus { get; set; } = new List<NavigationMenu> ();
        public virtual ICollection<SidebarItem> Sidebars { get; set; } = new List<SidebarItem>();
    }
}