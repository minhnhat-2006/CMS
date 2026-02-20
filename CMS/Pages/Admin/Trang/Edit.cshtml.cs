using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages.Trang
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPageItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // 1. Lấy bài viết cần sửa
            var contentPage = await _context.ContentPages.FirstOrDefaultAsync(m => m.Id == id);
            if (contentPage == null) return NotFound();

            ContentPageItem = contentPage;

            // 2. [QUAN TRỌNG] Nạp dữ liệu cho Dropdown Chuyên mục
            // Logic này phải giống hệt lúc Tạo mới (Create)

            var menus = await _context.NavigationMenus
                .Where(x => x.ParentId == null && x.IsVisible) // Lấy Menu cha
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            var categoryList = menus.Select(x => new
            {
                Name = x.Name,
                // Value phải là cái chuỗi bạn muốn lưu vào DB (ví dụ: "tin-tuc")
                // Nếu dùng Link động, có thể bạn muốn lưu Slug hoặc Tên không dấu
                Value = !string.IsNullOrEmpty(x.Url)
                        ? x.Url.Replace("/", "").ToLower().Trim()
                        : x.Name.ToLower().Trim() // Fallback nếu không có Url
            }).ToList();

            // Chọn sẵn giá trị hiện tại của bài viết
            ViewData["CategoryList"] = new SelectList(categoryList, "Value", "Name", ContentPageItem.Category);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, phải nạp lại Dropdown y hệt như OnGet
                // (Copy đoạn logic lấy Menu xuống đây)
                var menus = await _context.NavigationMenus.Where(x => x.ParentId == null && x.IsVisible).ToListAsync();
                var categoryList = menus.Select(x => new {
                    Name = x.Name,
                    Value = !string.IsNullOrEmpty(x.Url) ? x.Url.Replace("/", "").ToLower().Trim() : x.Name.ToLower().Trim()
                }).ToList();
                ViewData["CategoryList"] = new SelectList(categoryList, "Value", "Name", ContentPageItem.Category);

                return Page();
            }

            // Đánh dấu bản ghi đã bị thay đổi
            _context.Attach(ContentPageItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ContentPages.Any(e => e.Id == ContentPageItem.Id)) return NotFound();
                else throw;
            }

            return RedirectToPage("./Index");
        }
    }
}