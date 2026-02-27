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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContentPage ContentPageItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var contentPage = await _context.ContentPages.FirstOrDefaultAsync(m => m.Id == id);
            if (contentPage == null) return NotFound();

            ContentPageItem = contentPage;

            // Đã dọn dẹp phần load Dropdown Menu vì ta dùng thẻ hidden để giữ nguyên Category

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Bỏ qua validate một số trường không thay đổi trên form
            ModelState.Remove("ContentPageItem.SidebarItems");
            ModelState.Remove("ContentPageItem.Category");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                string errorString = string.Join("\\n- ", errors);
                string scriptError = $@"<script>alert('CẬP NHẬT THẤT BẠI!\n- {errorString}'); history.back();</script>";
                return Content(scriptError, "text/html");
            }

            // ✅ Xử lý upload Thumbnail nếu người dùng chọn ảnh mới
            if (ContentPageItem.ThumbnailFile != null && ContentPageItem.ThumbnailFile.Length > 0)
            {
                var file = ContentPageItem.ThumbnailFile;
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };

                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return Content("<script>alert('Ảnh không hợp lệ!'); history.back();</script>", "text/html");
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

                // Gán link ảnh mới (nếu không up ảnh mới, entity tự giữ đường link cũ từ hidden input)
                ContentPageItem.Thumbnail = $"/uploads/thumbnails/{fileName}";
            }
            ContentPageItem.CreatedAt = DateTime.SpecifyKind(ContentPageItem.CreatedAt, DateTimeKind.Utc);

            // Đánh dấu bản ghi đã bị thay đổi
            _context.Attach(ContentPageItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ContentPages.Any(e => e.Id == ContentPageItem.Id)) return NotFound();
                else throw;
            }

            // Điều hướng trả về đúng danh sách bài của chuyên mục cũ trong Popup
            return RedirectToPage("./Index", new { category = ContentPageItem.Category, layout = "popup" });
        }
    }
}