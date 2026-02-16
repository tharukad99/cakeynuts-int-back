using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class InventoryStockOutDto
{
    // If referencing by ID in URL, we don't strictly need Id here, but helpful for validation
    
    [Range(0.0001, double.MaxValue)]
    public decimal QuantityUsed { get; set; }

    // Optional: Unit handling if we want the backend to do conversion
    public string? Unit { get; set; }
}
