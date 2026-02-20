using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.ViewComponents
{
    public class MainNavigationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MainNavigationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.NavigationMenus
                .Include(x => x.Children)
                    .ThenInclude(c => c.LinkedPage) // <--- QUAN TRỌNG: Lấy bài viết của menu con
                .Include(x => x.LinkedPage)         // <--- QUAN TRỌNG: Lấy bài viết của menu cha
                .Where(x => x.ParentId == null && x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return View(items);
        }
    }
}