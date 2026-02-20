using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CMS.Pages.Trang
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPageItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();
            var pageItem = await _context.ContentPages.FirstOrDefaultAsync(m => m.Id == id);
            if (pageItem == null) return NotFound();
            ContentPageItem = pageItem;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();
            var pageItem = await _context.ContentPages.FindAsync(id);
            if (pageItem != null)
            {
                ContentPageItem = pageItem;
                _context.ContentPages.Remove(ContentPageItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}