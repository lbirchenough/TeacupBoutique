using inventory.Data;
using inventory.Entities;
using inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(InventoryDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            //AsNoTracking() tells EF Core to load entities as read-only: it doesn’t create a snapshot of them or put them in the change tracker.
            //Change tracker uses more memory and is slower as it tracks changes to the entities.
            // var products = await _context.Products
            //     .Include(p => p.Photos)
            //     .AsNoTracking()
            //     .ToListAsync();

            // var dtos = products.Select(p => new ProductListDto
            // {
            //     Id = p.Id,
            //     Name = p.Name,
            //     Description = p.Description,
            //     Colour = p.Colour,
            //     Price = p.Price,
            //     Servings = p.Servings,
            //     FeaturedPhotoUrl = p.Photos?.FirstOrDefault(ph => ph.IsFeatured)?.Url
            //         ?? p.Photos?.FirstOrDefault()?.Url
            // }).ToList();
            
            // This is the correct way to get the products with the featured photo url
            // Using the select projection, entity framework doesn't load the full product and photo entities into memory.
            // Instead, it loads only the necessary data for the projection.
            var products = await _context.Products
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    FeaturedPhotoUrl = p.Photos
                        .Where(ph => ph.IsFeatured)
                        .Select(ph => ph.Url)
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Photos)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

           //TODO WE SHOULD ADD A DTO AND DO SELECT PROJECTION HERE TOO, RETURNING DTO BETTER 
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductUpdateDto updateDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Update properties
            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.Colour = updateDto.Colour;
            product.Price = updateDto.Price;
            product.Servings = updateDto.Servings;
            product.DepositAmount = updateDto.DepositAmount;
            product.MinRentalDays = updateDto.MinRentalDays;
            product.MaxRentalDays = updateDto.MaxRentalDays;
            product.BufferDays = updateDto.BufferDays;
            product.IsActive = updateDto.IsActive;
            product.Contents = updateDto.Contents;


            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetProductItems(Guid id)
        {
            //Just check if the product exists without actually returning the product entity just true/false
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == id);

            if (!productExists)
            {
                return NotFound();
            }

            //Query the inventoryitems table filtering on product id
            //Note: Added ProductID as a property in addition to the Proudct navigation property in the InventoryItem entity to allow filtering on product id without needing SQL Join
            var items = await _context.InventoryItems
                // .Where(i => i.Product.Id == id)
                .Where(i => i.ProductId == id)
                .Select(i => new InventoryItemsDto
                {
                    Id = i.Id,
                    Condition = i.Condition,
                    Status = i.Status,
                    ConditionNotes = i.ConditionNotes,
                    MaintenanceHistory = i.MaintenanceHistory
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(items);
        }

        [HttpPut("{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateInventoryItem(Guid id, Guid itemId, InventoryItemsDto itemDto)
        {
            //This approach was inefficient as it loaded full product and inventory item entities into memory
            // var product = await _context.Products
            //     .Include(p => p.InventoryItems)
            //     .FirstOrDefaultAsync(p => p.Id == id);

            // if (product == null)
            // {
            //     return NotFound();
            // }

            // var inventoryItem = product.InventoryItems?.FirstOrDefault(i => i.Id == itemId);
            

            //Just directly query the inventoryitems table filtering on item id and product id
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ProductId == id);

            if (inventoryItem == null)
            {
                return NotFound();
            }

            // Update properties
            inventoryItem.Condition = itemDto.Condition;
            inventoryItem.Status = itemDto.Status;
            inventoryItem.ConditionNotes = itemDto.ConditionNotes;
            inventoryItem.MaintenanceHistory = itemDto.MaintenanceHistory;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // public IActionResult Get()
        // {
        //     return Ok(new string[] { "product1", "product2" });
        // }
    }
}
