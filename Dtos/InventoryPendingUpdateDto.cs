using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class InventoryPendingUpdateDto
{
    [Range(0.01, double.MaxValue)]
    public decimal NewCostPerUnit { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal NewQty { get; set; }
}
