using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.Wizard
{
    public class Step3_SidebarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public Step3_SidebarModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public int ContentPageId { get; set; }

        [BindProperty]
        public bool HasSidebar { get; set; }

        // MỚI THÊM: 2 Biến để đẩy dữ liệu ra bản Preview ngoài View
        public string PreviewTitle { get; set; }
        public string PreviewContent { get; set; }
        public string PreviewCategory { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("./Step1_Menu");
            var page = await _context.ContentPages.FindAsync(id);
            if (page == null) return NotFound();

            ContentPageId = page.Id;
            HasSidebar = page.HasSidebar;

            // MỚI THÊM: Gán dữ liệu thật từ DB vào biến Preview
            PreviewTitle = page.Title;
            PreviewContent = page.Content;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var pageToUpdate = await _context.ContentPages.FindAsync(ContentPageId);
            if (pageToUpdate == null) return NotFound();

            pageToUpdate.HasSidebar = HasSidebar;

            if (HasSidebar)
            {
                bool hasItem = await _context.SidebarItems.AnyAsync(s => s.Category == pageToUpdate.Category);
                if (!hasItem)
                {
                    _context.SidebarItems.Add(new SidebarItem
                    {
                        Title = pageToUpdate.Category,
                        Content = "<p>Nội dung liên quan</p>",
                        IsVisible = true,
                        Category = pageToUpdate.Category,
                        ContentPageId = pageToUpdate.Id
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Truy tìm Menu chứa bài viết này để ném ra trang Success
            var linkedMenu = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.ContentPageId == pageToUpdate.Id);
            int finalMenuId = linkedMenu != null ? linkedMenu.Id : 0;

            return RedirectToPage("./WizardSuccess", new { id = finalMenuId });
        }
    }
}