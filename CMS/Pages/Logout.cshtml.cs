using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMS.Pages // <-- ĐỔI TÊN NAMESPACE CHO ĐÚNG DỰ ÁN CỦA BẠN
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            // 1. Lệnh xóa Cookie đăng nhập
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Xóa xong thì đá về trang Đăng nhập lại
            return RedirectToPage("/Login");
        }
    }
}