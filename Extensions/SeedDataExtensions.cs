using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public static class SeedDataExtensions
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        // Ensure the Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Employeed"));
            // Ensure the Admin user exists

            var adminEmail = "admin@example.com";
            var adminUser = new User { UserName = "admin@example.com", Email = adminEmail, EmailConfirmed = true, PhoneNumber = "1234567890", PhoneNumberConfirmed = true };
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                var createAdminUser = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (createAdminUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var images = new List<Image>(){
                new(){
                    ImageGuid = Guid.NewGuid(),
                    GroupType="Banner",
                    Name="Banner 1",
                    Url="/uploads/GiatSayNhanh-main-banner.jpg"
                },
                new(){
                    ImageGuid = Guid.NewGuid(),
                    GroupType="Banner",
                    Name="Banner 2",
                    Url="/uploads/GiatSayNhanh-banner-2.jpg"
                },
                new(){
                    ImageGuid = Guid.NewGuid(),
                    GroupType="Banner",
                    Name="Banner 3",
                    Url="/uploads/GiatSayNhanh-banner-3a.jpg"
                },
            };

            await context.Images.AddRangeAsync(images);
            await context.SaveChangesAsync();

            var laundryServices = new List<LaundryService>()
            {
                new(){
                    Name="Giặt sấy nhanh",
                    Description ="Miễn phí giao nhận tận nơi đơn hàng trên 200k",
                    ImageUrl="https://giatsaynhanh.vn/wp-content/uploads/2018/03/Service-GiatSayNhanh-GiatSayNhanh.jpg",
                },
                new(){
                    Name="Giặt Hấp Chất lượng cao",
                    Description ="Miễn phí giao nhận tận nơi đơn hàng trên 200k",
                    ImageUrl="https://giatsaynhanh.vn/wp-content/uploads/2018/03/Service-Folded-GiatSayNhanh.jpg",
                },
                new(){
                    Name="Giặt ủi Spa, Nhà hàng, Khách sạn",
                    Description ="Chuyên Nghiệp - Uy Tín - Giá cả cạnh tranh",
                    ImageUrl="http://giatsaynhanh.vn/wp-content/uploads/2017/02/may-giat-say-electrolux-GiatSayNhanh.jpg",
                },
            };
            await context.LaundryServices.AddRangeAsync(laundryServices);
            await context.SaveChangesAsync();

            var laundryService = await context.LaundryServices.FirstOrDefaultAsync(p => p.Name == "Giặt sấy nhanh");
            if (laundryService != null)
            {
                var laundryServiceTypes = new List<LaundryServiceType>()
                {
                    new(){
                        Description="Trên 5kg",
                        UnitValue=5,
                        UnitType= UnitType.Weight,
                        ConditionType= ConditionTYpe.GreaterThan,
                        Price=20000,
                        LaundryServiceGuid = laundryService.Guid
                    },
                    new(){
                        Description="Dưới 5kg",
                        UnitValue=5,
                        UnitType= UnitType.Weight,
                        ConditionType= ConditionTYpe.LessThan,
                        Price=100000,
                        LaundryServiceGuid = laundryService.Guid
                    },
                };
                await context.LaundryServiceTypes.AddRangeAsync(laundryServiceTypes);
                await context.SaveChangesAsync();

                var laundryServiceType = await context.LaundryServiceTypes.FirstOrDefaultAsync(p => p.UnitValue == 5 && p.LaundryServiceGuid == laundryService.Guid && p.ConditionType == ConditionTYpe.GreaterThan);

                if (laundryServiceType != null)
                {
                    var weight = 8;
                    var orders = new List<Order>()
                    {
                        new(){
                            LaundryServiceGuid=laundryService.Guid,
                            LaundryServiceTypeGuid= laundryServiceType.Guid, //LaundryService chỉ có price khi là Giặt sấy nhanh và có weight
                            Email="admin@example.com",
                            PhoneNumber="0123456789",
                            DeliveryAddress="Số 1, Đường 1, Phường 1, Quận 1, TP.HCM",
                            UserName="Nguyễn Văn A",
                            PickupAddress="Số 1, Đường 1, Phường 1, Quận 1, TP.HCM",
                            Note="Giao hàng sau 1 ngày",
                            Status=OrderStatus.Done.ToString(),
                            Unit="Kg",
                            PickupDate=DateTime.UtcNow,
                            DeliveryDate=DateTime.UtcNow.AddDays(1),
                            Description="Giặt sấy nhanh",
                            Weight=weight,
                            TotalPrice= laundryServiceType.Price * (laundryServiceType.ConditionType == ConditionTYpe.GreaterThan ? weight : 1) ,
                        }
                    };
                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();

                }
            }
        }
    }

    // Tạo dữ liệu mẫu 
    public static async Task ApplySeedDataAsync(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var serviceProvider = serviceScope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            await SeedAsync(userManager, roleManager, context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }
}
