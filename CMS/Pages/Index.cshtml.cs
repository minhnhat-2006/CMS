using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CMS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách chứa bài viết để đưa ra giao diện
        public IList<ContentPage> LatestNews { get; set; } = default!;

        public async Task OnGetAsync()
        {
            LatestNews = await _context.ContentPages
                .Where(p => p.IsVisible)
                .OrderByDescending(p => p.Id) // <--- SỬA THÀNH Id (Thay vì CreatedDate)
                .Take(6)
                .ToListAsync();
        }
    }
}