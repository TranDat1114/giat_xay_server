using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq.Expressions;

namespace giat_xay_server;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<LaundryService> LaundryServices { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<LaundryServiceType> LaundryServiceTypes { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Thêm các thông tin về thời gian tạo, cập nhật, xóa và người tạo, cập nhật vào các bảng
    private void SetTimestamps(string? currentUser = "System")
    {
        var entries = ChangeTracker.Entries().Where(x => x.Entity is Entities && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));
        var Now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((Entities)entry.Entity).Guid = Guid.NewGuid();
                ((Entities)entry.Entity).CreatedAt = Now;
                ((Entities)entry.Entity).CreatedBy = currentUser;

                ((Entities)entry.Entity).IsDeleted = false;
            }
            ((Entities)entry.Entity).UpdatedAt = Now;
            ((Entities)entry.Entity).UpdatedBy = currentUser;

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                ((Entities)entry.Entity).IsDeleted = true;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Avatar).HasDefaultValue("https://fastly.picsum.photos/id/228/600/600.jpg?hmac=TDkN4LVBjPRvjQqMs-TT63NvrvlB-FhcHIilfj8U4xg");
        });

        modelBuilder.Entity<LaundryServiceType>(entity =>
        {
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)"); // Thay đổi precision và scale tùy vào yêu cầu của bạn
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)"); // Thay đổi precision và scale tùy vào yêu cầu của bạn
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntities).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
                var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}

