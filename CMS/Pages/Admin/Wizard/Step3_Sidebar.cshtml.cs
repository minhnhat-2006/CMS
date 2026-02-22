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

            if (HasSidebar)
            {
                // Kiểm tra lại lần nữa trước khi thêm mới
                bool hasItem = await _context.SidebarItems.AnyAsync(s => s.Category == pageToUpdate.Category);

                if (!hasItem)
                {
                    // CHƯA CÓ thì mới tạo (CHỈ TẠO 1 LẦN DUY NHẤT)
                    _context.SidebarItems.Add(new SidebarItem
                    {
                        Title = pageToUpdate.Category, // Có thể đổi thành "Danh mục liên quan" tùy ý bạn

                        // 🔥 CẬP NHẬT CHÍNH: Để trống hoàn toàn Content theo chuẩn "Cách 2"
                        Content = "",

                        IsVisible = true,
                        Category = pageToUpdate.Category,

                        // Ép thành NULL để biến Sidebar thành tài sản chung của Category
                        ContentPageId = null
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