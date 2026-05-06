using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralDisplaySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAspectRatio43",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsAspectRatio43", "Language", "Theme" },
                values: new object[] { true, "ro", "Dark" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAspectRatio43",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "AppSettings");
        }
    }
}
