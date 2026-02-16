using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class InventoryItemCreateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = "";
    
    [Required, MaxLength(40)]
    public string Unit { get; set; } = "pcs";

    public decimal QuantityOnHand { get; set; }
    public decimal CostPerUnit { get; set; }
}
