namespace EcomApi.Dtos;

public record ProductCreateDto(string Name, string? Description, decimal Price, int StockQty);
public record ProductUpdateDto(string Name, string? Description, decimal Price, int StockQty);