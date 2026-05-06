using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRemoteWidgetSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RemoteWidgetSettingsJson",
                table: "AppSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "RemoteWidgetSettingsJson",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemoteWidgetSettingsJson",
                table: "AppSettings");
        }
    }
}
