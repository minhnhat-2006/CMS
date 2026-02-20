using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages
{
    public class PageDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PageDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ContentPage ContentPage { get; set; } = default!;

        // QUAN TRỌNG: Nhận vào cả ID (số) và Slug (chữ)
        public async Task<IActionResult> OnGetAsync(int id, string slug)
        {
            // 1. TÌM BÀI VIẾT THEO ID (Đây là mỏ neo bất tử)
            // Dù đổi tên bài viết 100 lần, ID vẫn là số cũ => Tìm ra ngay
            var page = await _context.ContentPages
                .FirstOrDefaultAsync(m => m.Id == id && m.IsVisible);

            if (page == null)
            {
                return NotFound();
            }

            // 2. KIỂM TRA SLUG (SEO & Redirect)
            // Nếu Slug trên URL (slug cũ) KHÁC với Slug trong Database (slug mới)
            // Ví dụ: Link cũ là "123-tin-cu", nhưng DB đã đổi thành "tin-moi"
            if (!string.Equals(slug, page.Slug, StringComparison.OrdinalIgnoreCase))
            {
                // 3. Tự động chuyển hướng 301 sang Link đúng
                // Trình duyệt tự đổi URL thành "123-tin-moi"
                return RedirectToPagePermanent("/PageDetail", new { id = page.Id, slug = page.Slug });
            }

            ContentPage = page;
            return Page();
        }
    }
}