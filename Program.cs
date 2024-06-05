using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using giat_xay_server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Cấu hình bảo mật JWT cho Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT Bearer token vào ô bên dưới",

        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var jwtSettings = builder.Configuration.GetSection("JWT");

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "FUCK",
                ValidAudience = jwtSettings["Audience"] ?? "FUCK",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "FUCK"))
            };
        });
// .AddCookie(IdentityConstants.ApplicationScheme);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RolesString.Admin, policy => policy.RequireClaim(ClaimTypes.Role, RolesString.Admin))
    .AddPolicy(RolesString.User, policy => policy.RequireClaim(ClaimTypes.Role, RolesString.User));

builder.Services.AddIdentityCore<User>(
    options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    }
)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalDb")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddLogging();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrations();
    await app.ApplySeedDataAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors();

app.UseHttpsRedirection();

var auth = app.MapGroup("/auth").WithTags("Identity");

auth.MapPost("/register", async (UserManager<User> userManager, RegisterRequest request) =>
{
    var userData = new User { UserName = request.UserName, Email = request.Email };

    var user = await userManager.FindByEmailAsync(userData.Email);

    if (user != null)
    {
        return Results.BadRequest("Email already exists");
    }

    var result = await userManager.CreateAsync(userData, request.Password);

    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(userData, Roles.User.ToString());
        return Results.Ok();
    }
    else
    {
        return Results.BadRequest(result.Errors);
    }
});

// Đăng nhập endpoint
auth.MapPost("/login", async (UserManager<User> userManager, IConfiguration configuration, LoginRequest request) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user != null && await userManager.CheckPasswordAsync(user, request.Password))
    {
        var role = await userManager.GetRolesAsync(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role?.FirstOrDefault() == RolesString.Admin?RolesString.Admin:RolesString.User)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "FUCK")), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"] ?? "FUCK",
            Audience = jwtSettings["Audience"] ?? "FUCK",
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Results.Ok(new { AccessToken = tokenString });
    }

    return Results.Unauthorized();
});

auth.MapGet("users/me", [Authorize] async (ClaimsPrincipal claims, UserManager<User> userManager, ApplicationDbContext context) =>
{
    string userId = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
    var user = await context.Users.FindAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }
    user.Role = JsonSerializer.Serialize(await userManager.GetRolesAsync(user));
    return Results.Ok(user);
}).
RequireAuthorization();

var laundryService = app.MapGroup("laundry-services").WithTags("Laundry Services");

laundryService.MapGet("", async (ApplicationDbContext context) =>
    {
        var laundryServices = await context.LaundryServices
            .AsNoTracking()
            .Include(p => p.LaundryServiceTypes)
            .ToArrayAsync();
        return Results.Ok(laundryServices);
    });

laundryService.MapPost("", [Authorize(Policy = RolesString.Admin)] async (LaundryService laundryService, ApplicationDbContext context) =>
    {
        context.LaundryServices.Add(laundryService);
        await context.SaveChangesAsync();
        return Results.Created($"/laundryServices/{laundryService.Guid}", laundryService);
    })
    .RequireAuthorization();
laundryService.MapGet("{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(guid);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(laundryService);
    });

laundryService.MapPut("{guid}", [Authorize(Policy = RolesString.Admin)] async (Guid guid, LaundryService laundryService, ApplicationDbContext context) =>
    {
        var existingLaundryService = await context.LaundryServices.FindAsync(guid);
        if (existingLaundryService == null)
        {
            return Results.NotFound();
        }
        existingLaundryService.Name = laundryService.Name;
        existingLaundryService.Description = laundryService.Description;
        existingLaundryService.ImageUrl = laundryService.ImageUrl;

        await context.SaveChangesAsync();
        return Results.Ok(existingLaundryService);
    }).RequireAuthorization();

laundryService.MapDelete("{guid}", [Authorize(Policy = RolesString.Admin)] async (Guid guid, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(guid);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        context.LaundryServices.Remove(laundryService);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization();

var inCome = app.MapGroup("/income").WithTags("Income");

inCome.MapGet("", async (ApplicationDbContext context) =>
    {
        var income = new Income()
        {
            TotalIncome = await context.Orders.SumAsync(s => s.TotalPrice),
            TotalOrders = await context.Orders.CountAsync(),
            TotalIncomeThisWeek = await context.Orders.Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-7)).SumAsync(s => s.TotalPrice),
            TotalIncomeThisMonth = await context.Orders.Where(s => s.CreatedAt.Month == DateTime.UtcNow.Month).SumAsync(s => s.TotalPrice),
            TotalIncomeThisYear = await context.Orders.Where(s => s.CreatedAt.Year == DateTime.UtcNow.Year).SumAsync(s => s.TotalPrice),
            TotalOrdersThisWeek = await context.Orders.Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-7)).CountAsync(),
            TotalOrdersThisMonth = await context.Orders.Where(s => s.CreatedAt.Month == DateTime.UtcNow.Month).CountAsync(),
            TotalOrdersThisYear = await context.Orders.Where(s => s.CreatedAt.Year == DateTime.UtcNow.Year).CountAsync(),
        };
        return Results.Ok(income);
    });

var order = app.MapGroup("/orders").WithTags("Orders");

order.MapGet("", [Authorize(Policy = RolesString.Admin)] async ([AsParameters] Pagination pagination, ApplicationDbContext context) =>
    {
        ApiResponse<Pagination<Order>> response = new();
        try
        {
            PaginationService paginationService = new();
            var orders = await paginationService.GetPaginatedList(pagination, context.Orders);

            if (orders.Data != null && orders.Data.Any())
            {
                foreach (var order in orders.Data)
                {
                    var laundryService = await context.LaundryServices.FindAsync(order.LaundryServiceGuid);
                    var laundryServiceType = await context.LaundryServiceTypes.FindAsync(order.LaundryServiceTypeGuid);
                    order.LaundryServiceName = laundryService?.Name;
                    order.LaundryServiceTypeDescription = laundryServiceType?.Description;

                    order.TotalPrice = OrderPriceExtensions.TotalPrice(order.Value ?? 0, laundryServiceType!, laundryServiceType!.UnitType);
                }
            }

            response.Result = orders;
            response.Success = true;
            response.Message = "Get orders successfully";
            return Results.Ok(response);
        }
        catch (Exception e)
        {
            return Results.BadRequest(
                new
                {
                    e.Message
                }
            );
        }
    })
    .RequireAuthorization();

order.MapPost("", [Authorize] async (Order order, ApplicationDbContext context) =>
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return Results.Created($"/orders/{order.Guid}", order);
    });

order.MapGet("{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var order = await context.Orders.FindAsync(guid);
        if (order == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(order);
    })
    .RequireAuthorization();

order.MapPut("{guid}", async (Guid guid, Order order, ApplicationDbContext context) =>
    {
        var existingOrder = await context.Orders.FindAsync(guid);
        if (existingOrder == null)
        {
            return Results.NotFound();
        }
        existingOrder.UserName = order.UserName;
        existingOrder.Email = order.Email;
        existingOrder.Address = order.Address;
        existingOrder.DeliveryDate = order.DeliveryDate;
        existingOrder.PhoneNumber = order.PhoneNumber;
        existingOrder.Note = order.Note;
        existingOrder.Status = order.Status;
        existingOrder.LaundryServiceGuid = order.LaundryServiceGuid;
        existingOrder.LaundryServiceTypeGuid = order.LaundryServiceTypeGuid;
        await context.SaveChangesAsync();
        return Results.Ok(existingOrder);
    }).RequireAuthorization();

order.MapDelete("{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var order = await context.Orders.FindAsync(guid);
        if (order == null)
        {
            return Results.NotFound();
        }
        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization();

var image = app.MapGroup("/images").WithTags("Images");
image.MapPost("/upload", async (HttpRequest request, ApplicationDbContext db) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    var name = form["name"].ToString();
    var groupType = form["groupType"].ToString();

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    if (!Directory.Exists(uploads))
    {
        Directory.CreateDirectory(uploads);
    }

    var filePath = Path.Combine(uploads, file.FileName);
    await using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var imageUrl = $"/uploads/{file.FileName}";

    var image = new Image
    {
        Url = imageUrl,
        Name = name,
        GroupType = groupType
    };

    db.Images.Add(image);
    await db.SaveChangesAsync();

    return Results.Ok(new { Url = imageUrl });
}).Accepts<IFormFile>("multipart/form-data")
    .DisableAntiforgery()
    .WithOpenApi(
        options =>
        {
            options.Summary = "Upload image";
            options.Description = "Upload image to server and save to database with File name and group type.";
            return options;
        }
    );

image.MapGet("", async (ApplicationDbContext context) =>
{
    return Results.Ok(await context.Images.ToListAsync());
});

image.MapGet("group-type/{type}", async (string? type, ApplicationDbContext context) =>
{
    var image = await context.Images.Where(s => s.GroupType == type).ToListAsync();
    if (image == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(image);
});

image.MapGet("{id}", async (int id, ApplicationDbContext context) =>
{
    var image = await context.Images.FindAsync(id);
    if (image == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(image);
});

image.MapDelete("images/{id}", async (int id, ApplicationDbContext context) =>
{
    var image = await context.Images.FindAsync(id);
    if (image == null)
    {
        return Results.NotFound();
    }
    context.Images.Remove(image);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
