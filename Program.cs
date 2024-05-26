using System.Security.Claims;
using System.Text;
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
app.MapGroup("/laundryServices")
    .MapGet("", async (ApplicationDbContext context) =>
    {
        return Results.Ok(await context.LaundryServices.ToListAsync());
    })
    .WithTags("Laundry Services");

app.MapGroup("/laundryServices")
    .MapPost("", async (LaundryService laundryService, ApplicationDbContext context) =>
    {
        context.LaundryServices.Add(laundryService);
        await context.SaveChangesAsync();
        return Results.Created($"/laundryServices/{laundryService.Guid}", laundryService);
    })
    .RequireAuthorization()
    .WithTags("Laundry Services");
app.MapGroup("/laundryServices")
    .MapGet("{id}", async (Guid id, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(id);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(laundryService);
    })
    .WithTags("Laundry Services");

app.MapGroup("/laundryServices")
    .MapPut("{id}", async (Guid id, LaundryService laundryService, ApplicationDbContext context) =>
    {
        var existingLaundryService = await context.LaundryServices.FindAsync(id);
        if (existingLaundryService == null)
        {
            return Results.NotFound();
        }
        existingLaundryService.Name = laundryService.Name;
        await context.SaveChangesAsync();
        return Results.Ok(existingLaundryService);
    }).RequireAuthorization().WithTags("Laundry Services");

app.MapGroup("/laundryServices")
    .MapDelete("{id}", async (Guid id, ApplicationDbContext context) =>
    {
        var laundryService = await context.LaundryServices.FindAsync(id);
        if (laundryService == null)
        {
            return Results.NotFound();
        }
        context.LaundryServices.Remove(laundryService);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }).RequireAuthorization().WithTags("Laundry Services");

//crud Orders
app.MapGroup("/orders")
    .MapGet("", async (ApplicationDbContext context) =>
    {
        return Results.Ok(await context.Orders.ToListAsync());
    })
    .WithTags("Orders");

app.MapGroup("/order")
    .MapPost("", async (Order order, ApplicationDbContext context) =>
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return Results.Created($"/orders/{order.Guid}", order);
    }).WithTags("Orders");

app.Run();
