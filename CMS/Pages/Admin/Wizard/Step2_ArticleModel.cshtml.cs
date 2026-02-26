using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CMS.Pages.Admin.Wizard
{
    public class Step2_ArticleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public Step2_ArticleModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public ContentPage ContentPage { get; set; } = new ContentPage();

        [BindProperty]
        public int TargetMenuId { get; set; }

        public string MenuName { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            if (menuId <= 0) return RedirectToPage("./Step1_Menu");

            var currentMenu = await _context.NavigationMenus
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (currentMenu == null) return RedirectToPage("./Step1_Menu");

            TargetMenuId = currentMenu.Id;
            MenuName = currentMenu.Name;

            ContentPage.IsVisible = true;
            ContentPage.HasSidebar = true;
            ContentPage.Category = currentMenu.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("ContentPage.SidebarItems");

            if (!ModelState.IsValid)
                return Page();

            var currentMenu = await _context.NavigationMenus.FindAsync(TargetMenuId);
            if (currentMenu == null) return Page();

            ContentPage.Category = currentMenu.Name;
            ContentPage.ChuyenMucId = currentMenu.ChuyenMucId;

            // ✅ Xử lý upload thumbnail - tự động resize & nén
            if (ContentPage.ThumbnailFile != null && ContentPage.ThumbnailFile.Length > 0)
            {
                var file = ContentPage.ThumbnailFile;

                // Chỉ kiểm tra có phải ảnh không, không giới hạn dung lượng
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh JPG, PNG, WEBP, GIF.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    // Resize thông minh: chỉ thu nhỏ nếu ảnh quá lớn, không phóng to ảnh nhỏ
                    if (image.Width > 1280 || image.Height > 720)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1280, 720),
                            Mode = ResizeMode.Max // Giữ tỉ lệ gốc, không crop
                        }));
                    }

                    // Lưu ra JPEG chất lượng 85%
                    await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
                }

                ContentPage.Thumbnail = $"/uploads/thumbnails/{fileName}";
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ContentPages.Add(ContentPage);
                await _context.SaveChangesAsync();

                currentMenu.ContentPageId = ContentPage.Id;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToPage("./Step3_Sidebar", new { id = ContentPage.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.");
                return Page();
            }
        }
    }
}