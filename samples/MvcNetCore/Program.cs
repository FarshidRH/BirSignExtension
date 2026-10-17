using MapIdeaHub.BirSign.NetCoreExtension;
using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        manageUser: UserHelper.EnsureUserExistsAsync,
        webhookUrl: "/api/birsign/webhook",
        webhookHandler: async (serviceProvider, webhookEvent) =>
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Deliveries can be retried with the same EventId. For production,
            // persist processed EventIds in a unique, transactional inbox.
            // Upserts below make simple replay cases safe, not exactly-once.

            switch (webhookEvent)
            {
                case DepartmentCreatedEvent deptCreated:
                    {
                        var deptDto = deptCreated.Department;
                        var dept = await dbContext.Departments.FindAsync(deptDto.Id);
                        if (dept == null)
                        {
                            dept = new Department { Id = deptDto.Id };
                            dbContext.Departments.Add(dept);
                        }
                        dept.Title = deptDto.Title ?? "Unnamed Department";
                        dept.ParentDepartmentId = deptDto.ParentDepartmentId;
                        dept.FullName = deptDto.FullName;
                        dept.Level = deptDto.Level;
                        await dbContext.SaveChangesAsync();
                    }
                    break;

                case DepartmentUpdatedEvent deptUpdated:
                    {
                        var deptDto = deptUpdated.Department;
                        var existing = await dbContext.Departments.FindAsync(deptDto.Id);
                        if (existing != null)
                        {
                            existing.Title = deptDto.Title ?? existing.Title;
                            existing.ParentDepartmentId = deptDto.ParentDepartmentId;
                            existing.FullName = deptDto.FullName;
                            existing.Level = deptDto.Level;
                            dbContext.Departments.Update(existing);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;

                case DepartmentDeletedEvent deptDeleted:
                    {
                        var existing = await dbContext.Departments.FindAsync(deptDeleted.DepartmentId);
                        if (existing != null)
                        {
                            dbContext.Departments.Remove(existing);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;

                case PositionCreatedEvent posCreated:
                    {
                        var posDto = posCreated.Position;
                        var pos = await dbContext.Positions.FindAsync(posDto.Id);
                        if (pos == null)
                        {
                            pos = new Position { Id = posDto.Id };
                            dbContext.Positions.Add(pos);
                        }
                        pos.Title = posDto.Title ?? "Unnamed Position";
                        pos.DepartmentId = posDto.DepartmentId;
                        pos.Description = posDto.Description;
                        pos.QualificationCriteria = posDto.QualificationCriteria;
                        pos.PositionType = posDto.PositionType;
                        pos.Row = posDto.Row;
                        await dbContext.SaveChangesAsync();
                    }
                    break;

                case PositionUpdatedEvent posUpdated:
                    {
                        var posDto = posUpdated.Position;
                        var existing = await dbContext.Positions.FindAsync(posDto.Id);
                        if (existing != null)
                        {
                            existing.Title = posDto.Title ?? existing.Title;
                            existing.DepartmentId = posDto.DepartmentId;
                            existing.Description = posDto.Description;
                            existing.QualificationCriteria = posDto.QualificationCriteria;
                            existing.PositionType = posDto.PositionType;
                            existing.Row = posDto.Row;
                            dbContext.Positions.Update(existing);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;

                case PositionDeletedEvent posDeleted:
                    {
                        var existing = await dbContext.Positions.FindAsync(posDeleted.PositionId);
                        if (existing != null)
                        {
                            dbContext.Positions.Remove(existing);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;

                case UserPositionCreatedEvent upCreated:
                    {
                        var upDto = upCreated.UserPosition;
                        if (string.IsNullOrEmpty(upDto.UserId)) break;

                        var user = await dbContext.Users.FindAsync(upDto.UserId);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                Id = upDto.UserId,
                                UserName = upDto.UserName ?? upDto.UserEmail ?? upDto.UserId,
                                Email = upDto.UserEmail,
                                PhoneNumber = upDto.UserPhoneNumber,
                                Name = upDto.UserFirstName ?? "",
                                Family = upDto.UserLastName ?? "",
                                BirthDay = upDto.UserBirthday,
                                NationalCode = upDto.UserId,
                                EmailConfirmed = upDto.EmailConfirmed,
                                PhoneNumberConfirmed = upDto.PhoneNumberConfirmed,
                                Gender = upDto.Gender
                            };
                            user.SecurityStamp = Guid.NewGuid().ToString();
                            dbContext.Users.Add(user);
                            await dbContext.SaveChangesAsync();
                        }

                        var up = await dbContext.UserPositions.FindAsync(upDto.Id);
                        if (up == null)
                        {
                            up = new UserPosition { Id = upDto.Id };
                            dbContext.UserPositions.Add(up);
                        }
                        up.UserId = upDto.UserId;
                        up.PositionId = upDto.PositionId;
                        up.StartDate = upDto.StartDate;
                        up.EndDate = upDto.EndDate;
                        up.IsActive = upDto.IsActive;
                        await dbContext.SaveChangesAsync();
                    }
                    break;

                case UserPositionUpdatedEvent upUpdated:
                    {
                        var upDto = upUpdated.UserPosition;
                        if (string.IsNullOrEmpty(upDto.UserId)) break;

                        var user = await dbContext.Users.FindAsync(upDto.UserId);
                        if (user != null)
                        {
                            user.Name = upDto.UserFirstName ?? user.Name;
                            user.Family = upDto.UserLastName ?? user.Family;
                            user.BirthDay = upDto.UserBirthday ?? user.BirthDay;
                            user.Gender = upDto.Gender;
                            dbContext.Users.Update(user);
                        }

                        var existing = await dbContext.UserPositions.FindAsync(upDto.Id);
                        if (existing != null)
                        {
                            existing.PositionId = upDto.PositionId;
                            existing.StartDate = upDto.StartDate;
                            existing.EndDate = upDto.EndDate;
                            existing.IsActive = upDto.IsActive;
                            dbContext.UserPositions.Update(existing);
                        }
                        else
                        {
                            var up = new UserPosition
                            {
                                Id = upDto.Id,
                                UserId = upDto.UserId,
                                PositionId = upDto.PositionId,
                                StartDate = upDto.StartDate,
                                EndDate = upDto.EndDate,
                                IsActive = upDto.IsActive
                            };
                            dbContext.UserPositions.Add(up);
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    break;

                case UserPositionDeletedEvent upDeleted:
                    {
                        var existing = await dbContext.UserPositions.FindAsync(upDeleted.UserPositionId);
                        if (existing != null)
                        {
                            dbContext.UserPositions.Remove(existing);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;

                case UserProfileUpdatedEvent userProfileUpdated:
                    {
                        var user = await dbContext.Users.FindAsync(userProfileUpdated.UserId);
                        if (user != null)
                        {
                            user.Name = userProfileUpdated.FirstName ?? user.Name;
                            user.Family = userProfileUpdated.LastName ?? user.Family;
                            user.Email = userProfileUpdated.Email ?? user.Email;
                            user.PhoneNumber = userProfileUpdated.PhoneNumber ?? user.PhoneNumber;
                            dbContext.Users.Update(user);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    break;
            }
        });

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

// Apply any pending migrations at startup
await using var scope = app.Services.CreateAsyncScope();
var services = scope.ServiceProvider;
var dbContext = services.GetRequiredService<ApplicationDbContext>();
await dbContext.Database.MigrateAsync();

app.Run();
