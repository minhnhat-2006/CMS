using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides; // Đã gom lên trên cùng
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==========================================

// 1.1. Giúp app nhận diện đúng HTTPS khi chạy qua Proxy của Render
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 1.2. Cấu hình Database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 1.3. DỊCH VỤ QUAN TRỌNG
builder.Services.AddHttpContextAccessor();

// 1.4. Cấu hình MVC và Razor Pages
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

// ==========================================
// 1.5. CẤU HÌNH AUTHENTICATION & AUTHORIZATION
// ==========================================
var googleAuthConfig = builder.Configuration.GetSection("Authentication:Google");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Index";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
})
.AddGoogle(options =>
{
    options.ClientId = googleAuthConfig["ClientId"]!;
    options.ClientSecret = googleAuthConfig["ClientSecret"]!;
    options.CallbackPath = "/signin-google";

    options.Events.OnCreatingTicket = context =>
    {
        var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email) || !email.EndsWith("@muce.edu.vn"))
        {
            throw new Exception("InvalidEmailDomain");
        }
        return Task.CompletedTask;
    };

    options.Events.OnRemoteFailure = context =>
    {
        context.Response?.Redirect("/Login?error=invalid_domain");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// --- XÂY DỰNG APP ---
var app = builder.Build();

// ==========================================
// TỰ ĐỘNG CHẠY MIGRATION KHI LÊN SERVER
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// ==========================================
// 2. CẤU HÌNH ĐƯỜNG ỐNG (PIPELINE)
// ==========================================

// THÊM Ở ĐÂY: Kích hoạt Forwarded Headers trước khi hệ thống xử lý bất cứ thứ gì khác
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự vàng: Auth trước, Quyền sau
app.UseAuthentication();
app.UseAuthorization();

// --- ĐỊNH TUYẾN ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();