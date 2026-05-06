using FluentAssertions;
using Moq;
using SDAHymns.Core.Services;
using Xunit;

namespace SDAHymns.Tests.Services;

public class TimingRecorderTests
{
    private readonly Mock<IAudioPlayerService> _mockAudioPlayer;
    private readonly TimingRecorder _recorder;

    public TimingRecorderTests()
    {
        _mockAudioPlayer = new Mock<IAudioPlayerService>();
        _recorder = new TimingRecorder(_mockAudioPlayer.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenAudioPlayerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TimingRecorder(null!));
    }

    [Fact]
    public void StartRecording_InitializesRecordingState()
    {
        // Act
        _recorder.StartRecording();

        // Assert
        _recorder.IsRecording.Should().BeTrue();
        _recorder.TimingCount.Should().Be(0);
        _recorder.NextVerseNumber.Should().Be(1);
    }

    [Fact]
    public void StartRecording_ClearsPreviousTimings()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.StartRecording();
        _recorder.RecordTiming();

        // Act
        _recorder.StartRecording();

        // Assert
        _recorder.TimingCount.Should().Be(0);
        _recorder.NextVerseNumber.Should().Be(1);
    }

    [Fact]
    public void StopRecording_SetsIsRecordingToFalse()
    {
        // Arrange
        _recorder.StartRecording();

        // Act
        _recorder.StopRecording();

        // Assert
        _recorder.IsRecording.Should().BeFalse();
    }

    [Fact]
    public void RecordTiming_ThrowsInvalidOperationException_WhenNotRecording()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _recorder.RecordTiming());
    }

    [Fact]
    public void RecordTiming_RecordsCurrentTimeForNextVerseNumber()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(12.5));
        _recorder.StartRecording();

        // Act
        _recorder.RecordTiming();

        // Assert
        _recorder.TimingCount.Should().Be(1);
        _recorder.GetTiming(1).Should().Be(12.5);
        _recorder.NextVerseNumber.Should().Be(2);
    }

    [Fact]
    public void RecordTiming_RecordsMultipleTimings()
    {
        // Arrange
        _recorder.StartRecording();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.RecordTiming();

        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(25));
        _recorder.RecordTiming();

        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(42));
        _recorder.RecordTiming();

        // Assert
        _recorder.TimingCount.Should().Be(3);
        _recorder.GetTiming(1).Should().Be(10);
        _recorder.GetTiming(2).Should().Be(25);
        _recorder.GetTiming(3).Should().Be(42);
        _recorder.NextVerseNumber.Should().Be(4);
    }

    [Fact]
    public void RecordTiming_FiresTimingRecordedEvent()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(15));
        _recorder.StartRecording();

        var eventFired = false;
        var recordedVerseNumber = 0;
        _recorder.TimingRecorded += (sender, verseNumber) =>
        {
            eventFired = true;
            recordedVerseNumber = verseNumber;
        };

        // Act
        _recorder.RecordTiming();

        // Assert
        eventFired.Should().BeTrue();
        recordedVerseNumber.Should().Be(1);
    }

    [Fact]
    public void RecordTimingWithVerseNumber_RecordsSpecificVerseNumber()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(30));
        _recorder.StartRecording();

        // Act
        _recorder.RecordTiming(5);

        // Assert
        _recorder.TimingCount.Should().Be(1);
        _recorder.GetTiming(5).Should().Be(30);
        _recorder.NextVerseNumber.Should().Be(6);
    }

    [Fact]
    public void RecordTimingWithVerseNumber_ThrowsArgumentException_WhenVerseNumberIsZeroOrNegative()
    {
        // Arrange
        _recorder.StartRecording();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _recorder.RecordTiming(0));
        Assert.Throws<ArgumentException>(() => _recorder.RecordTiming(-1));
    }

    [Fact]
    public void RecordTimingWithVerseNumber_DoesNotChangeNextVerseNumber_WhenRecordingLowerNumber()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.StartRecording();
        _recorder.RecordTiming(5);  // NextVerseNumber becomes 6

        // Act
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(5));
        _recorder.RecordTiming(2);  // Recording verse 2 (earlier verse)

        // Assert
        _recorder.NextVerseNumber.Should().Be(6);  // Should not change
        _recorder.GetTiming(2).Should().Be(5);
    }

    [Fact]
    public void RemoveTiming_RemovesRecordedTiming()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.StartRecording();
        _recorder.RecordTiming();
        _recorder.RecordTiming();

        // Act
        _recorder.RemoveTiming(1);

        // Assert
        _recorder.TimingCount.Should().Be(1);
        _recorder.GetTiming(1).Should().BeNull();
        _recorder.GetTiming(2).Should().NotBeNull();
    }

    [Fact]
    public void ClearTimings_RemovesAllTimingsAndResetsState()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.StartRecording();
        _recorder.RecordTiming();
        _recorder.RecordTiming();
        _recorder.RecordTiming();

        // Act
        _recorder.ClearTimings();

        // Assert
        _recorder.TimingCount.Should().Be(0);
        _recorder.NextVerseNumber.Should().Be(1);
    }

    [Fact]
    public void GetAllTimings_ReturnsAllRecordedTimings()
    {
        // Arrange
        _recorder.StartRecording();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.RecordTiming();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(25));
        _recorder.RecordTiming();

        // Act
        var timings = _recorder.GetAllTimings();

        // Assert
        timings.Should().HaveCount(2);
        timings[1].Should().Be(10);
        timings[2].Should().Be(25);
    }

    [Fact]
    public void GetTimingMapJson_ReturnsEmptyObject_WhenNoTimings()
    {
        // Act
        var json = _recorder.GetTimingMapJson();

        // Assert
        json.Should().Be("{}");
    }

    [Fact]
    public void GetTimingMapJson_ReturnsValidJson_WithTimings()
    {
        // Arrange
        _recorder.StartRecording();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(12.5));
        _recorder.RecordTiming();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(45.2));
        _recorder.RecordTiming();

        // Act
        var json = _recorder.GetTimingMapJson();

        // Assert
        json.Should().Contain("\"1\"");
        json.Should().Contain("12.5");
        json.Should().Contain("\"2\"");
        json.Should().Contain("45.2");
    }

    [Fact]
    public void LoadTimingMap_LoadsTimingsFromValidJson()
    {
        // Arrange
        var json = "{\"1\":12.5,\"2\":45.2,\"3\":88.0}";

        // Act
        _recorder.LoadTimingMap(json);

        // Assert
        _recorder.TimingCount.Should().Be(3);
        _recorder.GetTiming(1).Should().Be(12.5);
        _recorder.GetTiming(2).Should().Be(45.2);
        _recorder.GetTiming(3).Should().Be(88.0);
        _recorder.NextVerseNumber.Should().Be(4);
    }

    [Fact]
    public void LoadTimingMap_ClearsPreviousTimings()
    {
        // Arrange
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(10));
        _recorder.StartRecording();
        _recorder.RecordTiming();

        var json = "{\"1\":20.0}";

        // Act
        _recorder.LoadTimingMap(json);

        // Assert
        _recorder.TimingCount.Should().Be(1);
        _recorder.GetTiming(1).Should().Be(20.0);
    }

    [Fact]
    public void LoadTimingMap_HandlesNullOrEmptyJson()
    {
        // Act & Assert
        _recorder.LoadTimingMap(null);
        _recorder.TimingCount.Should().Be(0);

        _recorder.LoadTimingMap("");
        _recorder.TimingCount.Should().Be(0);

        _recorder.LoadTimingMap("   ");
        _recorder.TimingCount.Should().Be(0);
    }

    [Fact]
    public void LoadTimingMap_HandlesInvalidJson()
    {
        // Act
        _recorder.LoadTimingMap("{invalid json}");

        // Assert
        _recorder.TimingCount.Should().Be(0);
    }

    [Fact]
    public void LoadTimingMap_IgnoresInvalidVerseNumbers()
    {
        // Arrange
        var json = "{\"1\":10.0,\"invalid\":20.0,\"2\":30.0}";

        // Act
        _recorder.LoadTimingMap(json);

        // Assert
        _recorder.TimingCount.Should().Be(2);
        _recorder.GetTiming(1).Should().Be(10.0);
        _recorder.GetTiming(2).Should().Be(30.0);
    }

    [Fact]
    public void RoundTrip_SaveAndLoadTimingMap_PreservesData()
    {
        // Arrange
        _recorder.StartRecording();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(12.5));
        _recorder.RecordTiming();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(45.2));
        _recorder.RecordTiming();
        _mockAudioPlayer.Setup(p => p.CurrentTime).Returns(TimeSpan.FromSeconds(88.0));
        _recorder.RecordTiming();

        // Act
        var json = _recorder.GetTimingMapJson();
        var newRecorder = new TimingRecorder(_mockAudioPlayer.Object);
        newRecorder.LoadTimingMap(json);

        // Assert
        newRecorder.TimingCount.Should().Be(3);
        newRecorder.GetTiming(1).Should().Be(12.5);
        newRecorder.GetTiming(2).Should().Be(45.2);
        newRecorder.GetTiming(3).Should().Be(88.0);
    }
}
