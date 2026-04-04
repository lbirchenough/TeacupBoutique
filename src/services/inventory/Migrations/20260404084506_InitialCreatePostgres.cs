using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace inventory.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CompletionNotes = table.Column<string>(type: "text", nullable: true),
                    DepositAmountKept = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Contents = table.Column<string>(type: "text", nullable: true),
                    Colour = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MinRentalDays = table.Column<int>(type: "integer", nullable: false),
                    MaxRentalDays = table.Column<int>(type: "integer", nullable: false),
                    BufferDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Servings = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CleanedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTag",
                columns: table => new
                {
                    ProductsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTag", x => new { x.ProductsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ProductTag_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DepositValuePerUnit = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItems_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingItems_ProductSets_ProductSetId",
                        column: x => x.ProductSetId,
                        principalTable: "ProductSets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpareStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpareStocks_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingItemComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DepositValuePerUnit = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItemComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItemComponents_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingItemComponents_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityGood = table.Column<int>(type: "integer", nullable: false),
                    QuantityDamaged = table.Column<int>(type: "integer", nullable: false),
                    QuantityMissing = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReplacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantityCustomerReturned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnAssessments_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnAssessments_SetItems_SetItemId",
                        column: x => x.SetItemId,
                        principalTable: "SetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "BufferDays", "CategoryId", "Colour", "Contents", "CreatedAt", "DepositAmount", "Description", "IsActive", "MaxRentalDays", "MinRentalDays", "Name", "Price", "Servings" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), 1, null, "White with rose pink", "1 teapot, 6 teacups & saucers, 6 side plates, 6 cake plates, milk jug, sugar bowl, 2-tier cake stand", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 80.00m, "A charming floral tea set perfect for an elegant afternoon. Classic bone china with a delicate rose pattern, ideal for bridal showers, baby showers, or a sophisticated catch-up with friends.", true, 2, 1, "Vintage Rose Tea Set", 45.00m, 6 },
                    { new Guid("11111111-1111-1111-1111-111111111102"), 1, null, "Cream and gold", "1 teapot, 4 teacups & saucers, 4 side plates, 4 dessert plates, milk jug, sugar bowl, 3-tier stand, serving tongs", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 100.00m, "Timeless cream and gold set that brings a touch of the Ritz to your home. Perfect for milestone birthdays, Mother's Day, or when you simply want to make an occasion feel special.", true, 2, 1, "Classic English Afternoon Set", 55.00m, 4 },
                    { new Guid("11111111-1111-1111-1111-111111111103"), 1, null, "Pastel floral", "1 large teapot, 8 teacups & saucers, 8 side plates, 8 cake plates, milk jug, sugar bowl, 3-tier cake stand, cake slice, napkins (16)", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 120.00m, "Our most popular set for larger gatherings. Pretty pastel florals on a generous three-tier stand—great for hen parties, engagement parties, or a big family Sunday.", true, 2, 1, "Garden Party Tier Set", 65.00m, 8 },
                    { new Guid("11111111-1111-1111-1111-111111111104"), 1, null, "White and grey", "1 teapot, 4 teacups & saucers, 4 side plates, milk jug, sugar bowl, 2-tier stand", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 70.00m, "Clean, modern take on high tea. Pure white ceramic with soft grey accents—ideal for contemporary homes, baby showers, or when you want a calm, uncluttered look.", true, 2, 1, "Minimalist White Set", 40.00m, 4 },
                    { new Guid("11111111-1111-1111-1111-111111111105"), 1, null, "Ivory with gold rim", "1 teapot, 6 teacups & saucers, 6 side plates, 6 soup/cereal bowls, milk jug, sugar bowl, 3-tier stand, butter dish, jam pot, serving tongs", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 140.00m, "Our most formal option. Fine bone china with a traditional border—perfect for anniversaries, retirement celebrations, or when you want to pull out all the stops.", true, 2, 1, "Royal Style Fine China Set", 75.00m, 6 }
                });

            migrationBuilder.InsertData(
                table: "ProductSets",
                columns: new[] { "Id", "CleanedAt", "CreatedAt", "Name", "ProductId", "Status" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111101"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222202"), null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111102"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222203"), null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111103"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222204"), null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111104"), 0 },
                    { new Guid("22222222-2222-2222-2222-222222222205"), null, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Set A", new Guid("11111111-1111-1111-1111-111111111105"), 0 }
                });

            migrationBuilder.InsertData(
                table: "SetItems",
                columns: new[] { "Id", "DepositValuePerUnit", "IsActive", "Name", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), 25.00m, true, "Teapot", new Guid("11111111-1111-1111-1111-111111111101"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333302"), 8.00m, true, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111101"), 6 },
                    { new Guid("33333333-3333-3333-3333-333333333303"), 7.00m, true, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111101"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333304"), 30.00m, true, "Teapot", new Guid("11111111-1111-1111-1111-111111111102"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333305"), 10.00m, true, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111102"), 4 },
                    { new Guid("33333333-3333-3333-3333-333333333306"), 30.00m, true, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111102"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333307"), 30.00m, true, "Teapot", new Guid("11111111-1111-1111-1111-111111111103"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333308"), 8.00m, true, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111103"), 8 },
                    { new Guid("33333333-3333-3333-3333-333333333309"), 26.00m, true, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111103"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333310"), 25.00m, true, "Teapot", new Guid("11111111-1111-1111-1111-111111111104"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333311"), 8.00m, true, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111104"), 4 },
                    { new Guid("33333333-3333-3333-3333-333333333312"), 13.00m, true, "Plates & Stand", new Guid("11111111-1111-1111-1111-111111111104"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333313"), 35.00m, true, "Teapot", new Guid("11111111-1111-1111-1111-111111111105"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333314"), 12.00m, true, "Teacups & Saucers", new Guid("11111111-1111-1111-1111-111111111105"), 6 },
                    { new Guid("33333333-3333-3333-3333-333333333315"), 33.00m, true, "Plates & Accessories", new Guid("11111111-1111-1111-1111-111111111105"), 1 }
                });

            migrationBuilder.InsertData(
                table: "SpareStocks",
                columns: new[] { "Id", "QuantityAvailable", "SetItemId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), 2, new Guid("33333333-3333-3333-3333-333333333301") },
                    { new Guid("44444444-4444-4444-4444-444444444402"), 2, new Guid("33333333-3333-3333-3333-333333333302") },
                    { new Guid("44444444-4444-4444-4444-444444444403"), 2, new Guid("33333333-3333-3333-3333-333333333303") },
                    { new Guid("44444444-4444-4444-4444-444444444404"), 2, new Guid("33333333-3333-3333-3333-333333333304") },
                    { new Guid("44444444-4444-4444-4444-444444444405"), 2, new Guid("33333333-3333-3333-3333-333333333305") },
                    { new Guid("44444444-4444-4444-4444-444444444406"), 2, new Guid("33333333-3333-3333-3333-333333333306") },
                    { new Guid("44444444-4444-4444-4444-444444444407"), 2, new Guid("33333333-3333-3333-3333-333333333307") },
                    { new Guid("44444444-4444-4444-4444-444444444408"), 2, new Guid("33333333-3333-3333-3333-333333333308") },
                    { new Guid("44444444-4444-4444-4444-444444444409"), 2, new Guid("33333333-3333-3333-3333-333333333309") },
                    { new Guid("44444444-4444-4444-4444-444444444410"), 2, new Guid("33333333-3333-3333-3333-333333333310") },
                    { new Guid("44444444-4444-4444-4444-444444444411"), 2, new Guid("33333333-3333-3333-3333-333333333311") },
                    { new Guid("44444444-4444-4444-4444-444444444412"), 2, new Guid("33333333-3333-3333-3333-333333333312") },
                    { new Guid("44444444-4444-4444-4444-444444444413"), 2, new Guid("33333333-3333-3333-3333-333333333313") },
                    { new Guid("44444444-4444-4444-4444-444444444414"), 2, new Guid("33333333-3333-3333-3333-333333333314") },
                    { new Guid("44444444-4444-4444-4444-444444444415"), 2, new Guid("33333333-3333-3333-3333-333333333315") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemComponents_BookingItemId",
                table: "BookingItemComponents",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemComponents_SetItemId",
                table: "BookingItemComponents",
                column: "SetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_BookingId",
                table: "BookingItems",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_ProductId",
                table: "BookingItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_ProductSetId",
                table: "BookingItems",
                column: "ProductSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ProductId",
                table: "Photos",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSets_ProductId",
                table: "ProductSets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTag_TagsId",
                table: "ProductTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnAssessments_BookingItemId",
                table: "ReturnAssessments",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnAssessments_SetItemId",
                table: "ReturnAssessments",
                column: "SetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetItems_ProductId",
                table: "SetItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SpareStocks_SetItemId",
                table: "SpareStocks",
                column: "SetItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingItemComponents");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "ProductTag");

            migrationBuilder.DropTable(
                name: "ReturnAssessments");

            migrationBuilder.DropTable(
                name: "SpareStocks");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "BookingItems");

            migrationBuilder.DropTable(
                name: "SetItems");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ProductSets");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
