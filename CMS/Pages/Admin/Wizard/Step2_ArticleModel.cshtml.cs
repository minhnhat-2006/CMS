using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

            var currentMenu = await _context.NavigationMenus
                .AsNoTracking() // Dùng AsNoTracking ở Get vì chỉ để hiển thị
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (currentMenu == null) return RedirectToPage("./Step1_Menu");

            TargetMenuId = currentMenu.Id;
            MenuName = currentMenu.Name;

            // Khởi tạo các giá trị mặc định cho form
            ContentPage.IsVisible = true;
            ContentPage.HasSidebar = true;

            // ĐÃ SỬA: Lấy trực tiếp tên của Menu hiện tại làm Category 
            // (Không dùng hàm GetRootCategoryNameAsync nữa để tránh nhận diện nhầm thành menu Cha)
            ContentPage.Category = currentMenu.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("ContentPage.SidebarItems");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Phải dùng tracking ở đây để EF theo dõi sự thay đổi
            var currentMenu = await _context.NavigationMenus.FindAsync(TargetMenuId);
            if (currentMenu == null) return Page();

            // ĐÃ SỬA: Chốt chặn cuối cùng: Ép buộc Category là tên của Menu hiện tại từ server
            ContentPage.Category = currentMenu.Name;

            // ==================================================================
            // 🔥 ĐIỂM CHỐT HẠ: Truyền ChuyenMucId từ Menu sang thẳng Bài viết
            // ==================================================================
            ContentPage.ChuyenMucId = currentMenu.ChuyenMucId;

            // ==================================================================
            // 🔥 TRANSACTION: Đảm bảo "Sống cùng sống, chết cùng chết"
            // ==================================================================
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu bài viết (Lúc này bài viết đã có sẵn ChuyenMucId không bao giờ null)
                _context.ContentPages.Add(ContentPage);
                await _context.SaveChangesAsync(); // Sau dòng này, ContentPage.Id có giá trị thực

                // 2. NỐI TƠ HỒNG: Gắn Bài Viết vào Menu
                currentMenu.ContentPageId = ContentPage.Id;
                // Không cần _context.NavigationMenus.Update() vì currentMenu đang được tracking
                await _context.SaveChangesAsync();

                // 3. Chốt giao dịch thành công
                await transaction.CommitAsync();

                return RedirectToPage("./Step3_Sidebar", new { id = ContentPage.Id });
            }
            catch
            {
                // Nếu có bất kỳ lỗi gì xảy ra, Rollback lại toàn bộ, không tạo ra dữ liệu rác
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.");
                return Page();
            }
        }
    }
}