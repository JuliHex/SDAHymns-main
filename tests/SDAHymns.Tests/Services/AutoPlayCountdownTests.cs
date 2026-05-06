using FluentAssertions;
using SDAHymns.Core.Services;
using Xunit;

namespace SDAHymns.Tests.Services;

public class AutoPlayCountdownTests
{
    [Fact]
    public void Start_ThrowsArgumentException_WhenDelayIsZeroOrNegative()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => countdown.Start(0));
        Assert.Throws<ArgumentException>(() => countdown.Start(-1));
    }

    [Fact]
    public void Start_SetsIsActiveToTrue()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();

        // Act
        countdown.Start(5);

        // Assert
        countdown.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_SetsRemainingSeconds()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();

        // Act
        countdown.Start(5);

        // Assert
        countdown.RemainingSeconds.Should().Be(5);
    }

    [Fact]
    public void Start_FiresInitialCountdownTickEvent()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var eventFired = false;
        var receivedSeconds = 0;

        countdown.CountdownTick += (sender, seconds) =>
        {
            eventFired = true;
            receivedSeconds = seconds;
        };

        // Act
        countdown.Start(5);

        // Assert
        eventFired.Should().BeTrue();
        receivedSeconds.Should().Be(5);
    }

    [Fact]
    public async Task Start_FiresCountdownTickEventsEachSecond()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var tickCount = 0;
        var receivedValues = new List<int>();

        countdown.CountdownTick += (sender, seconds) =>
        {
            tickCount++;
            receivedValues.Add(seconds);
        };

        // Act
        countdown.Start(3);
        await Task.Delay(3500);  // Wait for 3+ seconds

        // Assert
        tickCount.Should().BeGreaterThanOrEqualTo(3);
        receivedValues.Should().Contain(3);
        receivedValues.Should().Contain(2);
        receivedValues.Should().Contain(1);
    }

    [Fact]
    public async Task Start_FiresCountdownCompletedEvent_WhenReachingZero()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var completedFired = false;

        countdown.CountdownCompleted += (sender, e) =>
        {
            completedFired = true;
        };

        // Act
        countdown.Start(2);
        await Task.Delay(2500);  // Wait for countdown to complete

        // Assert
        completedFired.Should().BeTrue();
    }

    [Fact]
    public async Task Start_StopsAfterCountdownCompletes()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();

        // Act
        countdown.Start(1);
        await Task.Delay(1500);  // Wait for countdown to complete

        // Assert
        countdown.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Cancel_FiresCountdownCancelledEvent()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var cancelledFired = false;

        countdown.CountdownCancelled += (sender, e) =>
        {
            cancelledFired = true;
        };

        countdown.Start(5);

        // Act
        countdown.Cancel();

        // Assert
        cancelledFired.Should().BeTrue();
    }

    [Fact]
    public void Cancel_StopsCountdown()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        countdown.Start(5);

        // Act
        countdown.Cancel();

        // Assert
        countdown.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Cancel_DoesNotFireCancelledEvent_WhenNotActive()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var cancelledFired = false;

        countdown.CountdownCancelled += (sender, e) =>
        {
            cancelledFired = true;
        };

        // Act
        countdown.Cancel();  // Cancel without starting

        // Assert
        cancelledFired.Should().BeFalse();
    }

    [Fact]
    public void Stop_StopsCountdownWithoutFiringCancelledEvent()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var cancelledFired = false;

        countdown.CountdownCancelled += (sender, e) =>
        {
            cancelledFired = true;
        };

        countdown.Start(5);

        // Act
        countdown.Stop();

        // Assert
        countdown.IsActive.Should().BeFalse();
        cancelledFired.Should().BeFalse();
    }

    [Fact]
    public void Start_StopsExistingCountdown_BeforeStartingNew()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        countdown.Start(10);

        // Act
        countdown.Start(3);

        // Assert
        countdown.RemainingSeconds.Should().Be(3);
        countdown.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Countdown_DoesNotFireCompletedEvent_WhenCancelled()
    {
        // Arrange
        using var countdown = new AutoPlayCountdown();
        var completedFired = false;

        countdown.CountdownCompleted += (sender, e) =>
        {
            completedFired = true;
        };

        countdown.Start(2);

        // Act
        await Task.Delay(500);  // Wait half a second
        countdown.Cancel();
        await Task.Delay(2000);  // Wait longer than the original countdown

        // Assert
        completedFired.Should().BeFalse();
    }

    [Fact]
    public void Dispose_StopsCountdown()
    {
        // Arrange
        var countdown = new AutoPlayCountdown();
        countdown.Start(5);

        // Act
        countdown.Dispose();

        // Assert
        countdown.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var countdown = new AutoPlayCountdown();

        // Act & Assert - should not throw
        countdown.Dispose();
        countdown.Dispose();
        countdown.Dispose();
    }
}
