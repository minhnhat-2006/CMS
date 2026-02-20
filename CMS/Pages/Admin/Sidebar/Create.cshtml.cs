using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Models;

namespace CMS.Pages.Sidebar
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SidebarItem SidebarItem { get; set; } = default!;

        // --- HÀM LOAD DROPDOWN (Đã sửa logic sang ID) ---
        private async Task LoadDropdowns()
        {
            // 1. [MỚI] Lấy danh sách Menu để chọn ID
            // Không cần cắt chuỗi TrimStart('/') nữa, lấy thẳng ID và Name
            var menus = await _context.NavigationMenus
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name }) // Chỉ lấy 2 trường cần thiết cho nhẹ
                .ToListAsync();

            // Lưu vào ViewData["MenuId"] để khớp với giao diện Create.cshtml mới sửa
            ViewData["MenuId"] = new SelectList(menus, "Id", "Name");

            // 2. Load danh sách bài viết (Giữ nguyên)
            ViewData["PageList"] = new SelectList(await _context.ContentPages.ToListAsync(), "Id", "Title");
        }

        public async Task OnGetAsync()
        {
            await LoadDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Nếu form lỗi thì load lại dropdown và trả về trang
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return Page();
            }

            // Kiểm tra ContentPageId hợp lệ không (Giữ nguyên logic bảo vệ của bạn)
            if (SidebarItem.ContentPageId.HasValue)
            {
                bool exists = await _context.ContentPages.AnyAsync(x => x.Id == SidebarItem.ContentPageId);
                if (!exists)
                {
                    ModelState.AddModelError("", "Bài viết liên kết không tồn tại.");
                    await LoadDropdowns();
                    return Page();
                }
            }

            try
            {
                // [MỚI] Không cần gán Category thủ công nữa
                // SidebarItem.Category = ... (Bỏ dòng này)
                // SidebarItem.MenuId đã được tự động gán từ Dropdown nhờ [BindProperty]

                _context.SidebarItems.Add(SidebarItem);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Bắt lỗi SQL (Logic debug của bạn rất tốt, giữ nguyên)
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "❌ LỖI HỆ THỐNG: " + realError);

                await LoadDropdowns();
                return Page();
            }
        }
    }
}