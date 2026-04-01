using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "JobListings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "Id", "BaseUrl", "LogoUrl", "Name" },
                values: new object[] { 1, "https://www.kariyer.net", "https://www.kariyer.net/favicon.ico", "kariyer.net" });

            migrationBuilder.CreateIndex(
                name: "IX_JobListings_SourceId",
                table: "JobListings",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobListings_Sources_SourceId",
                table: "JobListings",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobListings_Sources_SourceId",
                table: "JobListings");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_JobListings_SourceId",
                table: "JobListings");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "JobListings");
        }
    }
}
