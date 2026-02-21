using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==========================================

// 1.1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 1.2. DỊCH VỤ QUAN TRỌNG (Sửa lỗi InvalidOperationException)
// Giúp truy cập HttpContext trong View/Menu
builder.Services.AddHttpContextAccessor();

// 1.3. Cấu hình MVC và Razor Pages
builder.Services.AddControllersWithViews(); // Để chạy ViewComponent (Menu)

builder.Services.AddRazorPages(options =>
{
    // Cấm truy cập thư mục Admin nếu không phải Admin
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

// 1.4. Cấu hình Authentication (Đăng nhập) & Authorization (Phân quyền)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Trang đăng nhập
        options.AccessDeniedPath = "/Index"; // Trang khi bị từ chối
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// --- XÂY DỰNG APP ---
var app = builder.Build();

// ==========================================
// TỰ ĐỘNG CHẠY MIGRATION KHI LÊN SERVER (RENDER)
// Chú ý: Phải đặt ngay sau var app = builder.Build();
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    // Tự động tìm thư mục Migrations và bắn cấu trúc bảng vào DB
    context.Database.Migrate();
}

// ==========================================
// 2. CẤU HÌNH ĐƯỜNG ỐNG (PIPELINE)
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- QUAN TRỌNG: Thứ tự phải đúng (Authentication trước Authorization) ---
app.UseAuthentication(); // Kích hoạt tính năng đăng nhập
app.UseAuthorization();  // Kích hoạt tính năng phân quyền

// --- ĐỊNH TUYẾN ---

// Ưu tiên 1: Controller (cho các tính năng mặc định)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ưu tiên 2: Razor Pages (để chạy CMS của bạn)
app.MapRazorPages();

app.Run();