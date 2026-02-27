using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Trang
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPageItem { get; set; } = new ContentPage();

        [BindProperty]
        public int? TargetMenuId { get; set; }

        public IActionResult OnGet(int? menuId) // Đổi thành IActionResult đồng bộ cho nhẹ, vì không query DB nữa
        {
            ContentPageItem.IsVisible = true;
            ContentPageItem.HasSidebar = true;

            if (menuId.HasValue)
                TargetMenuId = menuId.Value;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("ContentPageItem.SidebarItems");
            ModelState.Remove("ContentPageItem.Category");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                string errorString = string.Join("\\n- ", errors);
                string scriptError = $@"<script>alert('LƯU THẤT BẠI! Thiếu dữ liệu:\n- {errorString}'); history.back();</script>";
                return Content(scriptError, "text/html");
            }

            // ✅ Xử lý upload thumbnail
            if (ContentPageItem.ThumbnailFile != null && ContentPageItem.ThumbnailFile.Length > 0)
            {
                var file = ContentPageItem.ThumbnailFile;

                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    string scriptTypeErr = "<script>alert('Ảnh không hợp lệ! Chỉ chấp nhận JPG, PNG, WEBP, GIF.'); history.back();</script>";
                    return Content(scriptTypeErr, "text/html");
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    string scriptSizeErr = "<script>alert('Ảnh vượt quá 5MB! Vui lòng chọn ảnh nhỏ hơn.'); history.back();</script>";
                    return Content(scriptSizeErr, "text/html");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnails");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(file.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                ContentPageItem.Thumbnail = $"/uploads/thumbnails/{fileName}";
            }

            // Móc nối dữ liệu với Menu (Tự động gán theo TargetMenuId)
            if (TargetMenuId.HasValue)
            {
                var targetMenu = await _context.NavigationMenus.FindAsync(TargetMenuId.Value);
                if (targetMenu != null)
                {
                    ContentPageItem.ChuyenMucId = targetMenu.ChuyenMucId;
                    ContentPageItem.Category = targetMenu.Name;
                }
            }

            // Khởi tạo các trường mặc định
            ContentPageItem.CreatedAt = DateTime.UtcNow;
            ContentPageItem.ViewCount = 0;

            // Lưu bài viết vào DB
            _context.ContentPages.Add(ContentPageItem);
            await _context.SaveChangesAsync();

            // Lưu ngược ID bài viết vào Menu
            if (TargetMenuId.HasValue)
            {
                var menuToUpdate = await _context.NavigationMenus.FindAsync(TargetMenuId.Value);
                if (menuToUpdate != null)
                {
                    menuToUpdate.ContentPageId = ContentPageItem.Id;
                    _context.NavigationMenus.Update(menuToUpdate);
                    await _context.SaveChangesAsync();
                }
            }

            string scriptSuccess = $@"
            <script>
                alert('Đã xuất bản bài viết thành công!');
                window.parent.reloadTrang();
            </script>";
            return Content(scriptSuccess, "text/html");
        }
    }
}