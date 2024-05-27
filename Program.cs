using System.Security.Claims;
using System.Text;
using giat_xay_server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Giat xay API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value.",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
);

builder.Services.AddAuthentication()
   .AddBearerToken(IdentityConstants.BearerScheme)
;

// .AddJwtBearer(options =>
// {
// options.TokenValidationParameters = new TokenValidationParameters
// {
//     ValidateIssuer = true,
//     ValidateAudience = true,
//     ValidateLifetime = true,
//     ValidateIssuerSigningKey = true,
//     ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FUCK",
//     ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FUCK",
//     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "FUCK"))
// };
// });
// .AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"))
    .AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "User"));

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

app.MapGroup("/auth")
    .MapIdentityApi<User>().WithTags("Identity");

app.MapGroup("/auth").MapGet("users/me", [Authorize] async (ClaimsPrincipal claims, ApplicationDbContext context) =>
{
    string userId = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
    var user = await context.Users.FindAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }
    user.Role = claims.Claims.First(c => c.Type == ClaimTypes.Role).Value;
    return Results.Ok(user);
}).
RequireAuthorization().WithTags("Identity");

// app.MapGet("users/role/admin", [Authorize(Policy = "Admin")] (ClaimsPrincipal claims, ApplicationDbContext context) =>
// {
//     return "You are an admin!";
// }).
// RequireAuthorization();

// app.MapGet("users/role/public", (ClaimsPrincipal claims, ApplicationDbContext context) =>
// {
//     return "You are an user!";
// });

//crud Laundry Services 
app.MapGet("laundry-services", async (ApplicationDbContext context) =>
    {
        return Results.Ok(await context.LaundryServices.ToListAsync());
    })
    .WithTags("Laundry Services");

app.MapPost("laundry-services", [Authorize(Policy = "Admin")] async (LaundryService laundryService, ApplicationDbContext context) =>
    {
        context.LaundryServices.Add(laundryService);
        await context.SaveChangesAsync();
        return Results.Created($"/laundryServices/{laundryService.Guid}", laundryService);
    })
    .RequireAuthorization()
    .WithTags("Laundry Services");
app.MapGet("laundry-services/{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(guid);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(laundryService);
    })
    .WithTags("Laundry Services");

app.MapPut("laundry-services/{guid}", [Authorize(Policy = "Admin")] async (Guid guid, LaundryService laundryService, ApplicationDbContext context) =>
    {
        var existingLaundryService = await context.LaundryServices.FindAsync(guid);
        if (existingLaundryService == null)
        {
            return Results.NotFound();
        }
        existingLaundryService.Name = laundryService.Name;
        await context.SaveChangesAsync();
        return Results.Ok(existingLaundryService);
    }).RequireAuthorization().WithTags("Laundry Services");

app.MapDelete("laundry-services/{guid}", [Authorize(Policy = "Admin")] async (Guid guid, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(guid);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        context.LaundryServices.Remove(laundryService);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization().WithTags("Laundry Services");

//crud Orders
app.MapGet("orders", [Authorize(Policy = "Admin")] async (ApplicationDbContext context) =>
    {
        return Results.Ok(await context.Orders.ToListAsync());
    })
    .RequireAuthorization()
    .WithTags("Orders");

app.MapPost("orders", async (Order order, ApplicationDbContext context) =>
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return Results.Created($"/orders/{order.Guid}", order);
    }).WithTags("Orders");

app.MapGet("orders/{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var order = await context.Orders.FindAsync(guid);
        if (order == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(order);
    })
    .RequireAuthorization()
    .WithTags("Orders");

app.MapPut("orders/{guid}", async (Guid guid, Order order, ApplicationDbContext context) =>
    {
        var existingOrder = await context.Orders.FindAsync(guid);
        if (existingOrder == null)
        {
            return Results.NotFound();
        }

        existingOrder.PickupAddress = order.PickupAddress;
        existingOrder.DeliveryAddress = order.DeliveryAddress;
        existingOrder.PhoneNumber = order.PhoneNumber;
        existingOrder.Note = order.Note;
        existingOrder.Status = order.Status;
        existingOrder.LaundryServiceGuid = order.LaundryServiceGuid;
        await context.SaveChangesAsync();
        return Results.Ok(existingOrder);
    }).RequireAuthorization().WithTags("Orders");

app.MapDelete("orders/{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var order = await context.Orders.FindAsync(guid);
        if (order == null)
        {
            return Results.NotFound();
        }
        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization().WithTags("Orders");

//crud Prices
app.MapGet("prices", async (ApplicationDbContext context) =>
    {
        return Results.Ok(await context.Prices.ToListAsync());
    })
    .WithTags("Prices");

app.MapPost("prices", [Authorize(Policy = "Admin")] async (Price price, ApplicationDbContext context) =>
    {
        context.Prices.Add(price);
        await context.SaveChangesAsync();
        return Results.Created($"/prices/{price.Guid}", price);
    }).RequireAuthorization().WithTags("Prices");

app.MapGet("prices/{guid}", async (Guid guid, ApplicationDbContext context) =>
    {
        var price = await context.Prices.FindAsync(guid);
        if (price == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(price);
    }).RequireAuthorization().WithTags("Prices");

app.MapPut("prices/{guid}", [Authorize(Policy = "Admin")] async (Guid guid, Price price, ApplicationDbContext context) =>
    {
        var existingPrice = await context.Prices.FindAsync(guid);
        if (existingPrice == null)
        {
            return Results.NotFound();
        }
        existingPrice.Value = price.Value;
        existingPrice.Description = price.Description;
        existingPrice.Weight = price.Weight;
        existingPrice.LaundryServiceGuid = price.LaundryServiceGuid;
        await context.SaveChangesAsync();
        return Results.Ok(existingPrice);
    }).RequireAuthorization().WithTags("Prices");

app.MapDelete("prices/{guid}", [Authorize(Policy = "Admin")] async (Guid guid, ApplicationDbContext context) =>
    {
        var price = await context.Prices.FindAsync(guid);
        if (price == null)
        {
            return Results.NotFound();
        }
        context.Prices.Remove(price);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization().WithTags("Prices");

// app.MapPost("images/upload", async (ImageUploadRequest request, ApplicationDbContext db) =>
// {
//     if (request.File == null || request.File.Length == 0)
//     {
//         return Results.BadRequest("No file uploaded.");
//     }
//     var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
//     if (!Directory.Exists(uploads))
//     {
//         Directory.CreateDirectory(uploads);
//     }

//     var filePath = Path.Combine(uploads, request.File.FileName);
//     await using (var stream = new FileStream(filePath, FileMode.Create))
//     {
//         await request.File.CopyToAsync(stream);
//     }

//     var imageUrl = $"/uploads/{request.File.FileName}";

//     var image = new Image
//     {
//         Url = imageUrl,
//         Name = request.Name,
//         GroupType = request.GroupType
//     };

//     db.Images.Add(image);
//     await db.SaveChangesAsync();

//     return Results.Ok(new { Url = imageUrl });
// }).Accepts<IFormFile>("multipart/form-data").WithTags("Images").DisableAntiforgery();



app.MapPost("/images/upload", async (HttpRequest request, ApplicationDbContext db) =>
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
    .WithTags("Images")
    .DisableAntiforgery()
    .WithOpenApi(
        options =>
        {
            options.Summary = "Upload image";
            options.Description = "Upload image to server and save to database with File name and group type.";
            return options;
        }
    );

app.MapGet("images", async (ApplicationDbContext context) =>
{
    return Results.Ok(await context.Images.ToListAsync());
}).WithTags("Images");

app.MapGet("images/group-type/{type}", async (string? type, ApplicationDbContext context) =>
{
    var image = await context.Images.Where(s => s.GroupType == type).ToListAsync();
    if (image == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(image);
}).WithTags("Images");

app.MapGet("images/{id}", async (int id, ApplicationDbContext context) =>
{
    var image = await context.Images.FindAsync(id);
    if (image == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(image);
}).WithTags("Images");

app.MapDelete("images/{id}", async (int id, ApplicationDbContext context) =>
{
    var image = await context.Images.FindAsync(id);
    if (image == null)
    {
        return Results.NotFound();
    }
    context.Images.Remove(image);
    await context.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Images");

app.Run();
