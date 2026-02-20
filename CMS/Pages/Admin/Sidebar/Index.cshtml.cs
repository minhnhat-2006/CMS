using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages.Sidebar
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách Sidebar hiển thị ra bảng
        public IList<SidebarItem> SidebarList { get; set; } = default!;

        // Đối tượng dùng để tạo mới trong Popup
        [BindProperty]
        public SidebarItem NewSidebar { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // 1. Lấy danh sách Sidebar (Kèm thông tin Menu và Bài viết)
            SidebarList = await _context.SidebarItems
                .Include(s => s.Menu)
                .Include(s => s.ContentPage)
                .OrderBy(s => s.MenuId) // Gom nhóm theo Menu cho dễ nhìn
                .ThenBy(s => s.SortOrder) // Sau đó sắp xếp theo thứ tự
                .ToListAsync();

            // 2. [QUAN TRỌNG] Lấy danh sách Menu để nạp vào Dropdown
            var menus = await _context.NavigationMenus
                .Where(m => m.IsVisible)
                .OrderBy(m => m.Name)
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            ViewData["MenuList"] = new SelectList(menus, "Id", "Name");

            // 3. Lấy danh sách Bài viết để nạp vào Dropdown
            var pages = await _context.ContentPages
                .Where(p => p.IsVisible)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            ViewData["PageList"] = new SelectList(pages, "Id", "Title");
        }

        // Hàm xử lý khi bấm nút "Lưu" trong Popup
        public async Task<IActionResult> OnPostCreateQuickAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi thì load lại trang (để đơn giản)
                return RedirectToPage("./Index");
            }

            _context.SidebarItems.Add(NewSidebar);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        // Hàm xử lý xóa nhanh
        public async Task<IActionResult> OnPostDeleteQuickAsync(int id)
        {
            var sidebar = await _context.SidebarItems.FindAsync(id);
            if (sidebar != null)
            {
                _context.SidebarItems.Remove(sidebar);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}