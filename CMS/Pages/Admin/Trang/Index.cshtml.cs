using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Trang
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Layout { get; set; } // Giữ tham số layout=popup

        public IList<ContentPage> ContentPageList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // 1. Lấy danh sách Chuyên mục (từ NavigationMenus) để làm Dropdown
            var categories = await _context.NavigationMenus
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .Select(m => m.Name)
                .Distinct()
                .ToListAsync();

            ViewData["CategoryList"] = new SelectList(categories, Category);

            // 2. Query bài viết
            var query = _context.ContentPages.AsQueryable();

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(c => c.Category == Category);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                // Tìm theo tiêu đề hoặc nội dung
                query = query.Where(c => c.Title.Contains(SearchTerm) || c.Content.Contains(SearchTerm));
            }

            // Sắp xếp bài mới nhất lên đầu
            ContentPageList = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }
    }
}