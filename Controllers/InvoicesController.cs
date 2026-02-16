using CakeyNuts.Api.Data;
using CakeyNuts.Api.Dtos;
using CakeyNuts.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CakeyNuts.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public InvoicesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
    {
        if (dto.Lines == null || dto.Lines.Count == 0)
            return BadRequest(new { message = "Invoice must have at least 1 line item." });

        // Build lines + totals
        var lines = dto.Lines.Select(l =>
        {
            if (l.Quantity <= 0) throw new ArgumentException("Quantity must be > 0");
            if (l.UnitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative");

            var lineTotal = l.Quantity * l.UnitPrice;

            return new InvoiceLine
            {
                ItemName = l.ItemName.Trim(),
                Unit = (l.Unit ?? "pcs").Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = lineTotal
            };
        }).ToList();

        var subtotal = lines.Sum(x => x.LineTotal);
        var discount = dto.Discount < 0 ? 0 : dto.Discount;
        var tax = dto.Tax < 0 ? 0 : dto.Tax;

        var total = subtotal - discount + tax;
        if (total < 0) total = 0;

        var invoiceNo = await GenerateInvoiceNoAsync();

        var invoice = new Invoice
        {
            InvoiceNo = invoiceNo,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone?.Trim(),
            CustomerEmail = dto.CustomerEmail?.Trim(),
            Status = "Issued",
            Subtotal = subtotal,
            Discount = discount,
            Tax = tax,
            Total = total,
            Notes = dto.Notes?.Trim(),
            Lines = lines
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, new
        {
            invoice.Id,
            invoice.InvoiceNo,
            invoice.Total
        });
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.Invoices
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.InvoiceNo,
                x.CustomerName,
                x.Status,
                x.Subtotal,
                x.Discount,
                x.Tax,
                x.Total,
                x.CreatedUtc
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Invoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        return Ok(new
        {
            inv.Id,
            inv.InvoiceNo,
            inv.CustomerName,
            inv.CustomerPhone,
            inv.CustomerEmail,
            inv.InvoiceDateUtc,
            inv.Status,
            inv.Subtotal,
            inv.Discount,
            inv.Tax,
            inv.Total,
            inv.Notes,
            inv.CreatedUtc,
            Lines = inv.Lines.Select(l => new
            {
                l.Id,
                l.ItemName,
                l.Unit,
                l.Quantity,
                l.UnitPrice,
                l.LineTotal
            })
        });
    }

    // Simple invoice number generator (good enough for single-server dev)
    private async Task<string> GenerateInvoiceNoAsync()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

        var todayCount = await _db.Invoices.CountAsync(x =>
            x.CreatedUtc >= DateTime.UtcNow.Date && x.CreatedUtc < DateTime.UtcNow.Date.AddDays(1));

        var seq = todayCount + 1;
        return $"INV-{datePart}-{seq:0000}";
    }
}
