using Microsoft.EntityFrameworkCore;
using learniverse_be.Models;

namespace learniverse_be.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Auth> Auths => Set<Auth>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Otp> Otps => Set<Otp>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UserCategory> UserCategories => Set<UserCategory>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lecture> Lectures => Set<Lecture>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auth>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<Auth>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(e => e.BirthDate)
            .HasConversion(
                v => v.ToString("yyyy-MM-dd"),
                v => DateOnly.Parse(v)
            );

        modelBuilder.Entity<UserCategory>()
            .HasKey(uc => new { uc.UserId, uc.CategoryId });

        modelBuilder.Entity<UserCategory>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCategories)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserCategory>()
            .HasOne(uc => uc.Category)
            .WithMany(c => c.UserCategories)
            .HasForeignKey(uc => uc.CategoryId);

        modelBuilder.Entity<Instructor>()
            .HasIndex(a => a.DisplayName)
            .IsUnique();

        modelBuilder.Entity<Section>()
            .HasMany(s => s.Lectures)
            .WithOne(l => l.Section)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
