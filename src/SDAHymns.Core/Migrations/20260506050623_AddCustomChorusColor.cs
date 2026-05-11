using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomChorusColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomChorusColor",
                table: "DisplayProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EnableCustomChorusStyling",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CustomChorusColor", "EnableCustomChorusStyling" },
                values: new object[] { "#FFD700", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomChorusColor",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "EnableCustomChorusStyling",
                table: "DisplayProfiles");
        }
    }
}
