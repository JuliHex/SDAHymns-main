using System;
using System.Net.Http;
using System.Threading.Tasks;
using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

public class NetworkProjectionController : IProjectionController
{
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public NetworkProjectionController(HttpClient httpClient, string baseUrl = "http://192.168.100.37:4546")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public void SetBaseUrl(string url)
    {
        _baseUrl = url.TrimEnd('/');
    }

    public async Task ShowHymnAsync(Hymn hymn, int verseIndex)
    {
        // For the network controller, we mostly care about navigation
        // The server handles the hymn loading if we send a load command
        // But if we are already in a hymn, we might just want to sync index
        // For simplicity, we'll use specific endpoints
    }

    public async Task BlankDisplayAsync()
    {
        try { await _httpClient.PostAsync($"{_baseUrl}/blank", null); } catch { }
    }

    public async Task NextAsync()
    {
        try { await _httpClient.PostAsync($"{_baseUrl}/next", null); } catch { }
    }

    public async Task PreviousAsync()
    {
        try { await _httpClient.PostAsync($"{_baseUrl}/prev", null); } catch { }
    }

    public async Task LoadHymnAsync(int number, string category)
    {
        try { await _httpClient.PostAsync($"{_baseUrl}/load?number={number}&category={category}", null); } catch { }
    }

    public Task TogglePresenterViewAsync() => Task.CompletedTask; // Not relevant for remote
    public Task SetStatusMessageAsync(string message) => Task.CompletedTask;

    // Map interface methods
    Task IProjectionController.ShowHymnAsync(Hymn hymn, int verseIndex) => Task.CompletedTask; // Handled by load/next/prev
    Task IProjectionController.BlankDisplayAsync() => BlankDisplayAsync();
    Task IProjectionController.TogglePresenterViewAsync() => TogglePresenterViewAsync();
    Task IProjectionController.SetStatusMessageAsync(string message) => SetStatusMessageAsync(message);
}
