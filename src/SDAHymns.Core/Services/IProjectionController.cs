using SDAHymns.Core.Data.Models;
using System.Threading.Tasks;

namespace SDAHymns.Core.Services;

public interface IProjectionController
{
    Task ShowHymnAsync(Hymn hymn, int verseIndex);
    Task BlankDisplayAsync();
    Task TogglePresenterViewAsync();
    Task SetStatusMessageAsync(string message);
}
