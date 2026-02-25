using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.Wizard
{
    public class WizardSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public WizardSuccessModel(ApplicationDbContext context) => _context = context;

        public int CreatedMenuId { get; set; }
        public string CreatedMenuName { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string Slug { get; set; } = "#";
        public int TotalPosts { get; set; } = 0;

        // BIẾN ĐỂ TRANG GIAO DIỆN KHÔNG BỊ LỖI
        public int? ParentId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("Step1_Menu");
            CreatedMenuId = id;

            // ĐÃ SỬA: Include thêm ChuyenMuc để lấy Slug của dự án dạng Danh Mục
            var menu = await _context.NavigationMenus
                .Include(m => m.LinkedPage)
                .Include(m => m.ChuyenMuc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu != null)
            {
                CreatedMenuName = menu.Name ?? "Chưa đặt tên";

                if (menu.ParentId != null)
                {
                    var parent = await _context.NavigationMenus.FindAsync(menu.ParentId);
                    ParentName = parent?.Name;
                    ParentId = menu.ParentId;
                    TotalPosts = await _context.NavigationMenus.CountAsync(m => m.ParentId == menu.ParentId);
                }
                else
                {
                    ParentName = null;
                    ParentId = null;
                    TotalPosts = await _context.NavigationMenus.CountAsync(m => m.ParentId == null);
                }

                // ĐÃ SỬA: Logic lấy URL thông minh bao trọn các trường hợp
                if (menu.LinkedPage != null)
                {
                    Slug = $"bai-viet/{menu.LinkedPage.Id}/{menu.LinkedPage.Slug}";
                }
                else if (menu.ChuyenMuc != null)
                {
                    Slug = menu.ChuyenMuc.Slug;
                }
                else
                {
                    Slug = menu.Url ?? "#";
                }
            }
            return Page();
        }
    }
}