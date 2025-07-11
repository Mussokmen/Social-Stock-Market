using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorsaTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddParentCommentIdToCoinComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "CoinComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinComments_ParentCommentId",
                table: "CoinComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoinComments_CoinComments_ParentCommentId",
                table: "CoinComments",
                column: "ParentCommentId",
                principalTable: "CoinComments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoinComments_CoinComments_ParentCommentId",
                table: "CoinComments");

            migrationBuilder.DropIndex(
                name: "IX_CoinComments_ParentCommentId",
                table: "CoinComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "CoinComments");
        }
    }
}
