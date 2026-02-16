using CakeyNuts.Api.Data;
using CakeyNuts.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CakeyNuts.Api.Controllers;

[ApiController]
[Route("api/v1/costings")]
[Authorize]
public class CostingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CostingsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CostingCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ProductName))
            return BadRequest(new { message = "ProductName is required." });

        if (dto.Lines == null || dto.Lines.Count == 0)
            return BadRequest(new { message = "At least one line item is required." });

        // Build lines
        var lines = dto.Lines.Select(l =>
        {
            var lineCost = l.Quantity * l.CostPerUnit;
            return new ProductCostingLine
            {
                ItemName = l.ItemName.Trim(),
                Unit = l.Unit.Trim(),
                Quantity = l.Quantity,
                CostPerUnit = l.CostPerUnit,
                LineCost = lineCost
            };
        }).ToList();

        var ingredientsCost = lines.Sum(x => x.LineCost);

        var baseCost = ingredientsCost + dto.PackagingCost + dto.LabourCost + dto.OverheadCost;
        var wastageCost = baseCost * (dto.WastagePercent / 100m);
        var totalCost = baseCost + wastageCost;

        // Pricing
        var method = (dto.PricingMethod ?? "Markup").Trim();
        var percent = dto.PricingPercent;

        if (percent < 0) return BadRequest(new { message = "PricingPercent cannot be negative." });

        decimal labelPrice;
        if (method.Equals("Margin", StringComparison.OrdinalIgnoreCase))
        {
            // totalCost / (1 - margin)
            var margin = percent / 100m;
            if (margin >= 1m) return BadRequest(new { message = "Margin must be < 100%." });

            labelPrice = totalCost / (1m - margin);
        }
        else
        {
            // Markup
            labelPrice = totalCost * (1m + (percent / 100m));
            method = "Markup";
        }

        // Optional: nice pricing rounding (e.g., £24.99 style)
        labelPrice = RoundToPsychologicalPrice(labelPrice);

        var profit = labelPrice - totalCost;
        var profitMarginPercent = labelPrice == 0 ? 0 : (profit / labelPrice) * 100m;

        var entity = new ProductCosting
        {
            ProductName = dto.ProductName.Trim(),
            Servings = dto.Servings <= 0 ? 1 : dto.Servings,

            IngredientsCost = ingredientsCost,
            PackagingCost = dto.PackagingCost,
            LabourCost = dto.LabourCost,
            OverheadCost = dto.OverheadCost,

            WastagePercent = dto.WastagePercent,
            WastageCost = wastageCost,

            TotalCost = totalCost,

            PricingMethod = method,
            PricingPercent = percent,

            LabelPrice = labelPrice,
            Profit = profit,
            ProfitMarginPercent = profitMarginPercent,

            Lines = lines
        };

        _db.ProductCostings.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new
        {
            entity.Id,
            entity.ProductName,
            entity.TotalCost,
            entity.LabelPrice,
            entity.Profit,
            entity.ProfitMarginPercent
        });
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.ProductCostings
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.ProductName,
                x.TotalCost,
                x.LabelPrice,
                x.Profit,
                x.ProfitMarginPercent,
                x.CreatedUtc
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var x = await _db.ProductCostings
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (x == null) return NotFound();

        return Ok(new
        {
            x.Id,
            x.ProductName,
            x.Servings,
            x.IngredientsCost,
            x.PackagingCost,
            x.LabourCost,
            x.OverheadCost,
            x.WastagePercent,
            x.WastageCost,
            x.TotalCost,
            x.PricingMethod,
            x.PricingPercent,
            x.LabelPrice,
            x.Profit,
            x.ProfitMarginPercent,
            x.CreatedUtc,
            Lines = x.Lines.Select(l => new
            {
                l.Id,
                l.ItemName,
                l.Unit,
                l.Quantity,
                l.CostPerUnit,
                l.LineCost
            })
        });
    }

    private static decimal RoundToPsychologicalPrice(decimal value)
    {
        // Example: 24.12 -> 24.99, 25.01 -> 25.99
        // You can change this rule later.
        var roundedUp = Math.Ceiling(value);
        return roundedUp - 0.01m;
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CostingCreateDto dto)
    {
        var entity = await _db.ProductCostings
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (entity == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.ProductName))
            return BadRequest(new { message = "ProductName is required." });

        if (dto.Lines == null || dto.Lines.Count == 0)
            return BadRequest(new { message = "At least one line item is required." });

        // Update basic fields
        entity.ProductName = dto.ProductName.Trim();
        entity.Servings = dto.Servings <= 0 ? 1 : dto.Servings;
        entity.PackagingCost = dto.PackagingCost;
        entity.LabourCost = dto.LabourCost;
        entity.OverheadCost = dto.OverheadCost;
        entity.WastagePercent = dto.WastagePercent;
        entity.PricingMethod = (dto.PricingMethod ?? "Markup").Trim();
        entity.PricingPercent = dto.PricingPercent;

        // Recalculate Lines
        // Remove old lines
        _db.ProductCostingLines.RemoveRange(entity.Lines);
        
        // Add new lines
        var newLines = dto.Lines.Select(l =>
        {
            var lineCost = l.Quantity * l.CostPerUnit;
            return new ProductCostingLine
            {
                ProductCostingId = entity.Id, // Link to parent
                ItemName = l.ItemName.Trim(),
                Unit = l.Unit.Trim(),
                Quantity = l.Quantity,
                CostPerUnit = l.CostPerUnit,
                LineCost = lineCost
            };
        }).ToList();

        entity.Lines = newLines; // EF Core will handle insertion

        // Recalculate Totals
        var ingredientsCost = newLines.Sum(x => x.LineCost);
        var baseCost = ingredientsCost + dto.PackagingCost + dto.LabourCost + dto.OverheadCost;
        var wastageCost = baseCost * (dto.WastagePercent / 100m);
        var totalCost = baseCost + wastageCost;

        // Determine Price
        decimal labelPrice;
        if (entity.PricingMethod.Equals("Margin", StringComparison.OrdinalIgnoreCase))
        {
            var margin = entity.PricingPercent / 100m;
            if (margin >= 1m) return BadRequest(new { message = "Margin must be < 100%." });
            labelPrice = totalCost / (1m - margin);
        }
        else
        {
            labelPrice = totalCost * (1m + (entity.PricingPercent / 100m));
            entity.PricingMethod = "Markup";
        }

        labelPrice = RoundToPsychologicalPrice(labelPrice);
        var profit = labelPrice - totalCost;
        var profitMarginPercent = labelPrice == 0 ? 0 : (profit / labelPrice) * 100m;

        // Update Entity Calculated Fields
        entity.IngredientsCost = ingredientsCost;
        entity.WastageCost = wastageCost;
        entity.TotalCost = totalCost;
        entity.LabelPrice = labelPrice;
        entity.Profit = profit;
        entity.ProfitMarginPercent = profitMarginPercent;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            entity.Id,
            entity.ProductName,
            entity.TotalCost,
            entity.LabelPrice,
            entity.Profit
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _db.ProductCostings.FindAsync(id);
        if (x == null) return NotFound();

        _db.ProductCostings.Remove(x);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
