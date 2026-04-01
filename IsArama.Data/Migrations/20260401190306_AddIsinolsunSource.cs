using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsinolsunSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "Id", "BaseUrl", "LogoUrl", "Name" },
                values: new object[] { 2, "https://isinolsun.com", "https://isinolsun.com/favicon.ico", "isinolsun.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
