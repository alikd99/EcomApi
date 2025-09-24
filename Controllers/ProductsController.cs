using EcomApi.Data;
using EcomApi.Dtos;
using EcomApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET: /api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        => await db.Products.OrderByDescending(p => p.Id).ToListAsync();

    // GET: /api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Get(int id)
        => await db.Products.FindAsync(id) is { } p ? p : NotFound();

    // POST: /api/products
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");
        if (dto.Price < 0) return BadRequest("Price must be >= 0");
        if (dto.StockQty < 0) return BadRequest("StockQty must be >= 0");

        var p = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQty = dto.StockQty
        };

        db.Products.Add(p);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    // PUT: /api/products/5
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");
        if (dto.Price < 0) return BadRequest("Price must be >= 0");
        if (dto.StockQty < 0) return BadRequest("StockQty must be >= 0");

        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.StockQty = dto.StockQty;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/products/5
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();

        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
