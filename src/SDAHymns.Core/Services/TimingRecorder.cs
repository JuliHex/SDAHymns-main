namespace SDAHymns.Core.Services;

/// <summary>
/// Records timing information for hymn verses during audio playback
/// </summary>
public class TimingRecorder
{
    private readonly IAudioPlayerService _audioPlayer;
    private readonly Dictionary<int, double> _recordedTimings = new();
    private bool _isRecording = false;
    private int _nextVerseNumber = 1;

    public event EventHandler<int>? TimingRecorded;  // Fired when a timing is recorded

    public TimingRecorder(IAudioPlayerService audioPlayer)
    {
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    }

    /// <summary>
    /// Start recording session
    /// </summary>
    public void StartRecording()
    {
        _recordedTimings.Clear();
        _nextVerseNumber = 1;
        _isRecording = true;
    }

    /// <summary>
    /// Stop recording session
    /// </summary>
    public void StopRecording()
    {
        _isRecording = false;
    }

    /// <summary>
    /// Record a timing at the current playback position
    /// </summary>
    public void RecordTiming()
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Not currently recording. Call StartRecording() first.");
        }

        var currentTime = _audioPlayer.CurrentTime.TotalSeconds;
        _recordedTimings[_nextVerseNumber] = currentTime;
        TimingRecorded?.Invoke(this, _nextVerseNumber);
        _nextVerseNumber++;
    }

    /// <summary>
    /// Record a specific verse number at current position
    /// </summary>
    public void RecordTiming(int verseNumber)
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Not currently recording. Call StartRecording() first.");
        }

        if (verseNumber <= 0)
        {
            throw new ArgumentException("Verse number must be positive", nameof(verseNumber));
        }

        var currentTime = _audioPlayer.CurrentTime.TotalSeconds;
        _recordedTimings[verseNumber] = currentTime;
        TimingRecorded?.Invoke(this, verseNumber);

        // Update next verse number if needed
        if (verseNumber >= _nextVerseNumber)
        {
            _nextVerseNumber = verseNumber + 1;
        }
    }

    /// <summary>
    /// Remove a recorded timing
    /// </summary>
    public void RemoveTiming(int verseNumber)
    {
        _recordedTimings.Remove(verseNumber);
    }

    /// <summary>
    /// Clear all recorded timings
    /// </summary>
    public void ClearTimings()
    {
        _recordedTimings.Clear();
        _nextVerseNumber = 1;
    }

    /// <summary>
    /// Get the recorded timing for a verse
    /// </summary>
    public double? GetTiming(int verseNumber)
    {
        return _recordedTimings.TryGetValue(verseNumber, out var time) ? time : null;
    }

    /// <summary>
    /// Get all recorded timings
    /// </summary>
    public Dictionary<int, double> GetAllTimings()
    {
        return new Dictionary<int, double>(_recordedTimings);
    }

    /// <summary>
    /// Get timing map as JSON string
    /// </summary>
    public string GetTimingMapJson()
    {
        if (_recordedTimings.Count == 0)
        {
            return "{}";
        }

        var jsonMap = _recordedTimings.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);

        return System.Text.Json.JsonSerializer.Serialize(jsonMap);
    }

    /// <summary>
    /// Load existing timing map from JSON
    /// </summary>
    public void LoadTimingMap(string? timingMapJson)
    {
        _recordedTimings.Clear();
        _nextVerseNumber = 1;

        if (string.IsNullOrWhiteSpace(timingMapJson))
        {
            return;
        }

        try
        {
            var jsonMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(timingMapJson);
            if (jsonMap != null)
            {
                foreach (var kvp in jsonMap)
                {
                    if (int.TryParse(kvp.Key, out var verseNumber))
                    {
                        _recordedTimings[verseNumber] = kvp.Value;
                        if (verseNumber >= _nextVerseNumber)
                        {
                            _nextVerseNumber = verseNumber + 1;
                        }
                    }
                }
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Invalid JSON - leave empty
        }
    }

    /// <summary>
    /// Check if currently recording
    /// </summary>
    public bool IsRecording => _isRecording;

    /// <summary>
    /// Get the count of recorded timings
    /// </summary>
    public int TimingCount => _recordedTimings.Count;

    /// <summary>
    /// Get the next verse number that will be recorded
    /// </summary>
    public int NextVerseNumber => _nextVerseNumber;
}
