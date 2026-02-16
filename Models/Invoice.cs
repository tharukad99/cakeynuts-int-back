using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string InvoiceNo { get; set; } = ""; // e.g. INV-20260213-0001

    [MaxLength(120)]
    public string CustomerName { get; set; } = "";

    [MaxLength(30)]
    public string? CustomerPhone { get; set; }

    [MaxLength(160)]
    public string? CustomerEmail { get; set; }

    public DateTime InvoiceDateUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Cancelled

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }       // optional
    public decimal Tax { get; set; }            // optional (VAT etc.)
    public decimal Total { get; set; }

    [MaxLength(400)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<InvoiceLine> Lines { get; set; } = new();
}
