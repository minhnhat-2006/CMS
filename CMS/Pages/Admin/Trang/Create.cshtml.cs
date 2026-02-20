using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cái này
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages.Trang
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPageItem { get; set; } = default!;

        public IActionResult OnGet()
        {
            // Lấy các Menu Cha, đang hiện
            var menus = _context.NavigationMenus
                .Where(x => x.ParentId == null && x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .ToList(); // Lấy về danh sách trước

            // Tạo danh sách chuyên mục để hiển thị
            var categoryList = menus.Select(x => new
            {
                Name = x.Name,
                // Nếu Url có giá trị thì dùng Url, nếu không thì dùng chính cái Name làm Slug tạm
                Value = !string.IsNullOrEmpty(x.Url)
                        ? x.Url.Replace("/", "").ToLower().Trim()
                        : x.Name.ToLower().Trim() // Hoặc logic gì đó bạn muốn lưu vào cột Category
            }).ToList();

            ViewData["CategoryList"] = new SelectList(categoryList, "Value", "Name");

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // QUAN TRỌNG: Nếu lỗi (quên nhập tiêu đề...), phải Load lại Menu
                // Nếu không Dropdown sẽ bị trống trơn
                return OnGet();
            }

            // Tự động tạo Slug nếu chưa có
            if (string.IsNullOrEmpty(ContentPageItem.Slug))
            {
                // (Logic tạo slug của bạn ở đây nếu có)
            }

            _context.ContentPages.Add(ContentPageItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}