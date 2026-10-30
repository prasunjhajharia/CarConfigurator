using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarConfigurator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrimImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Trims",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Trims");
        }
    }
}
