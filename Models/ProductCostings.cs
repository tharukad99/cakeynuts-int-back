public class ProductCosting
{
    public int Id { get; set; }

    public string ProductName { get; set; } = "";
    public int Servings { get; set; } = 1;

    public decimal IngredientsCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal OverheadCost { get; set; }

    public decimal WastagePercent { get; set; } // e.g. 5 means 5%
    public decimal WastageCost { get; set; }

    public decimal TotalCost { get; set; }

    // Pricing input
    public string PricingMethod { get; set; } = "Markup"; // "Markup" or "Margin"
    public decimal PricingPercent { get; set; } // e.g. 40 means 40%

    // Pricing outputs
    public decimal LabelPrice { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMarginPercent { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<ProductCostingLine> Lines { get; set; } = new();
}

public class ProductCostingLine
{
    public int Id { get; set; }

    public int ProductCostingId { get; set; }
    public ProductCosting ProductCosting { get; set; } = null!;

    public string ItemName { get; set; } = "";  // e.g., Flour
    public string Unit { get; set; } = "g";     // g, ml, pcs
    public decimal Quantity { get; set; }       // e.g., 500
    public decimal CostPerUnit { get; set; }    // e.g., 0.002 per g

    public decimal LineCost { get; set; }       // Quantity * CostPerUnit
}
