using System.Security.Claims;
using EcomApi.Data;
using EcomApi.Dtos;
using EcomApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext db) : ControllerBase
{
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")!.Value);

    // الحالات المسموحة للطلب
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Pending", "Paid", "Cancelled" };

    // POST: /api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> Create(CreateOrderDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return BadRequest("No items");

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest("Some products not found");

        var order = new Order { UserId = GetUserId() };

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest("Invalid quantity");

            var prod = products.First(p => p.Id == item.ProductId);
            if (prod.StockQty < item.Quantity)
                return BadRequest($"Insufficient stock for {prod.Name}");

            order.Items.Add(new OrderItem
            {
                ProductId = prod.Id,
                Quantity = item.Quantity,
                UnitPrice = prod.Price
            });

            // خصم من المخزون
            prod.StockQty -= item.Quantity;
        }

        // حساب الإجمالي
        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    // GET: /api/orders/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> Get(int id)
    {
        var userId = GetUserId();
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        return order is null ? NotFound() : order;
    }

    // GET: /api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> MyOrders()
    {
        var userId = GetUserId();
        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return orders;
    }

    // PATCH: /api/orders/5/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status) || !AllowedStatuses.Contains(dto.Status))
            return BadRequest("Invalid status. Allowed: Pending, Paid, Cancelled");

        var userId = GetUserId();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null) return NotFound();

        order.Status = dto.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
