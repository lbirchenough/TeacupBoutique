using inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace inventory.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // SQL Server: only one cascade path to a table. BookingItem has FKs to Product and InventoryItem;
        // Product cascades to InventoryItem, so we get two paths to BookingItem. NoAction on this one fixes it.
        modelBuilder.Entity<BookingItem>()
            .HasOne(bi => bi.Product)
            .WithMany(p => p.BookingItems)
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.NoAction);

        SeedProducts(modelBuilder);
        SeedInventoryItems(modelBuilder);
    }

    private static void SeedInventoryItems(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<InventoryItem>().HasData(
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222201"), SerialNumber = "SN-001", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Condition = Condition.New, Status = Status.Available, ConditionNotes = (string?)null, MaintenanceHistory = (string?)null, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222202"), SerialNumber = "SN-002", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Condition = Condition.New, Status = Status.Available, ConditionNotes = (string?)null, MaintenanceHistory = (string?)null, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222203"), SerialNumber = "SN-003", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Condition = Condition.New, Status = Status.Available, ConditionNotes = (string?)null, MaintenanceHistory = (string?)null, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222204"), SerialNumber = "SN-004", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111104"), Condition = Condition.New, Status = Status.Available, ConditionNotes = (string?)null, MaintenanceHistory = (string?)null, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222205"), SerialNumber = "SN-005", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111105"), Condition = Condition.New, Status = Status.Available, ConditionNotes = (string?)null, MaintenanceHistory = (string?)null, CreatedAt = createdAt }
        );
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var id1 = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var id2 = Guid.Parse("11111111-1111-1111-1111-111111111102");
        var id3 = Guid.Parse("11111111-1111-1111-1111-111111111103");
        var id4 = Guid.Parse("11111111-1111-1111-1111-111111111104");
        var id5 = Guid.Parse("11111111-1111-1111-1111-111111111105");
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = id1,
                Name = "Vintage Rose Tea Set",
                Description = "A charming floral tea set perfect for an elegant afternoon. Classic bone china with a delicate rose pattern, ideal for bridal showers, baby showers, or a sophisticated catch-up with friends.",
                Contents = "1 teapot, 6 teacups & saucers, 6 side plates, 6 cake plates, milk jug, sugar bowl, 2-tier cake stand",
                Colour = "White with rose pink",
                Price = 45.00m,
                DepositAmount = 80.00m,
                CreatedAt = createdAt,
                MinRentalDays = 1,
                MaxRentalDays = 2,
                BufferDays = 1,
                IsActive = true,
                Servings = 6
            },
            new Product
            {
                Id = id2,
                Name = "Classic English Afternoon Set",
                Description = "Timeless cream and gold set that brings a touch of the Ritz to your home. Perfect for milestone birthdays, Mother's Day, or when you simply want to make an occasion feel special.",
                Contents = "1 teapot, 4 teacups & saucers, 4 side plates, 4 dessert plates, milk jug, sugar bowl, 3-tier stand, serving tongs",
                Colour = "Cream and gold",
                Price = 55.00m,
                DepositAmount = 100.00m,
                CreatedAt = createdAt,
                MinRentalDays = 1,
                MaxRentalDays = 2,
                BufferDays = 1,
                IsActive = true,
                Servings = 4
            },
            new Product
            {
                Id = id3,
                Name = "Garden Party Tier Set",
                Description = "Our most popular set for larger gatherings. Pretty pastel florals on a generous three-tier stand—great for hen parties, engagement parties, or a big family Sunday.",
                Contents = "1 large teapot, 8 teacups & saucers, 8 side plates, 8 cake plates, milk jug, sugar bowl, 3-tier cake stand, cake slice, napkins (16)",
                Colour = "Pastel floral",
                Price = 65.00m,
                DepositAmount = 120.00m,
                CreatedAt = createdAt,
                MinRentalDays = 1,
                MaxRentalDays = 2,
                BufferDays = 1,
                IsActive = true,
                Servings = 8
            },
            new Product
            {
                Id = id4,
                Name = "Minimalist White Set",
                Description = "Clean, modern take on high tea. Pure white ceramic with soft grey accents—ideal for contemporary homes, baby showers, or when you want a calm, uncluttered look.",
                Contents = "1 teapot, 4 teacups & saucers, 4 side plates, milk jug, sugar bowl, 2-tier stand",
                Colour = "White and grey",
                Price = 40.00m,
                DepositAmount = 70.00m,
                CreatedAt = createdAt,
                MinRentalDays = 1,
                MaxRentalDays = 2,
                BufferDays = 1,
                IsActive = true,
                Servings = 4
            },
            new Product
            {
                Id = id5,
                Name = "Royal Style Fine China Set",
                Description = "Our most formal option. Fine bone china with a traditional border—perfect for anniversaries, retirement celebrations, or when you want to pull out all the stops.",
                Contents = "1 teapot, 6 teacups & saucers, 6 side plates, 6 soup/cereal bowls, milk jug, sugar bowl, 3-tier stand, butter dish, jam pot, serving tongs",
                Colour = "Ivory with gold rim",
                Price = 75.00m,
                DepositAmount = 140.00m,
                CreatedAt = createdAt,
                MinRentalDays = 1,
                MaxRentalDays = 2,
                BufferDays = 1,
                IsActive = true,
                Servings = 6
            }
        );
    }
}
