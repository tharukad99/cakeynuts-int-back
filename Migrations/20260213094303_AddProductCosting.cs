using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeyNuts.backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Servings = table.Column<int>(type: "int", nullable: false),
                    IngredientsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PackagingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LabourCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverheadCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WastagePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WastageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricingPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LabelPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Profit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProfitMarginPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCostingLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCostingId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostingLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCostingLines_ProductCostings_ProductCostingId",
                        column: x => x.ProductCostingId,
                        principalTable: "ProductCostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostingLines_ProductCostingId",
                table: "ProductCostingLines",
                column: "ProductCostingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCostingLines");

            migrationBuilder.DropTable(
                name: "ProductCostings");
        }
    }
}
