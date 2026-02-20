using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Models;

namespace CMS.Pages.Sidebar
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SidebarItem SidebarItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Lấy Sidebar cần sửa từ Database
            // Include thêm Menu để nếu cần hiển thị tên Menu cũ (tùy chọn)
            var sidebar = await _context.SidebarItems
                .Include(s => s.Menu)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sidebar == null) return NotFound();

            SidebarItem = sidebar;

            // Load dữ liệu cho Dropdown (Đã sửa sang logic ID)
            await LoadDropdowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return Page();
            }

            // --- LOGIC ƯU TIÊN: Giữ nguyên logic hay của bạn ---
            // Nếu chọn bài viết nội bộ -> Xóa link ngoài
            if (SidebarItem.ContentPageId.HasValue)
            {
                SidebarItem.LinkUrl = null;
            }

            // --- CẬP NHẬT DỮ LIỆU ---
            // Cách dùng Attach này nhanh, nhưng yêu cầu SidebarItem phải có đủ ID
            _context.Attach(SidebarItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SidebarItemExists(SidebarItem.Id)) return NotFound();
                else throw;
            }

            return RedirectToPage("./Index");
        }

        private bool SidebarItemExists(int id)
        {
            return _context.SidebarItems.Any(e => e.Id == id);
        }

        // --- [QUAN TRỌNG] HÀM NÀY ĐÃ ĐƯỢC SỬA LẠI ---
        private async Task LoadDropdowns()
        {
            // 1. Load Menu theo ID (Thay vì cắt chuỗi Url)
            var menus = await _context.NavigationMenus
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name }) // Lấy gọn nhẹ
                .ToListAsync();

            // Lưu vào ViewData["MenuId"] để khớp với giao diện Edit.cshtml
            // Chọn giá trị mặc định là MenuId hiện tại của SidebarItem
            ViewData["MenuId"] = new SelectList(menus, "Id", "Name", SidebarItem?.MenuId);

            // 2. Load danh sách bài viết (Giữ nguyên)
            ViewData["PageList"] = new SelectList(await _context.ContentPages.ToListAsync(), "Id", "Title");
        }
    }
}