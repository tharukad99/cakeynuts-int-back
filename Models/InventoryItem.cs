using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Models;

public class InventoryItem
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(40)]
    public string Unit { get; set; } = "pcs"; // e.g., g, kg, ml, pcs

    [Range(0, double.MaxValue)]
    public decimal QuantityOnHand { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CostPerUnit { get; set; }  // for cost calculator

    // Pending / Next Batch Logic
    public decimal? NewCostPerUnit { get; set; }
    public decimal? NewQty { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
