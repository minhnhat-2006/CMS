using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Trang // Hoặc namespace tương ứng của bạn
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hứng tên Category từ URL (thay vì ChuyenMucId)
        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        public IList<ContentPage> ContentPageList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var query = _context.ContentPages.AsQueryable();

            // Lọc chính xác theo cột Category trong DB
            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(c => c.Category == Category);
            }

            ContentPageList = await query.ToListAsync();
        }
    }
}