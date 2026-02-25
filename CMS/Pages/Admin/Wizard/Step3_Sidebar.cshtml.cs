using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

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

        // --- Các property dùng cho Preview UI ---
        public string PreviewTitle { get; set; } = "";
        public string PreviewContent { get; set; } = "";
        public string PreviewCategory { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("./Step1_Menu");

            // Tối ưu: Dùng AsNoTracking vì chỉ để lấy dữ liệu Preview lên UI
            var page = await _context.ContentPages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null) return NotFound();

            ContentPageId = page.Id;
            HasSidebar = page.HasSidebar;

            // Đổ dữ liệu thật từ DB vào biến Preview
            PreviewTitle = page.Title;
            PreviewContent = page.Content; // Ghi chú: Nếu content dài, trên UI nhớ cắt chuỗi (Substring)
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
            if (!ModelState.IsValid)
            {
                // Nếu có lỗi binding, cần xử lý load lại Preview ở đây (tuy nhiên form này đơn giản nên ít xảy ra lỗi)
                return Page();
            }

            // Tracking bật để thực hiện Update
            var pageToUpdate = await _context.ContentPages.FindAsync(ContentPageId);
            if (pageToUpdate == null) return NotFound();

            pageToUpdate.HasSidebar = HasSidebar;

            // 🔥 TỐI ƯU: Chỉ Select đúng cột Id từ database thay vì lấy cả dòng Menu
            var finalMenuId = await _context.NavigationMenus
                .Where(m => m.ContentPageId == pageToUpdate.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

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

                        // 🔥 NÚT THẮT QUAN TRỌNG: Gắn chặt ID vào để hết bị liệt đếm!
                        MenuId = finalMenuId > 0 ? finalMenuId : null
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Chuyển sang trang Success mượt mà
            return RedirectToPage("./WizardSuccess", new { id = finalMenuId });
        }
    }
}