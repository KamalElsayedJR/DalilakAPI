using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    public partial class editAddFavoritesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CarPrice",
                table: "Favorites",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "CarOrigin",
                table: "Favorites",
                newName: "Origin");

            migrationBuilder.RenameColumn(
                name: "CarName",
                table: "Favorites",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CarEngine",
                table: "Favorites",
                newName: "Engine");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_UserId_CarName",
                table: "Favorites",
                newName: "IX_Favorites_UserId_Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Favorites",
                newName: "CarPrice");

            migrationBuilder.RenameColumn(
                name: "Origin",
                table: "Favorites",
                newName: "CarOrigin");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Favorites",
                newName: "CarName");

            migrationBuilder.RenameColumn(
                name: "Engine",
                table: "Favorites",
                newName: "CarEngine");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_UserId_Name",
                table: "Favorites",
                newName: "IX_Favorites_UserId_CarName");
        }
    }
}
