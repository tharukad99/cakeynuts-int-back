using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class InvoiceCreateDto
{
    [MaxLength(120)]
    public string CustomerName { get; set; } = "";

    [MaxLength(30)]
    public string? CustomerPhone { get; set; }

    [MaxLength(160)]
    public string? CustomerEmail { get; set; }

    public decimal Discount { get; set; } = 0;
    public decimal Tax { get; set; } = 0;

    [MaxLength(400)]
    public string? Notes { get; set; }

    [Required]
    public List<InvoiceLineCreateDto> Lines { get; set; } = new();
}

public class InvoiceLineCreateDto
{
    [Required, MaxLength(160)]
    public string ItemName { get; set; } = "";

    [MaxLength(40)]
    public string Unit { get; set; } = "pcs";

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
