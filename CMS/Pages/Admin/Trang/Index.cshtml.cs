using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        public IList<ContentPage> ContentPageList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            ContentPageList = await _context.ContentPages.ToListAsync();
        }
    }
}