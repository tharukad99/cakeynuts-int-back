using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Models;

public class InvoiceLine
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    [Required, MaxLength(160)]
    public string ItemName { get; set; } = ""; // e.g. "Chocolate Bento Cake"

    [MaxLength(40)]
    public string Unit { get; set; } = "pcs"; // pcs, box, kg, etc.

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }     // Quantity * UnitPrice
}
