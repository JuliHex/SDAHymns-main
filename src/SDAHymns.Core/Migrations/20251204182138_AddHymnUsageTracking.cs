using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddHymnUsageTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessCount",
                table: "Hymns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Hymns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccessedAt",
                table: "Hymns",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessCount",
                table: "Hymns");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Hymns");

            migrationBuilder.DropColumn(
                name: "LastAccessedAt",
                table: "Hymns");
        }
    }
}
