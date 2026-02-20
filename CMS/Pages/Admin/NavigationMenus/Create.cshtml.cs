using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages.NavigationMenus
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NavigationMenu NavigationMenu { get; set; } = default!;

        public IActionResult OnGet()
        {
            // 1. TẠO DROPDOWN MENU CHA
            // Lấy danh sách các menu hiện có để làm cha (nếu muốn tạo menu con)
            ViewData["ParentId"] = new SelectList(_context.NavigationMenus, "Id", "Name");

            // 2. TẠO DROPDOWN CHỌN NHANH TRANG (AvailablePages)
            // Lấy danh sách bài viết để người dùng chọn nhanh link mà không cần gõ thủ công
            // Value sẽ được format theo chuẩn: /{id}/{slug} mà ta đã cấu hình
            var availablePages = _context.ContentPages
                .Where(p => p.IsVisible) // Chỉ lấy bài đang hiện
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SelectListItem
                {
                    Text = p.Title,             // Hiển thị tên bài
                    Value = $"/bai-viet/{p.Id}/{p.Slug}"
                })
                .ToList();

            ViewData["AvailablePages"] = availablePages;

            // Khởi tạo giá trị mặc định (nếu cần)
            NavigationMenu = new NavigationMenu
            {
                IsVisible = true,
                DisplayOrder = 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi validate, phải load lại dữ liệu cho 2 cái dropdown
                // Nếu không load lại, trang web sẽ bị lỗi null khi hiển thị lại
                ViewData["ParentId"] = new SelectList(_context.NavigationMenus, "Id", "Name");

                var availablePages = _context.ContentPages
                    .Where(p => p.IsVisible)
                    .Select(p => new SelectListItem
                    {
                        Text = p.Title,
                        Value = $"/{p.Id}/{p.Slug}"
                    })
                    .ToList();
                ViewData["AvailablePages"] = availablePages;

                return Page();
            }

            _context.NavigationMenus.Add(NavigationMenu);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}