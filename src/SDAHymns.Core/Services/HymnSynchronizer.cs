using System.Text.Json;

namespace SDAHymns.Core.Services;

/// <summary>
/// Synchronizes audio playback with hymn verse display (auto-advance)
/// </summary>
public class HymnSynchronizer : IDisposable
{
    private readonly IAudioPlayerService _audioPlayer;
    private Dictionary<int, double> _timingMap = new();  // VerseNumber -> Seconds
    private List<(int VerseNumber, double TimeSeconds)> _sortedTimings = new();
    private int _currentTimingIndex = 0;
    private bool _isEnabled = false;
    private bool _disposed;

    // Events
    public event EventHandler<int>? VerseChangeRequested;  // Verse number to navigate to

    public HymnSynchronizer(IAudioPlayerService audioPlayer)
    {
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _audioPlayer.PositionChanged += OnPositionChanged;
        _audioPlayer.PlaybackEnded += OnPlaybackEnded;
        _audioPlayer.StateChanged += OnStateChanged;
    }

    /// <summary>
    /// Load timing map from JSON string
    /// </summary>
    public void LoadTimingMap(string? timingMapJson)
    {
        _timingMap.Clear();
        _sortedTimings.Clear();
        _currentTimingIndex = 0;

        if (string.IsNullOrWhiteSpace(timingMapJson))
        {
            return;
        }

        try
        {
            // Parse JSON: { "1": 12.5, "2": 45.2, "3": 88.0 }
            var jsonMap = JsonSerializer.Deserialize<Dictionary<string, double>>(timingMapJson);
            if (jsonMap != null)
            {
                _timingMap = jsonMap
                    .Where(kvp => int.TryParse(kvp.Key, out _))
                    .ToDictionary(kvp => int.Parse(kvp.Key), kvp => kvp.Value);

                // Create sorted list for efficient lookup
                _sortedTimings = _timingMap
                    .Select(kvp => (VerseNumber: kvp.Key, TimeSeconds: kvp.Value))
                    .OrderBy(t => t.TimeSeconds)
                    .ToList();
            }
        }
        catch (JsonException)
        {
            // Invalid JSON - leave empty
            _timingMap.Clear();
            _sortedTimings.Clear();
        }
    }

    /// <summary>
    /// Enable or disable auto-advance
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        if (enabled)
        {
            _currentTimingIndex = 0;  // Reset when enabled
        }
    }

    /// <summary>
    /// Check if timing map is loaded and has timings
    /// </summary>
    public bool HasTimings => _sortedTimings.Count > 0;

    /// <summary>
    /// Get current timing map as JSON string
    /// </summary>
    public string GetTimingMapJson()
    {
        if (_timingMap.Count == 0)
        {
            return "{}";
        }

        // Convert to string keys for JSON
        var jsonMap = _timingMap.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);

        return JsonSerializer.Serialize(jsonMap);
    }

    /// <summary>
    /// Add or update a timing entry
    /// </summary>
    public void SetTiming(int verseNumber, double timeSeconds)
    {
        _timingMap[verseNumber] = timeSeconds;

        // Rebuild sorted list
        _sortedTimings = _timingMap
            .Select(kvp => (VerseNumber: kvp.Key, TimeSeconds: kvp.Value))
            .OrderBy(t => t.TimeSeconds)
            .ToList();

        _currentTimingIndex = 0;  // Reset index
    }

    /// <summary>
    /// Remove a timing entry
    /// </summary>
    public void RemoveTiming(int verseNumber)
    {
        if (_timingMap.Remove(verseNumber))
        {
            _sortedTimings = _timingMap
                .Select(kvp => (VerseNumber: kvp.Key, TimeSeconds: kvp.Value))
                .OrderBy(t => t.TimeSeconds)
                .ToList();

            _currentTimingIndex = 0;
        }
    }

    /// <summary>
    /// Clear all timings
    /// </summary>
    public void ClearTimings()
    {
        _timingMap.Clear();
        _sortedTimings.Clear();
        _currentTimingIndex = 0;
    }

    private void OnPositionChanged(object? sender, TimeSpan currentTime)
    {
        if (!_isEnabled || _sortedTimings.Count == 0)
        {
            return;
        }

        var currentSeconds = currentTime.TotalSeconds;

        // Check if we need to advance to the next verse
        while (_currentTimingIndex < _sortedTimings.Count)
        {
            var timing = _sortedTimings[_currentTimingIndex];

            if (currentSeconds >= timing.TimeSeconds)
            {
                // Trigger verse change
                VerseChangeRequested?.Invoke(this, timing.VerseNumber);
                _currentTimingIndex++;
            }
            else
            {
                // Haven't reached next timing yet
                break;
            }
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        // Reset timing index when playback ends
        _currentTimingIndex = 0;
    }

    private void OnStateChanged(object? sender, PlaybackState newState)
    {
        if (newState == PlaybackState.Stopped)
        {
            // Reset timing index when stopped
            _currentTimingIndex = 0;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _audioPlayer.PositionChanged -= OnPositionChanged;
        _audioPlayer.PlaybackEnded -= OnPlaybackEnded;
        _audioPlayer.StateChanged -= OnStateChanged;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
