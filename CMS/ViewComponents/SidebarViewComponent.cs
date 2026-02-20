using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? currentCategory)
        {
            // 1. Query cơ bản
            var query = _context.SidebarItems
                .AsNoTracking()
                .Include(x => x.ContentPage) // Include bảng ContentPages để lấy Slug cho Link (nếu có)
                .Where(x => x.IsVisible == true);

            // 2. Logic Lọc theo Category
            if (!string.IsNullOrEmpty(currentCategory))
            {
                // Lấy các item thuộc Category này HOẶC item chung (Category = null)
                query = query.Where(x => x.Category == currentCategory || x.Category == null);
            }
            else
            {
                // Nếu không có Category, chỉ lấy item chung
                query = query.Where(x => x.Category == null);
            }

            // 3. Sắp xếp và Lấy dữ liệu
            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return View(items);
        }
    }
}