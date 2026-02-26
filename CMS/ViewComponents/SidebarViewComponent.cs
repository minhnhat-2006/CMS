using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? contentPageId = null)
        {
            // 1. Tự móc ID bài viết từ URL
            if (!contentPageId.HasValue)
            {
                var routeId = ViewContext.RouteData.Values["id"]?.ToString();
                if (int.TryParse(routeId, out int parsedId)) { contentPageId = parsedId; }
            }

            if (!contentPageId.HasValue) return View(new List<NavigationMenu>());

            // 2. Tìm Menu đang chứa bài viết này
            var currentMenu = await _context.NavigationMenus
                .FirstOrDefaultAsync(m => m.ContentPageId == contentPageId.Value);

            if (currentMenu == null) return View(new List<NavigationMenu>());

            // 3. Tìm thẳng lên Ông Tổ (Menu Cha)
            int parentId = currentMenu.ParentId ?? currentMenu.Id;
            var parentMenu = await _context.NavigationMenus
                .Include(m => m.LinkedPage)
                .FirstOrDefaultAsync(m => m.Id == parentId);

            if (parentMenu == null) return View(new List<NavigationMenu>());

            // 4. Lấy cái Vỏ Sidebar bằng MenuId (Bảng SidebarItems của bác lưu MenuId rất chuẩn)
            var sidebarConfig = await _context.SidebarItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MenuId == parentId && x.IsVisible);

            // 5. Gom bầy con (Bao gồm cả Cha)
            var allMenusForSidebar = new List<NavigationMenu>();
            if (parentMenu.IsVisible) allMenusForSidebar.Add(parentMenu);

            var siblingMenus = await _context.NavigationMenus
                .Include(m => m.LinkedPage)
                .Where(m => m.ParentId == parentId && m.IsVisible)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            allMenusForSidebar.AddRange(siblingMenus);

            ViewData["SidebarConfig"] = sidebarConfig;

            // TRẢ VỀ MENU CHỨ KHÔNG TRẢ VỀ BÀI VIẾT NỮA
            return View(allMenusForSidebar);
        }
    }
}