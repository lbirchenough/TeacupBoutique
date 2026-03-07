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
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Contents = dto.Contents,
                Colour = dto.Colour,
                Price = dto.Price,
                DepositAmount = dto.DepositAmount,
                CreatedAt = DateTime.UtcNow,
                MinRentalDays = dto.MinRentalDays,
                MaxRentalDays = dto.MaxRentalDays,
                BufferDays = dto.BufferDays,
                IsActive = dto.IsActive,
                Servings = dto.Servings
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { productId = product.Id }, product);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            //AsNoTracking() tells EF Core to load entities as read-only: it doesn’t create a snapshot of them or put them in the change tracker.
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

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            var product = await _context.Products
                .Include(p => p.Photos)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

           //TODO WE SHOULD ADD A DTO AND DO SELECT PROJECTION HERE TOO, RETURNING DTO BETTER 
            return Ok(product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(Guid productId, ProductUpdateDto updateDto)
        {
            var product = await _context.Products.FindAsync(productId);

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

        [HttpGet("{productId}/items")]
        public async Task<IActionResult> GetProductItems(Guid productId)
        {
            //Just check if the product exists without actually returning the product entity just true/false
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
            {
                return NotFound();
            }

            //Query the inventoryitems table filtering on product id
            //Note: Added ProductID as a property in addition to the Proudct navigation property in the InventoryItem entity to allow filtering on product id without needing SQL Join
            var items = await _context.InventoryItems
                // .Where(i => i.Product.Id == id)
                .Where(i => i.ProductId == productId)
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

        [HttpGet("{productId}/items/{itemId}")]
        public async Task<IActionResult> GetInventoryItem(Guid productId, Guid itemId)
        {
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ProductId == productId);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            return Ok(inventoryItem);
        }

        [HttpPut("{productId}/items/{itemId}")]
        public async Task<IActionResult> UpdateInventoryItem(Guid productId, Guid itemId, InventoryItemsDto itemDto)
        {

            //Just directly query the inventoryitems table filtering on item id and product id
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ProductId == productId);

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

        [HttpDelete("{productId}/items/{itemId}")]
        public async Task<IActionResult> DeleteInventoryItem(Guid productId, Guid itemId)
        {
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ProductId == productId);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{productId}/items")]
        public async Task<IActionResult> CreateInventoryItem(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var inventoryItem = new InventoryItem
            {
                ProductId = productId,
                Product = product,
                SerialNumber = $"AUTO-{Guid.NewGuid():N}",
                Condition = Condition.New,
                Status = Status.Available,
                ConditionNotes = "New item",
                MaintenanceHistory = "No maintenance history"
            };
            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();

            // return CreatedAtAction(
            //     nameof(GetInventoryItem),
            //     new { productId, itemId = inventoryItem.Id },
            //     null);

            return StatusCode(StatusCodes.Status201Created);
        }


        
    }
}
