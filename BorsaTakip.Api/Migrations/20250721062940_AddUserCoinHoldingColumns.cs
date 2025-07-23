using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorsaTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCoinHoldingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserCoinHoldings",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "CoinId",
                table: "UserCoinHoldings",
                newName: "CoinName");

            migrationBuilder.RenameColumn(
                name: "BuyPrice",
                table: "UserCoinHoldings",
                newName: "PurchasePrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "UserCoinHoldings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "UserCoinHoldings",
                newName: "BuyPrice");

            migrationBuilder.RenameColumn(
                name: "CoinName",
                table: "UserCoinHoldings",
                newName: "CoinId");
        }
    }
}
