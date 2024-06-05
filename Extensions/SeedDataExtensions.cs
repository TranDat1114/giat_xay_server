using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public static class SeedDataExtensions
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        // Ensure the Admin role exists
        if (!await roleManager.RoleExistsAsync(Roles.Admin.ToString()))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Manager.ToString()));
            // Ensure the Admin user exists

            var adminEmail = "admin@example.com";
            var adminUser = new User { UserName = "admin", Email = adminEmail, EmailConfirmed = true, PhoneNumber = "1234567890", PhoneNumberConfirmed = true };
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                var createAdminUser = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (createAdminUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                }
            }

            var demoUserEmail = "demo@gmail.com";
            var demoUser = new User { UserName = "demouser", Email = demoUserEmail, EmailConfirmed = true, PhoneNumber = "0987654321", PhoneNumberConfirmed = true };
            var demoUserExist = await userManager.FindByEmailAsync(demoUserEmail);

            if (demoUserExist == null)
            {
                var createDemoUser = await userManager.CreateAsync(demoUser, "Demo@123456");
                if (createDemoUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoUser, Roles.User.ToString());
                }
            }

            #region Example Data

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

            var laundryServiceSayNhanh = await context.LaundryServices.FirstOrDefaultAsync(p => p.Name == "Giặt sấy nhanh");
            if (laundryServiceSayNhanh != null)
            {
                var laundryServiceTypes = new List<LaundryServiceType>()
                {
                    new(){
                        Description="Trên 5kg",
                        UnitValue=1,
                        ConditionValue = 5,
                        UnitType= UnitTypes.Weight,
                        ConditionType= ConditionTypes.GreaterThan,
                        Price=20000,
                        LaundryServiceGuid = laundryServiceSayNhanh.Guid
                    },
                    new(){
                        Description="Dưới 5kg",
                        UnitValue=1,
                        ConditionValue = 5,
                        UnitType= UnitTypes.Time,
                        ConditionType= ConditionTypes.LessThan,
                        Price=100000,
                        LaundryServiceGuid = laundryServiceSayNhanh.Guid
                    },
                };
                await context.LaundryServiceTypes.AddRangeAsync(laundryServiceTypes);
                await context.SaveChangesAsync();

                var laundryServiceTypeOver5Kg = await context.LaundryServiceTypes.FirstOrDefaultAsync(p => p.ConditionValue == 5 && p.LaundryServiceGuid == laundryServiceSayNhanh.Guid && p.ConditionType == ConditionTypes.GreaterThan);

                if (laundryServiceTypeOver5Kg != null)
                {
                    var weight = 8;
                    var orders = new List<Order>()
                    {
                        new(){
                            LaundryServiceGuid=laundryServiceSayNhanh.Guid,
                            LaundryServiceTypeGuid= laundryServiceTypeOver5Kg.Guid, //LaundryService chỉ có price khi là Giặt sấy nhanh và có weight
                            Email="admin@example.com",
                            PhoneNumber="0123456789",
                            Address="Số 1, Đường 1, Phường 1, Quận 1, TP.HCM",
                            UserName="Nguyễn Văn A",
                            Note="Giao hàng sau 1 ngày",
                            Status=OrderStatus.Done.ToString(),
                            Unit="Kg",
                            DeliveryDate=DateTime.UtcNow.AddDays(1),
                            Value=weight,
                            TotalPrice= laundryServiceTypeOver5Kg.Price * (laundryServiceTypeOver5Kg.ConditionType == ConditionTypes.GreaterThan ? weight : 1) ,
                        }
                    };
                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();
                }

                var laundryServiceTypeUnder5Kg = await context.LaundryServiceTypes.FirstOrDefaultAsync(p => p.ConditionValue == 5 && p.LaundryServiceGuid == laundryServiceSayNhanh.Guid && p.ConditionType == ConditionTypes.LessThan);

                if (laundryServiceTypeUnder5Kg != null)
                {
                    var weight = 3;
                    var orders = new List<Order>()
                    {
                        new(){
                            LaundryServiceGuid=laundryServiceSayNhanh.Guid,
                            LaundryServiceTypeGuid= laundryServiceTypeUnder5Kg.Guid, //LaundryService chỉ có price khi là Giặt sấy nhanh và có weight
                            Email="demo@gmail.com",
                            PhoneNumber="0987654321",
                            Address="Số 2, Đường 2, Phường 2, Quận 2, TP.HCM",
                            UserName="Nguyễn Văn B",
                            Note="Giao hàng sau 2 ngày",
                            Status=OrderStatus.Done.ToString(),
                            Unit="Kg",
                            DeliveryDate=DateTime.UtcNow.AddDays(2),
                            Value=weight,
                            TotalPrice= laundryServiceTypeUnder5Kg.Price * (laundryServiceTypeUnder5Kg.ConditionType == ConditionTypes.GreaterThan ? weight : 1) ,
                        }
                    };
                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();
                }
            }
            var laundryServiceGiatHap = await context.LaundryServices.FirstOrDefaultAsync(p => p.Name == "Giặt Hấp Chất lượng cao");

            if (laundryServiceGiatHap != null)
            {
                var laundryServiceTypes = new List<LaundryServiceType>()
                {
                    new(){
                        Description="Áo - Quần",
                        UnitValue=1,
                        ConditionValue = 1,
                        UnitType= UnitTypes.Unit,
                        ConditionType= ConditionTypes.Equal,
                        Price=60000,
                        LaundryServiceGuid = laundryServiceGiatHap.Guid
                    },
                    new(){
                        Description="Bộ Vest - Đầm",
                        UnitValue=1,
                        ConditionValue = 1,
                        UnitType= UnitTypes.Unit,
                        ConditionType= ConditionTypes.Equal,

                        Price=150000,
                        LaundryServiceGuid = laundryServiceGiatHap.Guid
                    },
                     new(){
                        Description="Gấu Bông - (Chăn, Mền, Gối)",
                        UnitValue=1,
                        ConditionValue = 1,
                        UnitType= UnitTypes.Unit,
                        ConditionType= ConditionTypes.Equal,
                        Price=70000,
                        LaundryServiceGuid = laundryServiceGiatHap.Guid
                    }
                };
                await context.LaundryServiceTypes.AddRangeAsync(laundryServiceTypes);
                await context.SaveChangesAsync();

                if (laundryServiceGiatHap != null)
                {
                    var laundryServiceTypeAoQuan = await context.LaundryServiceTypes.FirstOrDefaultAsync(p => p.Description == "Áo - Quần" && p.LaundryServiceGuid == laundryServiceGiatHap.Guid);
                    if (laundryServiceTypeAoQuan != null)
                    {
                        var quantity = 5;
                        var orders = new List<Order>()
                        {
                            new(){
                                LaundryServiceGuid=laundryServiceGiatHap.Guid,
                                LaundryServiceTypeGuid= laundryServiceTypeAoQuan.Guid, //LaundryService chỉ có price khi là Giặt sấy nhanh và có weight
                                Email="demo@gmail.com",
                                PhoneNumber="0987654321",
                                Address="Số 2, Đường 2, Phường 2, Quận 2, TP.HCM",
                                UserName="Nguyễn Văn B",
                                Note="Giao hàng sau 2 ngày",
                                Status=OrderStatus.Done.ToString(),
                                Unit="Bộ",
                                DeliveryDate=DateTime.UtcNow.AddDays(2),
                                Value=quantity,
                                TotalPrice= laundryServiceTypeAoQuan.Price * quantity,
                            }
                        };
                        await context.Orders.AddRangeAsync(orders);
                        await context.SaveChangesAsync();
                    }
                }
            }
            #endregion
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
