using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniJobBoard.Infrastructure.Entities;

namespace MiniJobBoard.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EmployerProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithOne()
             .HasForeignKey<EmployerProfile>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Job>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Employer)
             .WithMany(x => x.Jobs)
             .HasForeignKey(x => x.EmployerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobApplication>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Job)
             .WithMany(x => x.Applications)
             .HasForeignKey(x => x.JobId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Applicant)
             .WithMany()
             .HasForeignKey(x => x.ApplicantUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}