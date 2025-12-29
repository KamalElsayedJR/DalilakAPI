using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class EditRegistercycle2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailOtpTokens_Users_UserId",
                table: "EmailOtpTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtpTokens_UserId",
                table: "EmailOtpTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmailOtpTokens");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "EmailOtpTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtpTokens_UserId",
                table: "EmailOtpTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOtpTokens_Users_UserId",
                table: "EmailOtpTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
