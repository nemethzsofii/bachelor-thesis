using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class CHangeColumnNameInTransactions2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Transactions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Transactions",
                newName: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Transactions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Transactions",
                newName: "category_id");
        }
    }
}
