using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCShop.Migrations
{
    /// <inheritdoc />
    public partial class addOriginalImageAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "OriginalImage",
                table: "Products",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalImage",
                table: "Products");
        }
    }
}
