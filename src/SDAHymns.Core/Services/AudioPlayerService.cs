using System.Timers;
using NAudio.Wave;
using SDAHymns.Core.Data.Models;
using Timer = System.Timers.Timer;

namespace SDAHymns.Core.Services;

/// <summary>
/// Audio playback service using NAudio for piano recording playback
/// </summary>
public class AudioPlayerService : IAudioPlayerService
{
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioFileReader;
    private Timer? _positionTimer;
    private string? _currentFilePath;
    private bool _disposed;
    private float _volumeOffset = 0.0f;

    // Properties
    public TimeSpan CurrentTime => _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
    public TimeSpan TotalDuration => _audioFileReader?.TotalTime ?? TimeSpan.Zero;
    public PlaybackState PlaybackState { get; private set; } = PlaybackState.Stopped;
    public bool IsLoaded => _audioFileReader != null;

    private float _volume = 0.8f;
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0.0f, 1.0f);
            if (_waveOut != null)
            {
                // Apply global volume plus per-track offset
                _waveOut.Volume = Math.Clamp(_volume + _volumeOffset, 0.0f, 1.0f);
            }
        }
    }

    // Events
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? PlaybackEnded;
    public event EventHandler<PlaybackState>? StateChanged;

    public Task LoadAsync(AudioRecording recording, string audioLibraryPath)
    {
        try
        {
            // Build full file path
            var filePath = Path.Combine(audioLibraryPath, recording.FilePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Audio file not found: {filePath}");
            }

            // Clean up previous resources
            DisposeAudioResources();

            // Load audio file
            _audioFileReader = new AudioFileReader(filePath);
            _currentFilePath = filePath;

            // Store volume offset and set audio file reader to pass-through (1.0f)
            _volumeOffset = recording.VolumeOffset;
            _audioFileReader.Volume = 1.0f;

            // Initialize playback device with combined volume (global + offset)
            _waveOut = new WaveOutEvent
            {
                Volume = Math.Clamp(_volume + _volumeOffset, 0.0f, 1.0f)
            };

            _waveOut.Init(_audioFileReader);
            _waveOut.PlaybackStopped += OnPlaybackStopped;

            // Initialize position timer (fires every 100ms for UI updates)
            _positionTimer = new Timer(100);
            _positionTimer.Elapsed += OnPositionTimerElapsed;

            SetPlaybackState(PlaybackState.Stopped);

            return Task.CompletedTask;
        }
        catch (Exception)
        {
            DisposeAudioResources();
            throw;
        }
    }

    public Task PlayAsync()
    {
        if (_waveOut == null || _audioFileReader == null)
        {
            throw new InvalidOperationException("No audio file loaded. Call LoadAsync first.");
        }

        _waveOut.Play();
        _positionTimer?.Start();
        SetPlaybackState(PlaybackState.Playing);

        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        if (_waveOut == null)
        {
            throw new InvalidOperationException("No audio file loaded. Call LoadAsync first.");
        }

        _waveOut.Pause();
        _positionTimer?.Stop();
        SetPlaybackState(PlaybackState.Paused);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_waveOut == null)
        {
            throw new InvalidOperationException("No audio file loaded. Call LoadAsync first.");
        }

        _waveOut.Stop();
        _positionTimer?.Stop();

        // Reset position to beginning
        if (_audioFileReader != null)
        {
            _audioFileReader.CurrentTime = TimeSpan.Zero;
        }

        SetPlaybackState(PlaybackState.Stopped);

        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position)
    {
        if (_audioFileReader == null)
        {
            throw new InvalidOperationException("No audio file loaded. Call LoadAsync first.");
        }

        // Clamp position to valid range
        var clampedPosition = TimeSpan.FromSeconds(
            Math.Clamp(position.TotalSeconds, 0, _audioFileReader.TotalTime.TotalSeconds));

        _audioFileReader.CurrentTime = clampedPosition;
        PositionChanged?.Invoke(this, clampedPosition);

        return Task.CompletedTask;
    }

    public void SetOutputDevice(int deviceNumber)
    {
        if (_waveOut != null)
        {
            // Need to recreate WaveOut with new device
            var wasPlaying = PlaybackState == PlaybackState.Playing;
            var currentPosition = CurrentTime;

            _waveOut.Stop();
            _waveOut.Dispose();

            _waveOut = new WaveOutEvent
            {
                DeviceNumber = deviceNumber,
                Volume = Math.Clamp(_volume + _volumeOffset, 0.0f, 1.0f)
            };

            if (_audioFileReader != null)
            {
                _waveOut.Init(_audioFileReader);
                _waveOut.PlaybackStopped += OnPlaybackStopped;

                // Restore position
                _audioFileReader.CurrentTime = currentPosition;

                if (wasPlaying)
                {
                    _waveOut.Play();
                }
            }
        }
    }

    public IEnumerable<AudioDeviceInfo> GetOutputDevices()
    {
        // WaveOutEvent uses the default output device and doesn't provide device enumeration
        // For more advanced device selection, would need to use DirectSoundOut or WASAPI
        // For now, just return the default device
        yield return new AudioDeviceInfo(-1, "Default Output Device");
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _positionTimer?.Stop();

        // Check if stopped due to end of playback or error
        if (e.Exception != null)
        {
            SetPlaybackState(PlaybackState.Stopped);
            // Could add error event here
        }
        else
        {
            // Natural end of playback
            SetPlaybackState(PlaybackState.Stopped);
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnPositionTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_audioFileReader != null)
        {
            PositionChanged?.Invoke(this, _audioFileReader.CurrentTime);
        }
    }

    private void SetPlaybackState(PlaybackState newState)
    {
        if (PlaybackState != newState)
        {
            PlaybackState = newState;
            StateChanged?.Invoke(this, newState);
        }
    }

    private void DisposeAudioResources()
    {
        _positionTimer?.Stop();
        _positionTimer?.Dispose();
        _positionTimer = null;

        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        _audioFileReader?.Dispose();
        _audioFileReader = null;
        _currentFilePath = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        DisposeAudioResources();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
