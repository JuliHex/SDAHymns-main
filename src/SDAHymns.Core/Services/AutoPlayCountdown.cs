using System.Timers;
using Timer = System.Timers.Timer;

namespace SDAHymns.Core.Services;

/// <summary>
/// Manages auto-play countdown timer
/// </summary>
public class AutoPlayCountdown : IDisposable
{
    private Timer? _timer;
    private int _remainingSeconds;
    private bool _disposed;

    public event EventHandler<int>? CountdownTick;  // Fired each second with remaining time
    public event EventHandler? CountdownCompleted;  // Fired when countdown reaches 0
    public event EventHandler? CountdownCancelled;  // Fired when countdown is cancelled

    /// <summary>
    /// Start countdown with specified delay in seconds
    /// </summary>
    public void Start(int delaySeconds)
    {
        if (delaySeconds <= 0)
        {
            throw new ArgumentException("Delay must be positive", nameof(delaySeconds));
        }

        Stop();  // Stop any existing countdown

        _remainingSeconds = delaySeconds;
        CountdownTick?.Invoke(this, _remainingSeconds);

        _timer = new Timer(1000);  // 1 second interval
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    /// <summary>
    /// Cancel the countdown
    /// </summary>
    public void Cancel()
    {
        if (_timer != null && _timer.Enabled)
        {
            Stop();
            CountdownCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Stop the countdown without firing cancelled event
    /// </summary>
    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    /// <summary>
    /// Check if countdown is active
    /// </summary>
    public bool IsActive => _timer != null && _timer.Enabled;

    /// <summary>
    /// Get remaining seconds
    /// </summary>
    public int RemainingSeconds => _remainingSeconds;

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _remainingSeconds--;

        if (_remainingSeconds > 0)
        {
            CountdownTick?.Invoke(this, _remainingSeconds);
        }
        else
        {
            Stop();
            CountdownCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
