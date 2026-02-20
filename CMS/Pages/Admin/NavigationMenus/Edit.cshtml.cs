using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages.NavigationMenus
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NavigationMenu NavigationMenu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Tìm Menu theo ID
            var navigationmenu = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.Id == id);

            if (navigationmenu == null)
            {
                return NotFound();
            }

            NavigationMenu = navigationmenu;

            // 2. Load các Dropdown dữ liệu
            LoadDropdowns(NavigationMenu.Id);

            return Page();
        }

        // Hàm hỗ trợ load dữ liệu cho các thẻ <select>
        private void LoadDropdowns(int currentId)
        {
            // a. Dropdown Menu Cha
            // QUAN TRỌNG: Loại bỏ chính menu đang sửa (Where m.Id != currentId) 
            // để tránh lỗi chọn chính mình làm cha.
            ViewData["ParentId"] = new SelectList(_context.NavigationMenus.Where(m => m.Id != currentId), "Id", "Name");

            // b. Dropdown Liên kết Bài viết (ContentPageId)
            // Lấy danh sách bài viết để gán quan hệ trực tiếp (nếu có dùng tính năng này)
            ViewData["PageList"] = new SelectList(_context.ContentPages, "Id", "Title");

            // c. Dropdown Chọn nhanh URL (Giống trang Create)
            var availablePages = _context.ContentPages
                .Where(p => p.IsVisible)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SelectListItem
                {
                    Text = p.Title,
                    Value = $"/bai-viet/{p.Id}/{p.Slug}"
                })
                .ToList();
            ViewData["AvailablePages"] = availablePages;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, phải load lại dropdown để không bị trắng trang
                LoadDropdowns(NavigationMenu.Id);
                return Page();
            }

            // 3. Xử lý Logic cập nhật
            // Attach báo cho EF Core biết đây là object cũ cần update
            _context.Attach(NavigationMenu).State = EntityState.Modified;

            try
            {
                // Logic bổ sung (Tùy chọn):
                // Nếu người dùng đã chọn "Liên kết bài viết" (ContentPageId có giá trị)
                // Thì ta có thể xóa trường Url đi để ưu tiên dùng ID bài viết
                if (NavigationMenu.ContentPageId != null)
                {
                    // NavigationMenu.Url = null; // Bỏ comment dòng này nếu muốn ưu tiên ID hơn Url cứng
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NavigationMenuExists(NavigationMenu.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool NavigationMenuExists(int id)
        {
            return _context.NavigationMenus.Any(e => e.Id == id);
        }
    }
}