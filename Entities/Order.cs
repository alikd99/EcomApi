namespace EcomApi.Entities;

public class Order
{
    public int Id { get; set; }

    // علاقة مع المستخدم
    public int UserId { get; set; }
    public User? User { get; set; }

    // عناصر الطلب
    public List<OrderItem> Items { get; set; } = new();

    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled
    public string? PaymentIntentId { get; set; }     // للربط مع Stripe
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
