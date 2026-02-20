using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Models;

namespace CMS.Pages.Sidebar
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SidebarItem SidebarItem { get; set; } = default!;

        // 1. Hiện thông tin để người dùng xác nhận trước khi xóa
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var sidebar = await _context.SidebarItems.FirstOrDefaultAsync(m => m.Id == id);

            if (sidebar == null) return NotFound();

            SidebarItem = sidebar;
            return Page();
        }

        // 2. Thực hiện xóa khi bấm nút xác nhận
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var sidebar = await _context.SidebarItems.FindAsync(id);

            if (sidebar != null)
            {
                SidebarItem = sidebar; // Gán lại để nếu lỗi còn biết
                _context.SidebarItems.Remove(SidebarItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}