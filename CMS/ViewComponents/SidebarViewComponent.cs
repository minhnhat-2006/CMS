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

        public async Task<IViewComponentResult> InvokeAsync(int? contentPageId = null, string? currentCategory = null)
        {
            SidebarItem sidebarConfig = null;
            var relatedArticles = new List<ContentPage>();

            // Tự động lấy ID từ URL nếu không truyền vào
            if (!contentPageId.HasValue)
            {
                var routeId = ViewContext.RouteData.Values["id"]?.ToString();
                if (int.TryParse(routeId, out int parsedId)) { contentPageId = parsedId; }
            }

            // ==========================================================
            // KỊCH BẢN 1: TRANG TĨNH (1-1) - Gom cả Cha lẫn Con
            // ==========================================================
            if (contentPageId.HasValue)
            {
                int pageId = contentPageId.Value;

                var currentMenu = await _context.NavigationMenus
                    .FirstOrDefaultAsync(m => m.ContentPageId == pageId);

                if (currentMenu != null)
                {
                    // Lấy ID của Menu cha
                    int parentId = currentMenu.ParentId ?? currentMenu.Id;

                    // Kéo thông tin thằng Cha lên
                    var parentMenu = await _context.NavigationMenus
                        .Include(m => m.LinkedPage)
                        .FirstOrDefaultAsync(m => m.Id == parentId);

                    if (parentMenu != null)
                    {
                        // 1. TÌM VỎ SIDEBAR (Quét lưới rộng: Tìm theo ID bài viết, theo Category hoặc theo Tên)
                        sidebarConfig = await _context.SidebarItems
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                (x.ContentPageId == parentMenu.ContentPageId ||
                                 x.Category == parentMenu.Name ||
                                 x.Title == parentMenu.Name)
                                && x.IsVisible);

                        // 2. GOM RUỘT (CẢ CHA LẪN CON)
                        var allMenusForSidebar = new List<NavigationMenu>();

                        // 👉 Mời Cha vào danh sách trước (Ví dụ: Giới thiệu chung)
                        if (parentMenu.ContentPageId != null && parentMenu.IsVisible)
                        {
                            allMenusForSidebar.Add(parentMenu);
                        }

                        // 👉 Kéo bầy Con vào theo sau (Ví dụ: cc, ls)
                        var siblingMenus = await _context.NavigationMenus
                            .Include(m => m.LinkedPage)
                            .Where(m => m.ParentId == parentId && m.ContentPageId != null && m.IsVisible)
                            .OrderBy(m => m.DisplayOrder)
                            .ToListAsync();

                        allMenusForSidebar.AddRange(siblingMenus);

                        // Trích xuất bài viết từ Menu để đưa ra giao diện
                        relatedArticles = allMenusForSidebar
                            .Where(m => m.LinkedPage != null)
                            .Select(m => m.LinkedPage)
                            .ToList();
                    }

                    ViewData["SidebarConfig"] = sidebarConfig;
                    return View(relatedArticles);
                }
            }

            // ==========================================================
            // KỊCH BẢN 2: CHUYÊN MỤC HUB (1-N)
            // ==========================================================
            if (!string.IsNullOrEmpty(currentCategory))
            {
                sidebarConfig = await _context.SidebarItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Category == currentCategory && x.IsVisible);

                relatedArticles = await _context.ContentPages
                    .AsNoTracking()
                    .Where(x => x.Category == currentCategory && x.IsVisible)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }

            ViewData["SidebarConfig"] = sidebarConfig;
            return View(relatedArticles);
        }
    }
}