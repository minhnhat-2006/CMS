using CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NavigationMenu> NavigationMenus { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<SidebarItem> SidebarItems { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CẤU HÌNH QUAN TRỌNG: QUAN HỆ CHA - CON ---
            modelBuilder.Entity<NavigationMenu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Cấm xóa Cha nếu còn Con (An toàn dữ liệu)
        }
    }
}