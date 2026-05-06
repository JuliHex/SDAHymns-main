using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioPlaybackSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings");

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Key",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AppSettings",
                newName: "AudioLibraryPath");

            migrationBuilder.AddColumn<string>(
                name: "TimingMapJson",
                table: "AudioRecordings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VolumeOffset",
                table: "AudioRecordings",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "ActiveDisplayProfileId",
                table: "AppSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioAutoPlayDelay",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AudioOutputDeviceId",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoAdvanceEnabled",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "GlobalVolume",
                table: "AppSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "LastWindowPosition",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastWindowSize",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSettingsKeyValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettingsKeyValue", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActiveDisplayProfileId", "AudioAutoPlayDelay", "AudioLibraryPath", "AudioOutputDeviceId", "AutoAdvanceEnabled", "GlobalVolume", "LastWindowPosition", "LastWindowSize" },
                values: new object[] { 1, 5, null, null, false, 0.8f, null, null });

            migrationBuilder.InsertData(
                table: "AppSettingsKeyValue",
                columns: new[] { "Id", "CreatedAt", "Description", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Last time legacy XML was imported", "LastDatabaseImportDate", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { 2, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Default display profile ID", "DefaultDisplayProfileId", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "1" },
                    { 3, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Track hymn usage statistics", "EnableStatisticsTracking", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_ActiveDisplayProfileId",
                table: "AppSettings",
                column: "ActiveDisplayProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettingsKeyValue_Key",
                table: "AppSettingsKeyValue",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_DisplayProfiles_ActiveDisplayProfileId",
                table: "AppSettings",
                column: "ActiveDisplayProfileId",
                principalTable: "DisplayProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_DisplayProfiles_ActiveDisplayProfileId",
                table: "AppSettings");

            migrationBuilder.DropTable(
                name: "AppSettingsKeyValue");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_ActiveDisplayProfileId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TimingMapJson",
                table: "AudioRecordings");

            migrationBuilder.DropColumn(
                name: "VolumeOffset",
                table: "AudioRecordings");

            migrationBuilder.DropColumn(
                name: "ActiveDisplayProfileId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AudioAutoPlayDelay",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AudioOutputDeviceId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AutoAdvanceEnabled",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GlobalVolume",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "LastWindowPosition",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "LastWindowSize",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "AudioLibraryPath",
                table: "AppSettings",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Last time legacy XML was imported", "LastDatabaseImportDate", "" });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "CreatedAt", "Description", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 2, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Default display profile ID", "DefaultDisplayProfileId", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "1" },
                    { 3, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Track hymn usage statistics", "EnableStatisticsTracking", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);
        }
    }
}
