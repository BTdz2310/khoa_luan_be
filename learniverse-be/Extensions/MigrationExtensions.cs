using learniverse_be.Data;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using AppDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();


        Console.WriteLine("Applying migrations...");

        dbContext.Database.Migrate();
    }
}