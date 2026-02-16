public class CostingCreateDto
{
    public string ProductName { get; set; } = "";
    public int Servings { get; set; } = 1;

    public List<CostingLineDto> Lines { get; set; } = new();

    public decimal PackagingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal OverheadCost { get; set; }

    public decimal WastagePercent { get; set; } // e.g. 5

    public string PricingMethod { get; set; } = "Markup"; // Markup or Margin
    public decimal PricingPercent { get; set; } // e.g. 40
}

public class CostingLineDto
{
    public string ItemName { get; set; } = "";
    public string Unit { get; set; } = "g";
    public decimal Quantity { get; set; }
    public decimal CostPerUnit { get; set; }
}
