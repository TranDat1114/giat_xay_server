using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace giat_xay_server;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<LaundryService> LaundryServices { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Price> Prices { get; set; } = null!;
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

    private void SetTimestamps(string? currentUser = "System")
    {
        var entries = ChangeTracker.Entries().Where(x => x.Entity is Entities && (x.State == EntityState.Added || x.State == EntityState.Modified));
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((Entities)entry.Entity).Guid = Guid.NewGuid();
                ((Entities)entry.Entity).CreatedAt = DateTime.UtcNow;
                ((Entities)entry.Entity).CreatedBy = currentUser;

                ((Entities)entry.Entity).IsDeleted = false;
            }
            ((Entities)entry.Entity).UpdatedAt = DateTime.UtcNow;
            ((Entities)entry.Entity).UpdatedBy = currentUser;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Role)
                .HasDefaultValue("User");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(d => d.LaundryService)
                .WithMany()
                .HasForeignKey(d => d.LaundryServiceGuid);
        });

        modelBuilder.Entity<Price>(entity =>
        {
            entity.Property(e => e.Value)
                .HasColumnType("decimal(18, 2)"); // Thay đổi precision và scale tùy vào yêu cầu của bạn
        });
    }

}

