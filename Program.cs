using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using WEBGIS_OSM_IOT.Core;
using WEBSITE_TRAVELBOOKING.Configuration;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using WEBSITE_TRAVELBOOKING.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(7013); // Dùng cổng 7013 giống như hiện tại
//});

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();


//HP
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
        options.CallbackPath = builder.Configuration.GetSection("GoogleKeys:CallbackPath").Value;
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
    }
    );
//HP end


builder.Services.AddDbContext<WebsiteCmsBookingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseSession();
app.UseAuthorization();
//HP
app.UseAuthentication();
//HP end
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//404
app.UseStatusCodePagesWithReExecute("/TrangChu/Error404");
// 404 end
app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=TrangChu}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "partner_area",
    areaName: "partner",
    pattern: "partner/{controller=TrangChu}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}");



//app.MapControllerRoute(
//    name: "admin_area",
//    pattern: "Admin/{controller=TrangChu}/{action=Index}/{id?}",
//    defaults: new { area = "admin" });


Account.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());
app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();
