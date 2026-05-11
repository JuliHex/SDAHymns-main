using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.Services;

namespace SDAHymns.Desktop.ViewModels;

public partial class RecorderModeViewModel : ViewModelBase
{
    private readonly IAudioPlayerService _audioPlayer;
    private readonly TimingRecorder _timingRecorder;
    private AudioRecording? _currentRecording;

    [ObservableProperty]
    private string _hymnTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TimingEntry> _recordedTimings = new();

    [ObservableProperty]
    private string _statusMessage = LocalizationManager.Instance.GetString("Recorder.Status.PressPlay");

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private bool _isRecording = false;

    [ObservableProperty]
    private string _audioTimeDisplay = "00:00 / 00:00";

    [ObservableProperty]
    private int _nextVerseNumber = 1;

    public RecorderModeViewModel(IAudioPlayerService audioPlayer)
    {
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _timingRecorder = new TimingRecorder(audioPlayer);

        // Subscribe to timing recorder events
        _timingRecorder.TimingRecorded += OnTimingRecorded;

        // Subscribe to audio player events
        _audioPlayer.PositionChanged += OnAudioPositionChanged;
        _audioPlayer.PlaybackEnded += OnPlaybackEnded;
        _audioPlayer.StateChanged += OnStateChanged;
    }

    public void LoadHymn(Hymn hymn, AudioRecording recording)
    {
        _currentRecording = recording;
        HymnTitle = $"{hymn.Number}. {hymn.Title}";
        StatusMessage = string.Format(LocalizationManager.Instance.GetString("Recorder.Status.Ready"), hymn.Title);
        NextVerseNumber = 1;
        RecordedTimings.Clear();

        // Load existing timings if any
        if (_timingRecorder.TimingCount > 0)
        {
            var timings = _timingRecorder.GetAllTimings();
            foreach (var timing in timings.OrderBy(t => t.Key))
            {
                RecordedTimings.Add(new TimingEntry
                {
                    VerseNumber = timing.Key,
                    Timestamp = timing.Value,
                    DisplayTime = FormatTime(timing.Value)
                });
            }
            NextVerseNumber = timings.Keys.Max() + 1;
        }
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        try
        {
            await _audioPlayer.PlayAsync();

            if (!IsRecording)
            {
                _timingRecorder.StartRecording();
                IsRecording = true;
                StatusMessage = LocalizationManager.Instance.GetString("Recorder.Status.Recording");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
        }
    }

    [RelayCommand]
    private async Task PauseAsync()
    {
        await _audioPlayer.PauseAsync();
        StatusMessage = LocalizationManager.Instance.GetString("Recorder.Status.Paused");
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await _audioPlayer.StopAsync();
        _timingRecorder.StopRecording();
        IsRecording = false;
        StatusMessage = LocalizationManager.Instance.GetString("Recorder.Status.Stopped");
    }

    [RelayCommand]
    private void TapTiming()
    {
        if (!IsRecording || !IsPlaying)
        {
            StatusMessage = LocalizationManager.Instance.GetString("Recorder.Status.PressPlayFirst");
            return;
        }

        _timingRecorder.RecordTiming(NextVerseNumber);
        NextVerseNumber++;
    }

    [RelayCommand]
    private void RemoveTiming(TimingEntry timing)
    {
        RecordedTimings.Remove(timing);
        _timingRecorder.RemoveTiming(timing.VerseNumber);

        // Adjust next verse number if needed
        if (RecordedTimings.Count > 0)
        {
            NextVerseNumber = RecordedTimings.Max(t => t.VerseNumber) + 1;
        }
        else
        {
            NextVerseNumber = 1;
        }
    }

    [RelayCommand]
    private void ClearAllTimings()
    {
        RecordedTimings.Clear();
        _timingRecorder.ClearTimings();
        NextVerseNumber = 1;
        StatusMessage = LocalizationManager.Instance.GetString("Recorder.Status.Cleared");
    }

    public string GetTimingMapJson()
    {
        return _timingRecorder.GetTimingMapJson();
    }

    private void OnTimingRecorded(object? sender, int verseNumber)
    {
        // Get the timing from the recorder
        var timestamp = _timingRecorder.GetTiming(verseNumber);
        if (timestamp.HasValue)
        {
            // Add to UI collection
            RecordedTimings.Add(new TimingEntry
            {
                VerseNumber = verseNumber,
                Timestamp = timestamp.Value,
                DisplayTime = FormatTime(timestamp.Value)
            });

            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Recorder.Status.Recorded"), verseNumber, FormatTime(timestamp.Value));
        }
    }

    private void OnAudioPositionChanged(object? sender, TimeSpan position)
    {
        var duration = _audioPlayer.TotalDuration;
        AudioTimeDisplay = $"{FormatTime(position.TotalSeconds)} / {FormatTime(duration.TotalSeconds)}";
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _timingRecorder.StopRecording();
        IsRecording = false;
        StatusMessage = string.Format(LocalizationManager.Instance.GetString("Recorder.Status.Finished"), RecordedTimings.Count);
    }

    private void OnStateChanged(object? sender, PlaybackState state)
    {
        IsPlaying = state == PlaybackState.Playing;
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
    }
}

public class TimingEntry
{
    public int VerseNumber { get; set; }
    public double Timestamp { get; set; }
    public string DisplayTime { get; set; } = string.Empty;
}
