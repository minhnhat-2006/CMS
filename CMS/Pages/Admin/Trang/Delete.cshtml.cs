using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace CMS.Pages.Trang
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DeleteModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                // ✅ Xóa file thumbnail nếu có
                if (!string.IsNullOrEmpty(pageItem.Thumbnail))
                {
                    var filePath = Path.Combine(_env.WebRootPath, pageItem.Thumbnail.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                ContentPageItem = pageItem;
                _context.ContentPages.Remove(ContentPageItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}