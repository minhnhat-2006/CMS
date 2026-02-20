using CMS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
// QUAN TRỌNG: Thêm dòng using trỏ đến thư mục chứa DbContext và Model của bạn
// Ví dụ: using TenDuAn.Data; 
// Ví dụ: using TenDuAn.Models;

namespace CMS.Pages // <-- Đổi tên namespace này cho đúng với dự án của bạn
{
    public class LoginModel : PageModel
    {
        // 1. Khai báo biến Database
        // HÃY SỬA TÊN 'ApplicationDbContext' THÀNH TÊN ĐÚNG CỦA BẠN (Ví dụ: WebContext)
        private readonly ApplicationDbContext _context;

        // 2. Hàm khởi tạo (Constructor) để nhận Database vào
        public LoginModel(ApplicationDbContext context) // <-- Sửa tên ở đây nữa
        {
            _context = context;
        }

        [BindProperty]
        public string ErrorMessage { get; set; } = string.Empty; // Gán mặc định để hết lỗi null

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 3. Tìm tài khoản trong Database
            // Lưu ý: Đảm bảo bảng trong SQL tên là TaiKhoan, và Code Model cũng tên là TaiKhoan
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
                    return RedirectToPage("/Admin/Dashboard/Index");
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