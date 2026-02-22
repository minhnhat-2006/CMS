using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==========================================

// 1.1. Cấu hình Database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 1.2. DỊCH VỤ QUAN TRỌNG
// Giúp truy cập HttpContext trong View/Menu
builder.Services.AddHttpContextAccessor();

// 1.3. Cấu hình MVC và Razor Pages
builder.Services.AddControllersWithViews(); // Để chạy ViewComponent (Menu)

builder.Services.AddRazorPages(options =>
{
    // Cấm truy cập thư mục Admin nếu không phải Admin
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

// ==========================================
// 1.4. CẤU HÌNH AUTHENTICATION & AUTHORIZATION
// (Đã gộp Cookie và Google vào chung một luồng)
// ==========================================
var googleAuthConfig = builder.Configuration.GetSection("Authentication:Google");

builder.Services.AddAuthentication(options =>
{
    // Đặt mặc định là dùng Cookie (để không bị nhảy thẳng sang Google khi vào trang cấm)
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login"; // Trang đăng nhập
    options.AccessDeniedPath = "/Index"; // Trang khi bị từ chối
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
})
.AddGoogle(options =>
{
    // Thêm dấu ! ở cuối để thề với trình biên dịch là "Tôi chắc chắn trong JSON có mã này, yên tâm!"
    options.ClientId = googleAuthConfig["ClientId"]!;
    options.ClientSecret = googleAuthConfig["ClientSecret"]!;
    options.CallbackPath = "/signin-google";

    options.Events.OnCreatingTicket = context =>
    {
        // Thêm dấu ? để nếu Principal rỗng thì email tự động bằng rỗng, không bị crash
        var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        // Dùng string.IsNullOrEmpty để kiểm tra an toàn tuyệt đối
        if (string.IsNullOrEmpty(email) || !email.EndsWith("@muce.edu.vn"))
        {
            throw new Exception("InvalidEmailDomain");
        }
        return Task.CompletedTask;
    };

    options.Events.OnRemoteFailure = context =>
    {
        // Dấu ? ở Response giúp tránh lỗi "Dereference of a possibly null reference"
        context.Response?.Redirect("/Login?error=invalid_domain");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});
// Đăng ký quyền (Authorization)
builder.Services.AddAuthorization(options =>
{
    // Định nghĩa cái tên 'AdminOnly' mà bạn đang dùng ở Razor Pages
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