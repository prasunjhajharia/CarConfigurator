using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarConfigurator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShareCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShareCode",
                table: "SavedConfigurations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SavedConfigurations_ShareCode",
                table: "SavedConfigurations",
                column: "ShareCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SavedConfigurations_ShareCode",
                table: "SavedConfigurations");

            migrationBuilder.DropColumn(
                name: "ShareCode",
                table: "SavedConfigurations");
        }
    }
}
