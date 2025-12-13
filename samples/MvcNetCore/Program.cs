using MapIdeaHub.BirSign.NetCoreExtension;
using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcNetCore.Data;
using MvcNetCore.Helpers;
using MvcNetCore.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

if (BirSignSettings.IsUseBirSign(builder.Configuration))
{
    builder.Services.AddBirSignAuthentication(
        builder.Configuration,
        manageUser: UserHelper.EnsureUserExistsAsync);

    builder.Services.AddScoped(sp =>
    {
        var config = builder.Configuration.GetSection("BirSign");
        var authorityUri = config["Authority"] ?? throw new InvalidOperationException("BirSign:Authority is not configured.");
        var birSignApiUri = config["ApiUri"] ?? throw new InvalidOperationException("BirSign:ApiUri is not configured.");
        var clientId = config["ClientId"] ?? throw new InvalidOperationException("BirSign:ClientId is not configured.");
        var clientSecret = config["ClientSecret"] ?? throw new InvalidOperationException("BirSign:ClientSecret is not configured.");

        return new IdsService(authorityUri, birSignApiUri, clientId, clientSecret);
    });
}

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
