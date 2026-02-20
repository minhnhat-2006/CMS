using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models // Nhớ đổi tên namespace
{
    [Table("TaiKhoan")] // Ánh xạ đúng tên bảng trong SQL
    public class TaiKhoan
    {
        [Key]
        public string TenDangNhap { get; set; } = null!; // Thêm = null!;

        public string MatKhau { get; set; } = null!;     // Thêm = null!;

        public string HoTen { get; set; } = null!;       // Thêm = null!;

        public string VaiTro { get; set; } = null!;      // Thêm = null!;
    }
}