using inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace inventory.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductSet> ProductSets { get; set; }
    public DbSet<SetItem> SetItems { get; set; }
    public DbSet<SpareStock> SpareStocks { get; set; }
    public DbSet<ReturnAssessment> ReturnAssessments { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }
    public DbSet<BookingItemComponent> BookingItemComponents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // SQL Server: only one cascade path to a table. BookingItem has FKs to both Product and ProductSet.
        // Product cascades to ProductSet, giving two paths to BookingItem. NoAction on both fixes it.
        modelBuilder.Entity<BookingItem>()
            .HasOne(bi => bi.Product)
            .WithMany(p => p.BookingItems)
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BookingItem>()
            .HasOne(bi => bi.ProductSet)
            .WithMany(ps => ps.BookingItems)
            .HasForeignKey("ProductSetId")
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BookingItemComponent>()
            .HasOne(c => c.BookingItem)
            .WithMany(bi => bi.Components)
            .HasForeignKey(c => c.BookingItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingItemComponent>()
            .HasOne(c => c.SetItem)
            .WithMany()
            .HasForeignKey(c => c.SetItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // SpareStock: one per SetItem (unique constraint)
        modelBuilder.Entity<SpareStock>()
            .HasIndex(ss => ss.SetItemId)
            .IsUnique();

        SeedProducts(modelBuilder);
        SeedProductSets(modelBuilder);
        SeedSetItems(modelBuilder);
        SeedSpareStock(modelBuilder);
    }

    private static void SeedProductSets(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<ProductSet>().HasData(
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222201"), Name = "Set A", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Status = Status.Available, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222202"), Name = "Set A", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Status = Status.Available, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222203"), Name = "Set A", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Status = Status.Available, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222204"), Name = "Set A", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111104"), Status = Status.Available, CreatedAt = createdAt },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222205"), Name = "Set A", ProductId = Guid.Parse("11111111-1111-1111-1111-111111111105"), Status = Status.Available, CreatedAt = createdAt }
        );
    }

    private static void SeedSetItems(ModelBuilder modelBuilder)
    {
        // Product 1: Vintage Rose Tea Set (deposit=80, servings=6)
        // Product 2: Classic English Afternoon Set (deposit=100, servings=4)
        // Product 3: Garden Party Tier Set (deposit=120, servings=8)
        // Product 4: Minimalist White Set (deposit=70, servings=4)
        // Product 5: Royal Style Fine China Set (deposit=140, servings=6)
        modelBuilder.Entity<SetItem>().HasData(
            // Product 1 — sum: 25 + 48 + 7 = 80
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333301"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Teapot", Quantity = 1, DepositValuePerUnit = 25.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333302"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Teacups & Saucers", Quantity = 6, DepositValuePerUnit = 8.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333303"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Plates & Stand", Quantity = 1, DepositValuePerUnit = 7.00m, IsActive = true },
            // Product 2 — sum: 30 + 40 + 30 = 100
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333304"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Teapot", Quantity = 1, DepositValuePerUnit = 30.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333305"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Teacups & Saucers", Quantity = 4, DepositValuePerUnit = 10.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333306"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Plates & Stand", Quantity = 1, DepositValuePerUnit = 30.00m, IsActive = true },
            // Product 3 — sum: 30 + 64 + 26 = 120
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333307"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Teapot", Quantity = 1, DepositValuePerUnit = 30.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333308"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Teacups & Saucers", Quantity = 8, DepositValuePerUnit = 8.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333309"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Plates & Stand", Quantity = 1, DepositValuePerUnit = 26.00m, IsActive = true },
            // Product 4 — sum: 25 + 32 + 13 = 70
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333310"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Teapot", Quantity = 1, DepositValuePerUnit = 25.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333311"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Teacups & Saucers", Quantity = 4, DepositValuePerUnit = 8.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333312"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Plates & Stand", Quantity = 1, DepositValuePerUnit = 13.00m, IsActive = true },
            // Product 5 — sum: 35 + 72 + 33 = 140
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333313"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Teapot", Quantity = 1, DepositValuePerUnit = 35.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333314"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Teacups & Saucers", Quantity = 6, DepositValuePerUnit = 12.00m, IsActive = true },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333315"), ProductId = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Plates & Accessories", Quantity = 1, DepositValuePerUnit = 33.00m, IsActive = true }
        );
    }

    private static void SeedSpareStock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpareStock>().HasData(
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444401"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333301"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444402"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333302"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444403"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333303"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444404"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333304"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444405"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333305"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444406"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333306"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444407"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333307"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444408"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333308"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444409"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333309"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444410"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333310"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444411"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333311"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444412"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333312"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444413"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333313"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444414"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333314"), QuantityAvailable = 2 },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444415"), SetItemId = Guid.Parse("33333333-3333-3333-3333-333333333315"), QuantityAvailable = 2 }
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
