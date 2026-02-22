using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMS.Pages
{
    public class LogoutModel : PageModel
    {
        // Khi người dùng bấm nút Submit từ Form, hàm OnPost này sẽ chạy
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Xóa sạch Cookie phiên đăng nhập hiện tại
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Đá người dùng về trang chủ
            return RedirectToPage("/Index");
        }

        // Đề phòng ai đó gõ thẳng link /Logout lên trình duyệt (GET), cũng đá về trang chủ luôn
        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }
    }
}