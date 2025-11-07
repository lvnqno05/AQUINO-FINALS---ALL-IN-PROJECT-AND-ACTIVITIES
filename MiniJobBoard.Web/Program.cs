using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniJobBoard.Infrastructure;
using MiniJobBoard.Infrastructure.Data;
using MiniJobBoard.Infrastructure.Entities;
using MiniJobBoard.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Infrastructure (DbContext provider)
builder.Services.AddInfrastructure(builder.Configuration);

// Identity (uses Infrastructure's AppDbContext)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, CustomPasswordValidator>();

var app = builder.Build();

// Ensure database exists and seed minimal data in development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Quick bootstrap for dev: create schema if it doesn't exist
    db.Database.EnsureCreated();

    if (app.Environment.IsDevelopment())
    {
        // Seed a minimal employer and a couple of jobs if empty
        if (!db.EmployerProfiles.Any())
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "employer@example.com",
                NormalizedUserName = "EMPLOYER@EXAMPLE.COM",
                Email = "employer@example.com",
                NormalizedEmail = "EMPLOYER@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            db.Users.Add(user);
            var employer = new EmployerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = "Acme Corp",
                Description = "We build things",
                Website = "https://example.com"
            };
            db.EmployerProfiles.Add(employer);
            db.SaveChanges();
        }

        if (!db.Jobs.Any())
        {
            var employerId = db.EmployerProfiles.Select(e => e.Id).First();
            db.Jobs.AddRange(
                new Job { Id = Guid.NewGuid(), EmployerId = employerId, Title = "Junior Developer", Description = "Build and test features.", Location = "Remote", SalaryMin = 40000, SalaryMax = 60000, IsActive = true },
                new Job { Id = Guid.NewGuid(), EmployerId = employerId, Title = "QA Analyst", Description = "Ensure quality.", Location = "Hybrid", SalaryMin = 35000, SalaryMax = 55000, IsActive = true }
            );
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Jobs}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
