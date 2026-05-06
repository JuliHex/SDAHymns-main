using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}

public record AudioDeviceInfo(int DeviceNumber, string DeviceName);

/// <summary>
/// Service for playing audio files (piano recordings) with playback controls
/// </summary>
public interface IAudioPlayerService : IDisposable
{
    // Playback control
    Task LoadAsync(AudioRecording recording, string audioLibraryPath);
    Task PlayAsync();
    Task PauseAsync();
    Task StopAsync();
    Task SeekAsync(TimeSpan position);

    // Properties
    TimeSpan CurrentTime { get; }
    TimeSpan TotalDuration { get; }
    float Volume { get; set; }  // 0.0 to 1.0
    PlaybackState PlaybackState { get; }
    bool IsLoaded { get; }

    // Device configuration
    void SetOutputDevice(int deviceNumber);
    IEnumerable<AudioDeviceInfo> GetOutputDevices();

    // Events
    event EventHandler<TimeSpan>? PositionChanged;  // Fired frequently for UI updates
    event EventHandler? PlaybackEnded;
    event EventHandler<PlaybackState>? StateChanged;
}
