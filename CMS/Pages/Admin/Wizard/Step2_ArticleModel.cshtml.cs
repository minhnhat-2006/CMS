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

        // ==================================================================
        // 🔥 HÀM MỚI: Dò ngược cây phả hệ tìm Menu Gốc (Chống lỗi Menu nhiều cấp)
        // ==================================================================
        private async Task<string> GetRootCategoryNameAsync(int menuId)
        {
            var menu = await _context.NavigationMenus.FindAsync(menuId);
            if (menu == null) return string.Empty;

            // Dùng vòng lặp while để leo ngược lên tận đỉnh, dù là cháu hay chắt
            while (menu.ParentId.HasValue && menu.ParentId.Value > 0)
            {
                var parent = await _context.NavigationMenus.FindAsync(menu.ParentId.Value);
                if (parent == null) break;
                menu = parent; // Gán lại để tiếp tục leo lên
            }

            // Thoát vòng lặp, lúc này menu chính là Menu Gốc ngoài cùng
            return menu.Name;
        }

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            if (menuId <= 0) return RedirectToPage("./Step1_Menu");

            // 1. Tìm Menu con vừa tạo ở Bước 1
            var currentMenu = await _context.NavigationMenus.FindAsync(menuId);
            if (currentMenu == null) return RedirectToPage("./Step1_Menu");

            TargetMenuId = currentMenu.Id;
            MenuName = currentMenu.Name;

            // Khởi tạo các giá trị mặc định cho form
            ContentPage.IsVisible = true;
            ContentPage.HasSidebar = true;

            // ------------------------------------------------------------------
            // 🔥 LOGIC TÌM MENU GỐC ĐỂ KẾ THỪA CATEGORY NGAY TRÊN GIAO DIỆN
            // ------------------------------------------------------------------
            ContentPage.Category = await GetRootCategoryNameAsync(currentMenu.Id);

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

            var currentMenu = await _context.NavigationMenus.FindAsync(TargetMenuId);
            if (currentMenu == null) return Page();

            // ------------------------------------------------------------------
            // 🔥 CHỐT CHẶN CUỐI CÙNG: ÉP BUỘC CATEGORY TỪ SERVER 
            // ------------------------------------------------------------------
            // Bất chấp HTML gửi lên cái gì, gọi hàm dò tìm Menu Gốc và đè lại dữ liệu
            ContentPage.Category = await GetRootCategoryNameAsync(TargetMenuId);

            // LƯU BÀI VIẾT VÀO DATABASE (Lúc này Category đã bị ép chuẩn 100%)
            _context.ContentPages.Add(ContentPage);
            await _context.SaveChangesAsync();
            // Lưu xong, ContentPage.Id sẽ có số mới

            // NỐI TƠ HỒNG: Gắn Bài Viết vào Menu
            currentMenu.ContentPageId = ContentPage.Id;
            _context.NavigationMenus.Update(currentMenu);
            await _context.SaveChangesAsync();

            // ĐI TIẾP BƯỚC 3 (Mang theo ID của Bài viết)
            return RedirectToPage("./Step3_Sidebar", new { id = ContentPage.Id });
        }
    }
}