using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.Wizard
{
    public class Step2_ArticleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Step2_ArticleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPage { get; set; } = new ContentPage();

        // Két sắt giữ ID của Menu từ Bước 1 truyền sang
        [BindProperty]
        public int TargetMenuId { get; set; }

        public string MenuName { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            if (menuId <= 0) return RedirectToPage("./Step1_Menu");

            var menu = await _context.NavigationMenus.FindAsync(menuId);
            if (menu == null) return RedirectToPage("./Step1_Menu");

            TargetMenuId = menu.Id;
            MenuName = menu.Name;

            // Khởi tạo mặc định
            ContentPage.IsVisible = true;
            ContentPage.HasSidebar = true;
            ContentPage.Category = "general"; // Có thể đổi tùy ý

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Xóa validate mảng liên kết để không báo lỗi ảo
            ModelState.Remove("ContentPage.SidebarItems");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. LƯU BÀI VIẾT VÀO DATABASE
            _context.ContentPages.Add(ContentPage);
            await _context.SaveChangesAsync();
            // Lưu xong, ContentPage.Id sẽ có số mới

            // 2. NỐI TƠ HỒNG: Gắn Bài Viết vào Menu
            var menuToUpdate = await _context.NavigationMenus.FindAsync(TargetMenuId);
            if (menuToUpdate != null)
            {
                menuToUpdate.ContentPageId = ContentPage.Id;
                await _context.SaveChangesAsync();
            }

            // 3. ĐI TIẾP BƯỚC 3 (Mang theo ID của Bài viết)
            return RedirectToPage("./Step3_Sidebar", new { id = ContentPage.Id });
        }
    }
}