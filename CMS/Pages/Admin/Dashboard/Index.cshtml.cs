using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Gọi namespace của thư mục Models và Data trong dự án CMS của bạn
using CMS.Models;
using CMS.Data;

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

        public async Task OnGetAsync()
        {
            if (_context.NavigationMenus != null)
            {
                // 1. CHỈ LẤY MENU GỐC (Tránh việc thẻ Card đẻ ra vô tội vạ khi tạo menu con)
                var rootMenus = await _context.NavigationMenus
                    .Where(m => m.ParentId == null) // Điều kiện then chốt: Bỏ qua các menu con
                    .OrderByDescending(m => m.Id)
                    .Take(6) // Nên để 6 (2 hàng) cho đẹp giao diện HTML của bạn
                    .ToListAsync();

                foreach (var menu in rootMenus)
                {
                    // 2. TÌM TẤT CẢ CÁC MENU CON THUỘC VỀ MENU NÀY
                    var childMenuNames = await _context.NavigationMenus
                        .Where(c => c.ParentId == menu.Id)
                        .Select(c => c.Name)
                        .ToListAsync();

                    // Gộp tên Menu cha và tên các Menu con vào chung 1 danh sách
                    var allCategories = new List<string> { menu.Name };
                    allCategories.AddRange(childMenuNames);

                    // ĐẾM BÀI VIẾT (Giữ nguyên logic gộp lúc nãy)
                    menu.PostCount = await _context.ContentPages
                        .CountAsync(p => allCategories.Contains(p.Category));

                    // SIDEBAR: Gộp thành 1. 
                    // Nếu Menu Cha có Sidebar rồi -> gán là 1 (Đã cấu hình). Nếu chưa -> 0.
                    // (Bạn điều chỉnh lại bảng Sidebar cho đúng tên trong DB của bạn nhé)
                    var hasSharedSidebar = await _context.SidebarItems
                        .AnyAsync(s => s.MenuId == menu.Id); // Chỉ tìm Sidebar gắn với ID của Menu gốc

                    menu.SidebarCount = hasSharedSidebar ? 1 : 0;
                }

                RecentMenus = rootMenus;
            }
        }
    }
}