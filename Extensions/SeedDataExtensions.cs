using Microsoft.AspNetCore.Identity;

namespace giat_xay_server;

public static class SeedDataExtensions
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure the Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Employeed"));
        }

        // Ensure the Admin user exists
        var adminEmail = "admin@example.com";
        var adminUser = new User { UserName = "admin@example.com", Email = adminEmail, EmailConfirmed = true, PhoneNumber = "1234567890", PhoneNumberConfirmed = true};
        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            var createAdminUser = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (createAdminUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    public static async Task ApplySeedDataAsync(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var serviceProvider = serviceScope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            await SeedAsync(userManager, roleManager);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }
}
