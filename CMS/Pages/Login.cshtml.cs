using CMS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;

namespace CMS.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        // CHỈ GIỮ LẠI MỘT HÀM OnGet NÀY THÔI
        public IActionResult OnGet()
        {
            // CHỐT CHẶN: Nếu đã đăng nhập rồi thì không cho vào trang Login nữa
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Nếu là Admin thì ném thẳng vào phòng làm việc
                if (User.IsInRole("Admin"))
                {
                    return RedirectToPage("/Admin/Dashboard");
                }

                // Dân thường thì mời ra trang chủ
                return RedirectToPage("/Index");
            }

            // Nếu chưa đăng nhập thì mới hiện Form Login bình thường
            return Page();
        }

        public IActionResult OnPostGoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Page("/Index") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Tìm tài khoản trong Database
            var user = _context.TaiKhoan.FirstOrDefault(u => u.TenDangNhap == Username && u.MatKhau == Password);

            if (user != null)
            {
                // Tìm thấy! Tạo xác thực
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                    new Claim(ClaimTypes.Role, user.VaiTro ?? "Student")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Kiểm tra quyền để chuyển hướng
                if (user.VaiTro == "Admin")
                {
                    return RedirectToPage("/Admin/Dashboard/Index"); // Đã sửa lại đường dẫn cho chuẩn
                }
                else
                {
                    return RedirectToPage("/Index");
                }
            }

            // Đăng nhập thất bại
            ErrorMessage = "Tài khoản hoặc mật khẩu không đúng!";
            return Page();
        }
    }
}