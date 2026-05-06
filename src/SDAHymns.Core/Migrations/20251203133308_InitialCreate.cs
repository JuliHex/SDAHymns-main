using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
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
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisplayProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BackgroundColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BackgroundOpacity = table.Column<double>(type: "REAL", nullable: false),
                    BackgroundImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FontFamily = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    TextColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TextAlignment = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EnableTextShadow = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShadowColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PaddingHorizontal = table.Column<int>(type: "INTEGER", nullable: false),
                    PaddingVertical = table.Column<int>(type: "INTEGER", nullable: false),
                    LineSpacing = table.Column<int>(type: "INTEGER", nullable: false),
                    TransparentBackground = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemProfile = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HymnCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LegacyFolderPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HymnCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hymns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    LegacyPowerPointPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hymns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hymns_HymnCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HymnCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AudioRecordings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HymnId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileFormat = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    RecordingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tempo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DefaultPlaybackSpeed = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioRecordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AudioRecordings_Hymns_HymnId",
                        column: x => x.HymnId,
                        principalTable: "Hymns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicePlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServicePlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    HymnId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedVerses = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePlanItems_Hymns_HymnId",
                        column: x => x.HymnId,
                        principalTable: "Hymns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePlanItems_ServicePlans_ServicePlanId",
                        column: x => x.ServicePlanId,
                        principalTable: "ServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HymnId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DisplayDurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioPlayed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServicePlanName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageStatistics_Hymns_HymnId",
                        column: x => x.HymnId,
                        principalTable: "Hymns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Verses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HymnId = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verses_Hymns_HymnId",
                        column: x => x.HymnId,
                        principalTable: "Hymns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "CreatedAt", "Description", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Last time legacy XML was imported", "LastDatabaseImportDate", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { 2, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Default display profile ID", "DefaultDisplayProfileId", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "1" },
                    { 3, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Track hymn usage statistics", "EnableStatisticsTracking", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.InsertData(
                table: "DisplayProfiles",
                columns: new[] { "Id", "BackgroundColor", "BackgroundImagePath", "BackgroundOpacity", "CreatedAt", "Description", "EnableTextShadow", "FontFamily", "FontSize", "IsDefault", "IsSystemProfile", "LineSpacing", "Name", "PaddingHorizontal", "PaddingVertical", "ShadowColor", "TextAlignment", "TextColor", "TransparentBackground", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "#000000", null, 1.0, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Default projector display settings", true, "Arial", 48, true, true, 15, "Projector", 60, 40, "#000000", "Center", "#FFFFFF", false, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "#000000", null, 0.0, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Transparent background for OBS/streaming", true, "Arial", 52, false, true, 18, "OBS Stream", 80, 60, "#000000", "Center", "#FFFFFF", true, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "#1a1a1a", null, 0.69999999999999996, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Lower opacity for practice/rehearsal", false, "Arial", 36, false, true, 12, "Practice Mode", 40, 30, "#000000", "Center", "#CCCCCC", false, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "HymnCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IconPath", "LegacyFolderPath", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Main Christian hymnbook", 1, null, "Imnuri Azs/Resurse/Imnuri crestine", "Imnuri crestine", "crestine", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Pathfinder/Companion hymns", 2, null, "Imnuri Azs/Resurse/Imnuri companioni", "Imnuri companioni", "companioni", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Explorer hymns", 3, null, "Imnuri Azs/Resurse/Imnuri exploratori", "Imnuri exploratori", "exploratori", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Firefly hymns (children's songs)", 4, null, "Imnuri Azs/Resurse/Imnuri licurici", "Imnuri licurici", "licurici", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Youth hymns", 5, null, "Imnuri Azs/Resurse/Imnuri tineret", "Imnuri tineret", "tineret", new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AudioRecordings_FilePath",
                table: "AudioRecordings",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_AudioRecordings_HymnId",
                table: "AudioRecordings",
                column: "HymnId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayProfiles_IsDefault",
                table: "DisplayProfiles",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayProfiles_Name",
                table: "DisplayProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HymnCategories_DisplayOrder",
                table: "HymnCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_HymnCategories_Name",
                table: "HymnCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HymnCategories_Slug",
                table: "HymnCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hymns_CategoryId",
                table: "Hymns",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Hymns_Number",
                table: "Hymns",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Hymns_Number_CategoryId",
                table: "Hymns",
                columns: new[] { "Number", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hymns_Title",
                table: "Hymns",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePlanItems_HymnId",
                table: "ServicePlanItems",
                column: "HymnId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePlanItems_ServicePlanId_DisplayOrder",
                table: "ServicePlanItems",
                columns: new[] { "ServicePlanId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePlans_IsActive",
                table: "ServicePlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePlans_Name",
                table: "ServicePlans",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePlans_ServiceDate",
                table: "ServicePlans",
                column: "ServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_UsageStatistics_HymnId",
                table: "UsageStatistics",
                column: "HymnId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageStatistics_UsedAt",
                table: "UsageStatistics",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Verses_HymnId",
                table: "Verses",
                column: "HymnId");

            migrationBuilder.CreateIndex(
                name: "IX_Verses_HymnId_DisplayOrder",
                table: "Verses",
                columns: new[] { "HymnId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Verses_HymnId_VerseNumber",
                table: "Verses",
                columns: new[] { "HymnId", "VerseNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AudioRecordings");

            migrationBuilder.DropTable(
                name: "DisplayProfiles");

            migrationBuilder.DropTable(
                name: "ServicePlanItems");

            migrationBuilder.DropTable(
                name: "UsageStatistics");

            migrationBuilder.DropTable(
                name: "Verses");

            migrationBuilder.DropTable(
                name: "ServicePlans");

            migrationBuilder.DropTable(
                name: "Hymns");

            migrationBuilder.DropTable(
                name: "HymnCategories");
        }
    }
}
