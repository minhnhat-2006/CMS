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
            // 1. Chỉ lấy 1 cái Cấu hình chung của Category này (Cái Vỏ)
            var sidebarConfig = await _context.SidebarItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Category == currentCategory && x.IsVisible);

            // 2. Truy tìm TẤT CẢ bài viết thuộc Category này (Cái Ruột)
            var relatedArticles = await _context.ContentPages
                .AsNoTracking()
                .Where(x => x.Category == currentCategory && x.IsVisible)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            // 3. Đẩy cái vỏ vào ViewData, truyền cái ruột ra Model
            ViewData["SidebarConfig"] = sidebarConfig;

            // (Lưu ý: Bạn phải cập nhật file Giao diện Default.cshtml theo hướng dẫn ở tin nhắn trước của tôi)
            return View(relatedArticles);
        }
    }
}