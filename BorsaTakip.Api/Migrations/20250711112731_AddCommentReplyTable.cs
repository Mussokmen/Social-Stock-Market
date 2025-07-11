using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorsaTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentReplyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "CommentReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoinCommentId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReplies_CoinComments_CoinCommentId",
                        column: x => x.CoinCommentId,
                        principalTable: "CoinComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentReplies_CoinCommentId",
                table: "CommentReplies",
                column: "CoinCommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReplies");

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
    }
}
