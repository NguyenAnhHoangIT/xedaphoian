using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Data;
using ThueXeDapHoiAn.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Distributed Memory Cache for Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure the application cookie (for redirecting to login page if not authenticated)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/DangNhap";  // Redirect if not logged in
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect if forbidden
    });

builder.Services.AddDbContext<AppDbContextClient>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
// Register DatabaseHelper (to interact with the custom `TaiKhoan` table)
builder.Services.AddScoped<DatabaseHelperClient>();
builder.Services.AddScoped<DatabaseHelper>();

//đăng kí PaypalClient dạng Singleton()
builder.Services.AddSingleton(x => new PaypalClient(
        builder.Configuration["PaypalOptions:AppId"],
        builder.Configuration["PaypalOptions:AppSecret"],
        builder.Configuration["PaypalOptions:Mode"]
));
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<DatabaseHelperAdmin>();
// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Make sure to use session before UseRouting
app.UseSession(); // <-- This line should be placed before UseRouting

app.UseRouting();

// Add authentication/authorization middleware if required
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
