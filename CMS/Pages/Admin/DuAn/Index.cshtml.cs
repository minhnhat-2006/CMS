using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.DuAn // 1. CẬP NHẬT NAMESPACE MỚI
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Khai báo danh sách để hiển thị ra HTML
        public List<NavigationMenu> RecentMenus { get; set; } = new List<NavigationMenu>();

        // Giữ nguyên hàm OnPost để bạn có thể gọi lệnh vá dữ liệu từ trang Dự án
        public async Task<IActionResult> OnPostFixDataAsync()
        {
            var fixMenuSql = @"
                UPDATE ""NavigationMenus"" nm
                SET ""ContentPageId"" = cp.""Id""
                FROM ""ContentPages"" cp
                WHERE TRIM(nm.""Name"") ILIKE TRIM(cp.""Category"")
                  AND (nm.""ContentPageId"" IS NULL OR nm.""ContentPageId"" = 0)";

            var fixSidebarSql = @"
                UPDATE ""SidebarItems"" si
                SET ""MenuId"" = nm.""Id""
                FROM ""NavigationMenus"" nm
                WHERE si.""Title"" ILIKE '%' || nm.""Name"" || '%'
                  AND si.""MenuId"" IS NULL";

            await _context.Database.ExecuteSqlRawAsync(fixMenuSql);
            await _context.Database.ExecuteSqlRawAsync(fixSidebarSql);

            TempData["Message"] = "Hệ thống đã tự động kết nối dữ liệu thành công!";
            return RedirectToPage();
        }

        public async Task OnGetAsync()
        {
            // 2. TỰ ĐỘNG VÁ DỮ LIỆU MỖI KHI VÀO TRANG DỰ ÁN
            var syncSql = @"
                UPDATE ""NavigationMenus"" nm
                SET ""ContentPageId"" = cp.""Id""
                FROM ""ContentPages"" cp
                WHERE TRIM(nm.""Name"") ILIKE TRIM(cp.""Category"")
                  AND (nm.""ContentPageId"" IS NULL OR nm.""ContentPageId"" = 0);";

            await _context.Database.ExecuteSqlRawAsync(syncSql);

            if (_context.NavigationMenus != null)
            {
                // 3. LẤY CÁC DỰ ÁN GỐC (CHA)
                var rootMenus = await _context.NavigationMenus
                    .Where(m => m.ParentId == null || m.ParentId == 0)
                    .OrderByDescending(m => m.Id)
                    .ToListAsync();

                foreach (var menu in rootMenus)
                {
                    // Lấy các menu con để tính tổng bài viết của cả luồng
                    var childMenus = await _context.NavigationMenus
                        .Where(c => c.ParentId == menu.Id)
                        .ToListAsync();

                    var allMenuNames = new List<string> { menu.Name.Trim() };
                    allMenuNames.AddRange(childMenus.Select(c => c.Name.Trim()));

                    var allMenuIds = new List<int> { menu.Id };
                    allMenuIds.AddRange(childMenus.Select(c => c.Id));

                    // 4. ĐẾM BÀI VIẾT THEO CATEGORY
                    menu.PostCount = await _context.ContentPages
                        .CountAsync(p => allMenuNames.Contains(p.Category.Trim()));

                    // 5. KIỂM TRA SIDEBAR
                    var hasSidebar = await _context.SidebarItems
                        .AnyAsync(s => s.MenuId != null && allMenuIds.Contains(s.MenuId.Value));

                    menu.SidebarCount = hasSidebar ? 1 : 0;
                }
                RecentMenus = rootMenus;
            }
        }
    }
}