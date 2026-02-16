using CakeyNuts.Api.Data;
using CakeyNuts.Api.Dtos;
using CakeyNuts.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CakeyNuts.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public InventoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.InventoryItems
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Unit,
                x.QuantityOnHand,
                x.CostPerUnit,
                x.NewCostPerUnit,
                x.NewQty,
                x.CreatedUtc
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryItemCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var entity = new InventoryItem
        {
            Name = dto.Name.Trim(),
            Unit = dto.Unit.Trim(),
            QuantityOnHand = dto.QuantityOnHand,
            CostPerUnit = dto.CostPerUnit
        };

        _db.InventoryItems.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, new { entity.Id });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] InventoryAdjustmentDto dto)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Name == dto.ItemName);
        if (item == null) return NotFound(new { message = $"Item '{dto.ItemName}' not found." });

        decimal quantityToDeduct = dto.Quantity;
        
        // Basic Unit Conversion Logic
        var master = item.Unit.Trim().ToLower();
        var usage = dto.Unit.Trim().ToLower();

        if (master != usage)
        {
             if (master == "kg" && usage == "g") quantityToDeduct /= 1000m;
             else if (master == "l" && usage == "ml") quantityToDeduct /= 1000m;
             else if (master == "g" && usage == "kg") quantityToDeduct *= 1000m;
             else if (master == "ml" && usage == "l") quantityToDeduct *= 1000m;
             // Add more if needed. If units are completely different (e.g. pcs vs kg), just use raw value.
        }

        // We now use the new StockOut logic instead of this simple AdjustStock when ID is known.
        // But for compatibility with frontend "ByName" call:
        return await StockOutLogic(item, quantityToDeduct);
    }

    [HttpPut("{id:int}/pending")]
    public async Task<IActionResult> SetPending(int id, [FromBody] InventoryPendingUpdateDto dto)
    {
        var item = await _db.InventoryItems.FindAsync(id);
        if (item == null) return NotFound();

        item.NewCostPerUnit = dto.NewCostPerUnit;
        item.NewQty = dto.NewQty;

        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPost("{id:int}/stockout")]
    public async Task<IActionResult> StockOut(int id, [FromBody] InventoryStockOutDto dto)
    {
        var item = await _db.InventoryItems.FindAsync(id);
        if (item == null) return NotFound();
        
        // Handle unit conversion if 'dto.Unit' is provided, otherwise assume base unit
        decimal quantityToDeduct = dto.QuantityUsed;
        
        if (!string.IsNullOrEmpty(dto.Unit)) {
             var master = item.Unit.Trim().ToLower();
             var usage = dto.Unit.Trim().ToLower();
             if (master != usage) {
                 if (master == "kg" && usage == "g") quantityToDeduct /= 1000m;
                 else if (master == "l" && usage == "ml") quantityToDeduct /= 1000m;
                 else if (master == "g" && usage == "kg") quantityToDeduct *= 1000m;
                 else if (master == "ml" && usage == "l") quantityToDeduct *= 1000m;
             }
        }

        return await StockOutLogic(item, quantityToDeduct);
    }

    private async Task<IActionResult> StockOutLogic(InventoryItem item, decimal quantityToDeduct)
    {
        // 1. Deduct Stock
        item.QuantityOnHand -= quantityToDeduct;
        await _db.SaveChangesAsync(); // Save the deduction first

        // 2. Check Promotion Condition (Atomic Logic)
        // If stock <= 0 and we have pending values, promote them.
        // The user asked for a Stored Procedure call here or equivalent transaction logic.
        // We will do it in a transaction block for safety.
        
        if (item.QuantityOnHand <= 0 && item.NewQty.HasValue && item.NewCostPerUnit.HasValue)
        {
            // Begin Transaction to ensure atomicity of promotion
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Reload to be safe inside transaction (though we just saved)
                // In high concurrency, we might want to lock, but standard EF logic:
                // We already have 'item' tracked.
                
                // Promotion
                // We add the current negative balance to the new quantity to deduct the overflow from the new batch
                item.QuantityOnHand = item.NewQty.Value + item.QuantityOnHand; 
                item.CostPerUnit = item.NewCostPerUnit.Value;
                
                // Clear pending
                item.NewQty = null;
                item.NewCostPerUnit = null;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return Ok(new 
        { 
            item.Id, 
            item.Name, 
            Stock = item.QuantityOnHand, 
            PendingStock = item.NewQty,
            item.CostPerUnit 
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _db.InventoryItems.FindAsync(id);
        if (x == null) return NotFound();

        _db.InventoryItems.Remove(x);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
