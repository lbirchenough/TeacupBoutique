using inventory.Data;
using inventory.Entities;
using inventory.Models;
using inventory.Services;
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
                Colour = dto.Colour,
                Price = dto.Price,
                Contents = null,
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
            var products = await _context.Products
                .Include(p => p.SetItems!.Where(si => si.IsActive))
                .Include(p => p.Photos)
                .AsNoTracking()
                .ToListAsync();

            return Ok(products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Colour = p.Colour,
                Price = p.Price,
                DepositAmount = p.SetItems!.Sum(si => si.Quantity * si.DepositValuePerUnit),
                Servings = p.Servings,
                Contents = p.SetItems!.Any() ? string.Join('\n', p.SetItems!.Select(si => $"{si.Quantity} x {si.Name}")) : null,
                FeaturedPhotoUrl = p.Photos?
                    .Where(ph => ph.IsFeatured)
                    .Select(ph => ph.Url)
                    .FirstOrDefault(),
                Photos = p.Photos?
                    .OrderBy(ph => !ph.IsFeatured)
                    .ThenBy(ph => ph.DisplayOrder)
                    .Select(ph => new ProductPhotoDto
                    {
                        Id = ph.Id,
                        Url = ph.Url,
                        IsFeatured = ph.IsFeatured,
                        DisplayOrder = ph.DisplayOrder
                    }).ToList() ?? []
            }));
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            var product = await _context.Products
                .Where(p => p.Id == productId)
                .Include(p => p.SetItems!.Where(si => si.IsActive))
                .Include(p => p.Photos)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();

            return Ok(new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Contents = product.SetItems!.Any() ? string.Join('\n', product.SetItems!.Select(si => $"{si.Quantity} x {si.Name}")) : null,
                Colour = product.Colour,
                Price = product.Price,
                DepositAmount = product.SetItems!.Sum(si => si.Quantity * si.DepositValuePerUnit),
                Servings = product.Servings,
                MinRentalDays = product.MinRentalDays,
                MaxRentalDays = product.MaxRentalDays,
                BufferDays = product.BufferDays,
                IsActive = product.IsActive,
                Photos = product.Photos?.Select(ph => new ProductPhotoDto
                {
                    Id = ph.Id,
                    Url = ph.Url,
                    IsFeatured = ph.IsFeatured,
                    DisplayOrder = ph.DisplayOrder
                }).ToList() ?? []
            });
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(Guid productId, ProductUpdateDto updateDto)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.Colour = updateDto.Colour;
            product.Price = updateDto.Price;
            product.Servings = updateDto.Servings;
            product.MinRentalDays = updateDto.MinRentalDays;
            product.MaxRentalDays = updateDto.MaxRentalDays;
            product.BufferDays = updateDto.BufferDays;
            product.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{productId}/sets")]
        public async Task<IActionResult> GetProductSets(Guid productId)
        {
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
            {
                return NotFound();
            }

            var sets = await _context.ProductSets
                .Where(ps => ps.ProductId == productId)
                .Select(ps => new ProductSetDto
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    Status = ps.Status
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(sets);
        }

        [HttpPost("{productId}/sets")]
        public async Task<IActionResult> CreateProductSet(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var existingCount = await _context.ProductSets
                .CountAsync(ps => ps.ProductId == productId);

            var setLabel = (char)('A' + existingCount);
            var productSet = new ProductSet
            {
                ProductId = productId,
                Product = product,
                Name = $"Set {setLabel}",
                Status = Status.Available
            };

            _context.ProductSets.Add(productSet);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("{productId}/set-items")]
        public async Task<IActionResult> GetSetItems(Guid productId)
        {
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == productId);

            if (!productExists) return NotFound();

            var items = await _context.SetItems
                .Where(si => si.ProductId == productId)
                .Select(si => new
                {
                    si.Id,
                    si.Name,
                    si.Quantity,
                    si.DepositValuePerUnit,
                    SpareStock = si.SpareStock != null ? si.SpareStock.QuantityAvailable : 0,
                    si.IsActive
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("{productId}/set-items")]
        public async Task<IActionResult> CreateSetItem(Guid productId, [FromBody] SetItemCreateDto dto)
        {
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == productId);

            if (!productExists) return NotFound();

            var setItem = new SetItem
            {
                ProductId = productId,
                Name = dto.Name,
                Quantity = dto.Quantity,
                DepositValuePerUnit = dto.DepositValuePerUnit,
            };

            _context.SetItems.Add(setItem);

            var spareStock = new SpareStock
            {
                SetItemId = setItem.Id,
                QuantityAvailable = 0,
            };

            _context.SpareStocks.Add(spareStock);

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete("{productId}/set-items/{setItemId}")]
        public async Task<IActionResult> DeactivateSetItem(Guid productId, Guid setItemId)
        {
            var setItem = await _context.SetItems
                .FirstOrDefaultAsync(si => si.Id == setItemId && si.ProductId == productId);

            if (setItem is null) return NotFound();

            setItem.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{productId}/set-items/{setItemId}/activate")]
        public async Task<IActionResult> ActivateSetItem(Guid productId, Guid setItemId)
        {
            var setItem = await _context.SetItems
                .FirstOrDefaultAsync(si => si.Id == setItemId && si.ProductId == productId);

            if (setItem is null) return NotFound();

            setItem.IsActive = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] DateOnly date)
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    DepositAmount = p.SetItems!.Where(si => si.IsActive).Sum(si => si.Quantity * si.DepositValuePerUnit),
                    p.Servings,
                    FeaturedPhotoUrl = p.Photos
                        .Where(ph => ph.IsFeatured)
                        .Select(ph => ph.Url)
                        .FirstOrDefault(),
                    Photos = p.Photos
                        .OrderBy(ph => !ph.IsFeatured)
                        .ThenBy(ph => ph.DisplayOrder)
                        .Select(ph => new { ph.Id, ph.Url, ph.IsFeatured, ph.DisplayOrder })
                })
                .AsNoTracking()
                .ToListAsync();

            var totals = await _context.ProductSets
                .Where(ps => ps.Status != Status.Retired)
                .GroupBy(ps => ps.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Count() })
                .ToListAsync();

            var booked = await _context.BookingItems
                .Where(bi =>
                    bi.ReservationDate == date &&
                    bi.Booking!.Status != BookingStatus.Cancelled)
                .GroupBy(bi => bi.ProductId)
                .Select(g => new { ProductId = g.Key, Booked = g.Count() })
                .ToListAsync();

            var result = products.Select(p =>
            {
                int total = totals.FirstOrDefault(t => t.ProductId == p.Id)?.Total ?? 0;
                int bookedCount = booked.FirstOrDefault(b => b.ProductId == p.Id)?.Booked ?? 0;
                return new
                {
                    productId = p.Id,
                    name = p.Name,
                    featuredPhotoUrl = p.FeaturedPhotoUrl,
                    photos = p.Photos,
                    pricePerDay = p.Price,
                    depositAmount = p.DepositAmount,
                    servings = p.Servings,
                    available = Math.Max(0, total - bookedCount),
                    total
                };
            });

            return Ok(result);
        }

        [HttpPost("{productId}/photos")]
        public async Task<IActionResult> UploadPhoto(Guid productId, IFormFile file,
            [FromServices] ICloudinaryService cloudinary)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) return NotFound();

            try
            {
                var upload = await cloudinary.UploadAsync(file, $"products/{productId}");
                var hasPhotos = await _context.Photos.AnyAsync(ph => ph.ProductId == productId);
                var maxOrder = hasPhotos
                    ? await _context.Photos.Where(ph => ph.ProductId == productId).MaxAsync(ph => ph.DisplayOrder)
                    : 0;

                var photo = new Photo
                {
                    Url = upload.Url,
                    PublicId = upload.PublicId,
                    IsFeatured = !hasPhotos,
                    DisplayOrder = maxOrder + 1,
                    ProductId = productId,
                    Product = null!
                };

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return StatusCode(201, new ProductPhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsFeatured = photo.IsFeatured,
                    DisplayOrder = photo.DisplayOrder
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{productId}/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(Guid productId, Guid photoId,
            [FromServices] ICloudinaryService cloudinary,
            ILogger<ProductsController> logger)
        {
            var photo = await _context.Photos
                .FirstOrDefaultAsync(ph => ph.Id == photoId && ph.ProductId == productId);
            if (photo is null) return NotFound();

            if (photo.PublicId is not null)
            {
                try { await cloudinary.DeleteAsync(photo.PublicId); }
                catch (Exception ex) { logger.LogError(ex, "Failed to delete photo from Cloudinary: {PublicId}", photo.PublicId); }
            }

            var wasFeatured = photo.IsFeatured;
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            if (wasFeatured)
            {
                var next = await _context.Photos
                    .Where(ph => ph.ProductId == productId)
                    .OrderBy(ph => ph.DisplayOrder)
                    .FirstOrDefaultAsync();
                if (next is not null)
                {
                    next.IsFeatured = true;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        [HttpPut("{productId}/photos/{photoId}/featured")]
        public async Task<IActionResult> SetFeaturedPhoto(Guid productId, Guid photoId)
        {
            var photos = await _context.Photos
                .Where(ph => ph.ProductId == productId)
                .ToListAsync();

            if (!photos.Any(ph => ph.Id == photoId)) return NotFound();

            foreach (var ph in photos)
                ph.IsFeatured = ph.Id == photoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record SetItemCreateDto(string Name, int Quantity, decimal DepositValuePerUnit);
}
