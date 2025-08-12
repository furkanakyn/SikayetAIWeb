using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using SikayetAIWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Hizmetler
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Normal kullanýcýlar için Session ayarlarý
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "UserSession"; 
});

// Admin için SADECE Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "AdminAuthCookie";
})
.AddCookie("AdminAuthCookie", options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.Cookie.Name = "AdminAuthCookie";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servisler
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ComplaintService>();
builder.Services.AddHttpClient<CategoryPredictionService>();
builder.Services.AddScoped<CategoryPredictionService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    
    if (!db.Users.Any(u => u.UserType == UserType.admin))
    {
        var authService = services.GetRequiredService<AuthService>();
        authService.CreatePasswordHash("1234", out string passwordHash);

        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            Email = "admin@example.com",
            FullName = "Admin User",
            UserType = UserType.admin,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapAreaControllerRoute(
    name: "MunicipalityArea",
    areaName: "Municipality",
    pattern: "Municipality/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// Varsayýlan rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();