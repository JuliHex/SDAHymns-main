using System;
using System.Threading.Tasks;
using System.Text.Json;
using SDAHymns.Core.Services;

namespace SDAHymns.Desktop.Services;

public class DesktopRemoteControlService : IRemoteControlHandler
{
    // Events that ViewModels will subscribe to
    public event Action? OnNextRequested;
    public event Action? OnPrevRequested;
    public event Action<int, string>? OnLoadRequested;
    public event Action? OnToggleBlankRequested;
    
    // State that ViewModels will update
    public string CurrentHymnNumber { get; set; } = "";
    public string CurrentHymnTitle { get; set; } = "";
    public string CurrentVerseIndicator { get; set; } = "";
    public bool IsBlackScreen { get; set; }

    public Task<string> GetStatusJsonAsync()
    {
        var status = new
        {
            hymnNumber = CurrentHymnNumber,
            hymnTitle = CurrentHymnTitle,
            verseIndicator = CurrentVerseIndicator,
            isBlackScreen = IsBlackScreen
        };
        return Task.FromResult(JsonSerializer.Serialize(status));
    }

    public void NextVerse()
    {
        OnNextRequested?.Invoke();
    }

    public void PreviousVerse()
    {
        OnPrevRequested?.Invoke();
    }

    public Task LoadHymnAsync(int number, string category)
    {
        OnLoadRequested?.Invoke(number, category);
        return Task.CompletedTask;
    }

    public void ToggleBlackScreen()
    {
        OnToggleBlankRequested?.Invoke();
    }
}
