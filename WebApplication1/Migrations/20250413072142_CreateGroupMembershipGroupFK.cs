using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class CreateGroupMembershipGroupFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_Groups_GroupId",
                table: "GroupMemberships",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_Groups_GroupId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships");
        }
    }
}
