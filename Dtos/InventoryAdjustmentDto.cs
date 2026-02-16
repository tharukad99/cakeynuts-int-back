using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class InventoryAdjustmentDto
{
    [Required]
    public string ItemName { get; set; } = "";
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "pcs";
}
