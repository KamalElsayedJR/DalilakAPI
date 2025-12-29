using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class edit2AddFavoritesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_Name",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Engine",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Favorites");

            migrationBuilder.AddColumn<string>(
                name: "CarJson",
                table: "Favorites",
                type: "NVARCHAR(MAX)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarJson",
                table: "Favorites");

            migrationBuilder.AddColumn<string>(
                name: "Engine",
                table: "Favorites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Favorites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "Favorites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Favorites",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_Name",
                table: "Favorites",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }
    }
}
