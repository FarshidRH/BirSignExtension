using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcNetCore.Models;

namespace MvcNetCore.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;
        public DbSet<UserPosition> UserPositions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.ParentDepartment)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(p => p.Department)
                    .WithMany(d => d.Positions)
                    .HasForeignKey(p => p.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserPosition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(up => up.User)
                    .WithMany()
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Position)
                    .WithMany(p => p.UserPositions)
                    .HasForeignKey(up => up.PositionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
