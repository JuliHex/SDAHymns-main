namespace SDAHymns.Core.Services;

public interface ILegacyXmlImportService
{
    Task<ImportResult> ImportAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ImportResult> ImportCategoryAsync(string categorySlug, CancellationToken cancellationToken = default);
    Task<ImportStatistics> GetImportStatisticsAsync();
}
