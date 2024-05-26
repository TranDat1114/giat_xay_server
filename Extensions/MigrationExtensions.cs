using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var anyPendingMigrations = await context.Database.GetPendingMigrationsAsync();

        if (anyPendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
        }
    }
}


