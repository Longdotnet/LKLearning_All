using Microsoft.AspNetCore.Authentication.Cookies;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<CXuLy>(); // Đăng ký CXuLy với Scoped lifetime

// Đăng ký Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// Add Session service
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
ApiConfig.Initialize(builder.Configuration);


    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapAreaControllerRoute(
        name: "Admin",
        areaName: "Admin",
        pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
      name: "Staff",
      areaName: "Staff",
      pattern: "Staff/{controller=Dashboard}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
        name: "User",
        areaName: "User",
        pattern: "User/{controller=Dashboard}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
        name: "Login",
        areaName: "Login",
        pattern: "Login/{controller=Login}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
      name: "ForgotPassword",
      areaName: "ForgotPassword",
      pattern: "ForgotPassword/{controller=ForgotPassword}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
        name: "SignUp",
        areaName: "SignUp",
        pattern: "{controller=SignUp}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
        name: "UpdateProfile",
        areaName: "UpdateProfile",
        pattern: "UpdateProfile/{controller=UpdateProfile}/{action=Index}/{id?}"
    );
    endpoints.MapAreaControllerRoute(
        name: "Base",
        areaName: "Base",
        pattern: "Base/{controller=Base}/{action=Index}/{id?}"
    );
    endpoints.MapControllerRoute(
        name: "Home",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();