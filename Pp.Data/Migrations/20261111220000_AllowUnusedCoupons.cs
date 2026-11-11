using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pp.Data.Context;

namespace Pp.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261111220000_AllowUnusedCoupons")]
public sealed class AllowUnusedCoupons : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        DropDependencies(migrationBuilder);
        migrationBuilder.AlterColumn<long>(
            name: "UsedByUserId",
            table: "Coupons",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");
        RestoreDependencies(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM [Coupons] WHERE [UsedByUserId] IS NULL)
                THROW 51000, 'Unused coupons must be resolved before reverting this migration.', 1;
            """);
        DropDependencies(migrationBuilder);
        migrationBuilder.AlterColumn<long>(
            name: "UsedByUserId",
            table: "Coupons",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);
        RestoreDependencies(migrationBuilder);
    }

    private static void DropDependencies(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Coupons_Users_UsedByUserId", "Coupons");
        migrationBuilder.DropIndex("IX_Coupons_UsedByUserId", "Coupons");
    }

    private static void RestoreDependencies(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex("IX_Coupons_UsedByUserId", "Coupons", "UsedByUserId");
        migrationBuilder.AddForeignKey("FK_Coupons_Users_UsedByUserId", "Coupons", "UsedByUserId",
            "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }
}
