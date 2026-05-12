using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDAHymns.Android.ViewModels;

public partial class AndroidRemoteViewModel : ObservableObject, IDisposable
{
    private readonly NetworkProjectionController _controller;
    private readonly HttpClient _httpClient;
    private bool _isAutoUpdating = true;

    [ObservableProperty]
    private string _activeHymnNumber = "";

    [ObservableProperty]
    private string _activeHymnTitle = "No hymn loaded";

    [ObservableProperty]
    private string _verseIndicator = "";

    [ObservableProperty]
    private string _hymnNumberInput = "";

    [ObservableProperty]
    private string _serverIp = "192.168.100.37";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public AndroidRemoteViewModel()
    {
        _httpClient = new HttpClient();
        _controller = new NetworkProjectionController(_httpClient);
        
        // Start status update loop
        Task.Run(UpdateStatusLoop);
    }

    [RelayCommand]
    private async Task Next() => await _controller.NextAsync();

    [RelayCommand]
    private async Task Previous() => await _controller.PreviousAsync();

    [RelayCommand]
    private async Task Load()
    {
        if (int.TryParse(HymnNumberInput, out int num))
        {
            await _controller.LoadHymnAsync(num, "crestine");
            HymnNumberInput = "";
        }
    }

    [RelayCommand]
    private async Task Blank() => await _controller.BlankDisplayAsync();

    partial void OnServerIpChanged(string value)
    {
        _controller.SetBaseUrl($"http://{value}:4546");
    }

    private async Task UpdateStatusLoop()
    {
        while (_isAutoUpdating)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://{ServerIp}:4546/status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var status = JsonDocument.Parse(json).RootElement;
                    
                    ActiveHymnNumber = status.GetProperty("hymnNumber").GetString() ?? "";
                    ActiveHymnTitle = status.GetProperty("hymnTitle").GetString() ?? "No hymn loaded";
                    VerseIndicator = status.GetProperty("verseIndicator").GetString() ?? "";
                    StatusMessage = "Connected";
                }
            }
            catch
            {
                StatusMessage = "Connecting...";
            }
            await Task.Delay(1000);
        }
    }

    public void Dispose()
    {
        _isAutoUpdating = false;
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
