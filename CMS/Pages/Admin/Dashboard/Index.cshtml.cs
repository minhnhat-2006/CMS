using CMS.Data;
// Gọi namespace của thư mục Models và Data trong dự án CMS của bạn
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<NavigationMenu> RecentMenus { get; set; } = new List<NavigationMenu>();

        public async Task<IActionResult> OnPostFixDataAsync()
        {
            // 1. Lệnh vá ContentPageId: Nối Menu với Bài viết dựa trên tên Category
            // Dùng ILIKE để "giới thiệu" khớp với "Giới Thiệu"
            var fixMenuSql = @"
        UPDATE ""NavigationMenus"" nm
        SET ""ContentPageId"" = cp.""Id""
        FROM ""ContentPages"" cp
        WHERE TRIM(nm.""Name"") ILIKE TRIM(cp.""Category"")
          AND (nm.""ContentPageId"" IS NULL OR nm.""ContentPageId"" = 0)";

            // 2. Lệnh vá MenuId cho Sidebar: Nối Sidebar với Menu dựa trên tên
            var fixSidebarSql = @"
        UPDATE ""SidebarItems"" si
        SET ""MenuId"" = nm.""Id""
        FROM ""NavigationMenus"" nm
        WHERE si.""Title"" ILIKE '%' || nm.""Name"" || '%'
          AND si.""MenuId"" IS NULL";

            // Thực thi trực tiếp xuống PostgreSQL
            await _context.Database.ExecuteSqlRawAsync(fixMenuSql);
            await _context.Database.ExecuteSqlRawAsync(fixSidebarSql);

            TempData["Message"] = "Hệ thống đã tự động kết nối các dữ liệu bị NULL thành công!";
            return RedirectToPage();
        }

        public async Task OnGetAsync()
        {
            var syncSql = @"
        UPDATE ""NavigationMenus"" nm
        SET ""ContentPageId"" = cp.""Id""
        FROM ""ContentPages"" cp
        WHERE TRIM(nm.""Name"") ILIKE TRIM(cp.""Category"")
          AND (nm.""ContentPageId"" IS NULL OR nm.""ContentPageId"" = 0);";

            await _context.Database.ExecuteSqlRawAsync(syncSql);
            if (_context.NavigationMenus != null)
            {
                // 1. CHỈ LẤY MENU GỐC (Cha)
                var rootMenus = await _context.NavigationMenus
                    .Where(m => m.ParentId == null || m.ParentId == 0) // Tối ưu: Bắt cả null và 0
                    .OrderByDescending(m => m.Id)
                    .Take(6)
                    .ToListAsync();

                foreach (var menu in rootMenus)
                {
                    // 1. LẤY TẤT CẢ MENU CON (Lấy cả ID và Name để quét)
                    var childMenus = await _context.NavigationMenus
                        .Where(c => c.ParentId == menu.Id)
                        .ToListAsync();

                    // 2. GOM TÊN (ĐỂ ĐẾM BÀI) VÀ GOM ID (ĐỂ ĐẾM SIDEBAR)
                    // Dùng Trim() để gọt sạch dấu cách thừa 2 đầu đề phòng gõ nhầm
                    var allMenuNames = new List<string> { menu.Name.Trim() };
                    allMenuNames.AddRange(childMenus.Select(c => c.Name.Trim()));

                    var allMenuIds = new List<int> { menu.Id };
                    allMenuIds.AddRange(childMenus.Select(c => c.Id));

                    // 3. ĐẾM BÀI VIẾT: Bắt chính xác Category
                    // Lưu ý: Nếu vẫn ra 0, bạn phải check lại trong DB xem chữ viết Hoa/Thường có khớp nhau 100% không!
                    menu.PostCount = await _context.ContentPages
                        .CountAsync(p => allMenuNames.Contains(p.Category.Trim()));

                    // 4. ĐẾM SIDEBAR: Quét cả dòng họ (Cha + Các Con)
                    // Chỉ cần 1 thằng trong dòng họ có Sidebar là đếm 1
                    var hasSidebar = await _context.SidebarItems
                        .AnyAsync(s => s.MenuId != null && allMenuIds.Contains(s.MenuId.Value));

                    menu.SidebarCount = hasSidebar ? 1 : 0;
                }
                RecentMenus = rootMenus;
            }
        }
    }
}