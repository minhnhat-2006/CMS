using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages
{
    public class CategoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CategoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string CategoryName { get; set; } = string.Empty;
        public List<ContentPage> Articles { get; set; } = new List<ContentPage>();
        public List<NavigationMenu> SidebarMenus { get; set; } = new List<NavigationMenu>();
        public NavigationMenu? ParentMenu { get; set; }

        // BỔ SUNG 1: Biến IsHub để báo cho Frontend biết "Tôi là Cha hay Con"
        public bool IsHub { get; set; } = false;

        // ĐÃ SỬA: Thêm dấu ? vào category để cho phép URL 1 tham số (Menu cha)
        public async Task<IActionResult> OnGetAsync(string section, string? category)
        {
            // ĐÃ SỬA: Chặn cửa thông minh hơn. Chỉ cần thiếu section mới đuổi về Index.
            // Nếu thiếu category (bấm Menu cha), vẫn cho đi tiếp!
            if (string.IsNullOrEmpty(section))
            {
                return RedirectToPage("/Index");
            }

            // 1. LẤY ĐƯỜNG DẪN ĐỘNG HIỆN TẠI (Ví dụ: "/tin-tuc" hoặc "/tin-tuc/san-pham")
            // Dùng cái này làm "chứng minh thư" để dò trong DB là tuyệt đối chính xác
            string currentPath = Request.Path.Value?.ToLower() ?? string.Empty;

            // 2. TÌM MENU HOÀN TOÀN ĐỘNG THEO URL CHUẨN ĐÃ LƯU Ở BƯỚC 1
            var currentMenu = await _context.NavigationMenus
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Url != null && m.Url.ToLower() == currentPath);

            // Fallback (Dự phòng): Lỡ như URL có sai sót nhỏ (dư dấu /), tìm bằng Slug cuối cùng
            if (currentMenu == null)
            {
                string targetSlug = string.IsNullOrEmpty(category) ? section : category;
                currentMenu = await _context.NavigationMenus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Url != null && m.Url.EndsWith(targetSlug));

                // Trạm thu phí cuối cùng: Vẫn không thấy thì mới báo 404
                if (currentMenu == null) return NotFound();
            }

            // Xử lý tìm Parent Menu
            if (currentMenu.ParentId.HasValue)
            {
                ParentMenu = await _context.NavigationMenus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == currentMenu.ParentId);
            }

            // 3. GÁN TÊN VÀ XÁC ĐỊNH HUB HAY CON
            CategoryName = currentMenu.Name;

            // Một menu được coi là Hub nếu nó không có ParentId (Nó là menu gốc)
            IsHub = currentMenu.ParentId == null;

            int rootId = currentMenu.ParentId ?? currentMenu.Id;

            // 4. LẤY MENU SIDEBAR
            SidebarMenus = await _context.NavigationMenus
                .AsNoTracking()
                .Where(m => m.ParentId == rootId)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            // 5. GOM BÀI VIẾT THÔNG MINH
            List<string> categoryNamesToFetch = new List<string> { currentMenu.Name };

            // Nếu là trang Hub, lấy bài viết của chính nó và TẤT CẢ các mục con
            if (IsHub && SidebarMenus.Any())
            {
                categoryNamesToFetch.AddRange(SidebarMenus.Select(m => m.Name));
            }

            Articles = await _context.ContentPages
                .AsNoTracking()
                .Where(p => categoryNamesToFetch.Contains(p.Category) && p.IsVisible)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Page();
        }
    }
}