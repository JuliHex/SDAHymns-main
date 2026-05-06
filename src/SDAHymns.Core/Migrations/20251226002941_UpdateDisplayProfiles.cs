using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SDAHymns.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDisplayProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaddingVertical",
                table: "DisplayProfiles",
                newName: "VerseFontSize");

            migrationBuilder.RenameColumn(
                name: "PaddingHorizontal",
                table: "DisplayProfiles",
                newName: "TitleFontSize");

            migrationBuilder.RenameColumn(
                name: "LineSpacing",
                table: "DisplayProfiles",
                newName: "ShowVerseNumbers");

            migrationBuilder.RenameColumn(
                name: "FontSize",
                table: "DisplayProfiles",
                newName: "ShowHymnTitle");

            migrationBuilder.AlterColumn<string>(
                name: "ShadowColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageMode",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BackgroundImageOpacity",
                table: "DisplayProfiles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTextOutline",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FontWeight",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LabelColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LabelFontSize",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "LetterSpacing",
                table: "DisplayProfiles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LineHeight",
                table: "DisplayProfiles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MarginBottom",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarginLeft",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarginRight",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarginTop",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutlineColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OutlineThickness",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShadowBlurRadius",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShadowOffsetX",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShadowOffsetY",
                table: "DisplayProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TitleColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VerticalAlignment",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccentColor", "BackgroundImageMode", "BackgroundImageOpacity", "Description", "EnableTextOutline", "EnableTextShadow", "FontFamily", "FontWeight", "LabelColor", "LabelFontSize", "LetterSpacing", "LineHeight", "MarginBottom", "MarginLeft", "MarginRight", "MarginTop", "Name", "OutlineColor", "OutlineThickness", "ShadowBlurRadius", "ShadowOffsetX", "ShadowOffsetY", "ShowHymnTitle", "ShowVerseNumbers", "TextAlignment", "TitleColor", "TitleFontSize", "VerseFontSize", "VerticalAlignment" },
                values: new object[] { "#0078D4", "Fill", 0.29999999999999999, "Default black background with white text, left-aligned", false, false, "Inter", "Normal", "#CCCCCC", 28, 0.0, 1.3999999999999999, 60, 100, 100, 60, "Classic Dark", "#000000", 2, 10, 2, 2, true, true, "Left", "#FFFFFF", 36, 48, "Center" });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AccentColor", "BackgroundImageMode", "BackgroundImageOpacity", "BackgroundOpacity", "Description", "EnableTextOutline", "FontFamily", "FontWeight", "LabelColor", "LabelFontSize", "LetterSpacing", "LineHeight", "MarginBottom", "MarginLeft", "MarginRight", "MarginTop", "Name", "OutlineColor", "OutlineThickness", "ShadowBlurRadius", "ShadowOffsetX", "ShadowOffsetY", "ShowHymnTitle", "ShowVerseNumbers", "TextAlignment", "TitleColor", "TitleFontSize", "TransparentBackground", "VerseFontSize", "VerticalAlignment" },
                values: new object[] { "#FFFFFF", "Fill", 0.29999999999999999, 1.0, "Pure black and white with bold text and shadow for maximum visibility", false, "Inter", "Bold", "#FFFFFF", 32, 0.0, 1.3999999999999999, 60, 100, 100, 60, "High Contrast", "#000000", 2, 15, 3, 3, true, true, "Left", "#FFFFFF", 40, false, 56, "Center" });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccentColor", "BackgroundColor", "BackgroundImageMode", "BackgroundImageOpacity", "BackgroundOpacity", "Description", "EnableTextOutline", "FontFamily", "FontWeight", "LabelColor", "LabelFontSize", "LetterSpacing", "LineHeight", "MarginBottom", "MarginLeft", "MarginRight", "MarginTop", "Name", "OutlineColor", "OutlineThickness", "ShadowBlurRadius", "ShadowOffsetX", "ShadowOffsetY", "ShowHymnTitle", "ShowVerseNumbers", "TextColor", "TitleColor", "TransparentBackground", "VerseFontSize", "VerticalAlignment" },
                values: new object[] { "#0078D4", "#000000", "Fill", 0.29999999999999999, 0.0, "Transparent background with text outline for streaming over video", true, "Inter", "SemiBold", "#EEEEEE", 30, 0.0, 1.3999999999999999, 80, 120, 120, 80, "OBS Stream", "#000000", 3, 10, 2, 2, true, true, "#FFFFFF", "#FFFFFF", true, 52, "Center" });

            migrationBuilder.InsertData(
                table: "DisplayProfiles",
                columns: new[] { "Id", "AccentColor", "BackgroundColor", "BackgroundImageMode", "BackgroundImageOpacity", "BackgroundImagePath", "BackgroundOpacity", "CreatedAt", "Description", "EnableTextOutline", "EnableTextShadow", "FontFamily", "FontWeight", "IsDefault", "IsSystemProfile", "LabelColor", "LabelFontSize", "LetterSpacing", "LineHeight", "MarginBottom", "MarginLeft", "MarginRight", "MarginTop", "Name", "OutlineColor", "OutlineThickness", "ShadowBlurRadius", "ShadowColor", "ShadowOffsetX", "ShadowOffsetY", "ShowHymnTitle", "ShowVerseNumbers", "TextAlignment", "TextColor", "TitleColor", "TitleFontSize", "TransparentBackground", "UpdatedAt", "VerseFontSize", "VerticalAlignment" },
                values: new object[,]
                {
                    { 4, "#FFD700", "#001F3F", "Fill", 0.29999999999999999, null, 1.0, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Dark blue background with yellow text for bright environments", false, true, "Inter", "Bold", false, true, "#FFC700", 30, 0.0, 1.3999999999999999, 60, 100, 100, 60, "Bright Room", "#000000", 2, 12, "#000000", 2, 2, true, true, "Center", "#FFD700", "#FFD700", 38, false, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), 52, "Center" },
                    { 5, "#666666", "#000000", "Fill", 0.29999999999999999, null, 1.0, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Clean and simple with minimal styling", false, false, "Inter", "Normal", false, true, "#CCCCCC", 26, 0.0, 1.5, 50, 80, 80, 50, "Minimalist", "#000000", 2, 10, "#000000", 2, 2, true, true, "Left", "#EEEEEE", "#EEEEEE", 32, false, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), 44, "Center" },
                    { 6, "#D4AF37", "#00274D", "Fill", 0.29999999999999999, null, 1.0, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Classic church aesthetic with navy and gold colors", false, false, "Georgia", "Normal", false, true, "#C4A030", 32, 0.0, 1.5, 70, 120, 120, 70, "Traditional", "#000000", 2, 10, "#000000", 2, 2, true, true, "Center", "#D4AF37", "#D4AF37", 40, false, new DateTime(2025, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), 50, "Center" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "BackgroundImageMode",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "BackgroundImageOpacity",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "EnableTextOutline",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "FontWeight",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "LabelColor",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "LabelFontSize",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "LetterSpacing",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "LineHeight",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "MarginBottom",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "MarginLeft",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "MarginRight",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "MarginTop",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "OutlineColor",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "OutlineThickness",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "ShadowBlurRadius",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "ShadowOffsetX",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "ShadowOffsetY",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "TitleColor",
                table: "DisplayProfiles");

            migrationBuilder.DropColumn(
                name: "VerticalAlignment",
                table: "DisplayProfiles");

            migrationBuilder.RenameColumn(
                name: "VerseFontSize",
                table: "DisplayProfiles",
                newName: "PaddingVertical");

            migrationBuilder.RenameColumn(
                name: "TitleFontSize",
                table: "DisplayProfiles",
                newName: "PaddingHorizontal");

            migrationBuilder.RenameColumn(
                name: "ShowVerseNumbers",
                table: "DisplayProfiles",
                newName: "LineSpacing");

            migrationBuilder.RenameColumn(
                name: "ShowHymnTitle",
                table: "DisplayProfiles",
                newName: "FontSize");

            migrationBuilder.AlterColumn<string>(
                name: "ShadowColor",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DisplayProfiles",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "EnableTextShadow", "FontFamily", "FontSize", "LineSpacing", "Name", "PaddingHorizontal", "PaddingVertical", "TextAlignment" },
                values: new object[] { "Default projector display settings", true, "Arial", 48, 15, "Projector", 60, 40, "Center" });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BackgroundOpacity", "Description", "FontFamily", "FontSize", "LineSpacing", "Name", "PaddingHorizontal", "PaddingVertical", "TextAlignment", "TransparentBackground" },
                values: new object[] { 0.0, "Transparent background for OBS/streaming", "Arial", 52, 18, "OBS Stream", 80, 60, "Center", true });

            migrationBuilder.UpdateData(
                table: "DisplayProfiles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BackgroundColor", "BackgroundOpacity", "Description", "FontFamily", "FontSize", "LineSpacing", "Name", "PaddingVertical", "TextColor", "TransparentBackground" },
                values: new object[] { "#1a1a1a", 0.69999999999999996, "Lower opacity for practice/rehearsal", "Arial", 36, 12, "Practice Mode", 30, "#CCCCCC", false });
        }
    }
}
