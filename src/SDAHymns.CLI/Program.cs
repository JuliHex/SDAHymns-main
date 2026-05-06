using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDAHymns.CLI.Commands;
using SDAHymns.Core.Data;
using SDAHymns.Core.Services;

// Setup dependency injection
var services = new ServiceCollection();

// Helper to find solution root
static string GetSolutionRoot()
{
    var currentDir = Directory.GetCurrentDirectory();
    while (currentDir != null)
    {
        if (File.Exists(Path.Combine(currentDir, "SDAHymns.sln")))
        {
            return currentDir;
        }
        currentDir = Directory.GetParent(currentDir)?.FullName;
    }
    throw new InvalidOperationException("Could not find solution root directory");
}

// Database context
var solutionRoot = GetSolutionRoot();
var dbPath = Path.Combine(solutionRoot, "Resources", "hymns.db");

services.AddDbContext<HymnsContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

// Logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Services
services.AddScoped<PowerPointParserService>();
services.AddScoped<ILegacyXmlImportService, LegacyXmlImportService>();
services.AddScoped<IVerseImportService, VerseImportService>();

// Command handlers
services.AddScoped<ImportCommandHandler>();
services.AddScoped<ImportOrphanPptCommandHandler>();
services.AddScoped<TestPptCommandHandler>();
services.AddScoped<TestVerseExtractionCommandHandler>();

var serviceProvider = services.BuildServiceProvider();

// Parse command-line arguments and execute
return await Parser.Default.ParseArguments<ImportOptions, ImportOrphanPptCommand, TestPptCommand, TestVerseExtractionCommand, ImportVersesCommand>(args)
    .MapResult(
        async (ImportOptions opts) =>
        {
            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ImportCommandHandler>();
            return await handler.ExecuteAsync(opts);
        },
        async (ImportOrphanPptCommand cmd) =>
        {
            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ImportOrphanPptCommandHandler>();
            return await handler.ExecuteAsync(cmd);
        },
        async (TestPptCommand cmd) =>
        {
            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TestPptCommandHandler>();
            return await handler.ExecuteAsync(cmd);
        },
        async (TestVerseExtractionCommand cmd) =>
        {
            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TestVerseExtractionCommandHandler>();
            return await handler.ExecuteAsync(cmd);
        },
        async (ImportVersesCommand cmd) =>
        {
            using var scope = serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IVerseImportService>();
            return await ImportVersesCommand.ExecuteAsync(cmd, importService);
        },
        errs => Task.FromResult(1));
