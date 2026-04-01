using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceFaviconUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "Sources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FaviconUrl", "LogoUrl" },
                values: new object[] { "https://www.kariyer.net/favicon.ico", "/images/kariyer-logo.webp" });

            migrationBuilder.UpdateData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FaviconUrl", "LogoUrl" },
                values: new object[] { "https://isinolsun.com/favicon.ico", "https://isinolsun-next.mncdn.com/_next/static/images/logo.png" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "Sources");

            migrationBuilder.UpdateData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogoUrl",
                value: "https://www.kariyer.net/favicon.ico");

            migrationBuilder.UpdateData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 2,
                column: "LogoUrl",
                value: "https://isinolsun.com/favicon.ico");
        }
    }
}
