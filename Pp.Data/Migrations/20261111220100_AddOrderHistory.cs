using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pp.Data.Context;

namespace Pp.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261111220100_AddOrderHistory")]
public sealed class AddOrderHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CouponCode = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                CouponDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                PointsUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                InsertDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                InsertUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Orders", order => order.Id));

        migrationBuilder.CreateIndex("IX_Orders_UserId_Id", "Orders", new[] { "UserId", "Id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Orders");
    }
}
