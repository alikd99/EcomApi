namespace EcomApi.Dtos;

public record OrderItemDto(int ProductId, int Quantity);

public record CreateOrderDto(List<OrderItemDto> Items);

public record UpdateOrderStatusDto(string Status);
