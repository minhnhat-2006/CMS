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
        // Ghi chú: Nếu file DbContext của bạn tên khác, hãy đổi chữ ApplicationDbContext thành tên của bạn
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ĐÂY CHÍNH LÀ KHÚC GIẢI QUYẾT LỖI ĐỎ "RecentMenus"
        // Ghi chú: Đổi chữ NavigationMenu thành tên class Menu tương ứng trong thư mục Models của bạn
        public List<NavigationMenu> RecentMenus { get; set; } = new List<NavigationMenu>();

        public async Task OnGetAsync()
        {
            // Truy vấn lấy 4 Menu vừa tạo mới nhất từ DB
            if (_context.NavigationMenus != null)
            {
                RecentMenus = await _context.NavigationMenus
                    .OrderByDescending(m => m.Id)
                    .Take(4)
                    .ToListAsync();
            }
        }
    }
}