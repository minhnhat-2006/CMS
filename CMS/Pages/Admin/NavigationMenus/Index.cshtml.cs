using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CMS.Models;
using CMS.Data; // <--- THÊM DÒNG QUAN TRỌNG NÀY (Để hết lỗi đỏ ApplicationDbContext)

namespace CMS.Pages.NavigationMenus
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<NavigationMenu> NavigationMenus { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.NavigationMenus != null)
            {
                NavigationMenus = await _context.NavigationMenus.ToListAsync();
            }
        }
    }
}