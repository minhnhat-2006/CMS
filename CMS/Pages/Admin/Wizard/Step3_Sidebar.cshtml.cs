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

        public string PreviewTitle { get; set; } = "";
        public string PreviewContent { get; set; } = "";
        public string PreviewCategory { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("./Step1_Menu");
            var page = await _context.ContentPages.FindAsync(id);
            if (page == null) return NotFound();

            ContentPageId = page.Id;
            HasSidebar = page.HasSidebar;

            // Đổ dữ liệu thật từ DB vào biến Preview
            PreviewTitle = page.Title;
            PreviewContent = page.Content;
            PreviewCategory = page.Category;

            // Kiểm tra xem Menu Gốc (Category) đã có Sidebar nào chưa?
            bool isSidebarExist = await _context.SidebarItems.AnyAsync(s => s.Category == page.Category);

            // Truyền cờ ra ngoài View để thay đổi giao diện 
            ViewData["IsShared"] = isSidebarExist;
            if (isSidebarExist)
            {
                ViewData["Message"] = $"Chuyên mục '{page.Category}' đã có sẵn cấu hình Sidebar dùng chung.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var pageToUpdate = await _context.ContentPages.FindAsync(ContentPageId);
            if (pageToUpdate == null) return NotFound();

            pageToUpdate.HasSidebar = HasSidebar;

            // 🔥 DỜI ĐOẠN TÌM MENU LÊN ĐÂY!
            // Truy tìm Menu gốc chứa bài viết này để lấy MenuId
            var linkedMenu = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.ContentPageId == pageToUpdate.Id);
            int finalMenuId = linkedMenu != null ? linkedMenu.Id : 0;

            if (HasSidebar)
            {
                bool hasItem = await _context.SidebarItems.AnyAsync(s => s.Category == pageToUpdate.Category);

                if (!hasItem)
                {
                    _context.SidebarItems.Add(new SidebarItem
                    {
                        Title = pageToUpdate.Category,
                        Content = "",
                        IsVisible = true,
                        Category = pageToUpdate.Category,
                        ContentPageId = null,

                        // 🔥 NÚT THẮT QUAN TRỌNG NHẤT LÀ ĐÂY:
                        MenuId = finalMenuId > 0 ? finalMenuId : null // Gắn chặt ID vào để hết bị liệt đếm!
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Chuyển sang trang Success mượt mà
            return RedirectToPage("./WizardSuccess", new { id = finalMenuId });
        }
    }
}