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

        // 👉 ĐÂY LÀ BIẾN CÒN THIẾU ĐỂ TRANG GIAO DIỆN KHÔNG BỊ LỖI
        public int? ParentId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("Step1_Menu");
            CreatedMenuId = id;

            var menu = await _context.NavigationMenus.Include(m => m.LinkedPage).FirstOrDefaultAsync(m => m.Id == id);
            if (menu != null)
            {
                CreatedMenuName = menu.Name ?? "Chưa đặt tên";

                if (menu.ParentId != null)
                {
                    var parent = await _context.NavigationMenus.FindAsync(menu.ParentId);
                    ParentName = parent?.Name;

                    // Lấy ID cha gán vào biến
                    ParentId = menu.ParentId;

                    TotalPosts = await _context.NavigationMenus.CountAsync(m => m.ParentId == menu.ParentId);
                }
                else
                {
                    ParentName = null;

                    // Menu gốc thì không có cha
                    ParentId = null;

                    TotalPosts = await _context.NavigationMenus.CountAsync(m => m.ParentId == null);
                }
                Slug = menu.LinkedPage?.Slug ?? "#";
            }
            return Page();
        }
    }
}