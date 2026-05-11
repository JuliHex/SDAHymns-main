using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;

var dbPath = "hymns.db"; // Assuming it's in the root
if (!System.IO.File.Exists(dbPath)) {
    dbPath = "src/SDAHymns.Desktop/hymns.db";
}

var optionsBuilder = new DbContextOptionsBuilder<HymnsContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var context = new HymnsContext(optionsBuilder.Options);

var hymn = context.Hymns
    .Include(h => h.Category)
    .Include(h => h.Verses)
    .FirstOrDefault(h => h.Number == 2 && h.Category.Slug == "exploratori");

if (hymn == null) {
    Console.WriteLine("Hymn not found");
    return;
}

Console.WriteLine($"Hymn: {hymn.Number}. {hymn.Title}");
foreach (var verse in hymn.Verses.OrderBy(v => v.DisplayOrder)) {
    Console.WriteLine($"Label: {verse.Label} | Content: {verse.Content.Replace("\n", " ").Substring(0, Math.Min(30, verse.Content.Length))}...");
}
