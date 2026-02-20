using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Models;

namespace CMS.Pages.NavigationMenus
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NavigationMenu NavigationMenu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var navigationmenu = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.Id == id);

            if (navigationmenu == null) return NotFound();
            else NavigationMenu = navigationmenu;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Tìm cái Menu đang muốn xóa
            var navigationMenu = await _context.NavigationMenus.FindAsync(id);

            if (navigationMenu != null)
            {
                // --- BƯỚC QUAN TRỌNG: DỌN DẸP SIDEBAR (CON) TRƯỚC ---

                // Tìm tất cả Sidebar đang bám theo Menu này
                var sidebarsCon = _context.SidebarItems.Where(s => s.MenuId == id);

                // Nếu có thì xóa sạch
                if (sidebarsCon.Any())
                {
                    _context.SidebarItems.RemoveRange(sidebarsCon);
                }

                // -----------------------------------------------------

                // 2. Sau khi dọn con xong, giờ xóa thằng cha (Menu) an toàn
                _context.NavigationMenus.Remove(navigationMenu);

                // 3. Lưu lệnh xuống Database
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}