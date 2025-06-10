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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auth>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<Auth>()
            .HasIndex(a => a.Email)
            .IsUnique();
    }
}
