using FluentAssertions;
using Moq;
using NuGet.Versioning;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.ViewModels;
using Velopack;
using Velopack.Sources;
using Xunit;

namespace SDAHymns.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly Mock<IHymnDisplayService> _mockHymnService;
    private readonly Mock<IUpdateService> _mockUpdateService;
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly Mock<IDisplayProfileService> _mockProfileService;
    private readonly Mock<IAudioPlayerService> _mockAudioPlayer;
    private readonly Mock<ISettingsService> _mockSettingsService;

    public MainWindowViewModelTests()
    {
        _mockHymnService = new Mock<IHymnDisplayService>();
        _mockUpdateService = new Mock<IUpdateService>();
        _mockSearchService = new Mock<ISearchService>();
        _mockProfileService = new Mock<IDisplayProfileService>();
        _mockAudioPlayer = new Mock<IAudioPlayerService>();
        _mockSettingsService = new Mock<ISettingsService>();
    }

    [Fact]
    public void Constructor_InitializesWithNoUpdateAvailable()
    {
        // Arrange & Act
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeFalse();
        viewModel.LatestVersion.Should().BeNull();
        viewModel.IsDownloadingUpdate.Should().BeFalse();
        viewModel.DownloadProgress.Should().Be(0);
    }

    [Fact]
    public void ShowUpdateNotification_SetsPropertiesCorrectly()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");

        // Act
        viewModel.ShowUpdateNotification(mockUpdateInfo);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeTrue();
        viewModel.LatestVersion.Should().Be("1.2.3");
    }

    [Fact]
    public void DismissUpdate_HidesBanner()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");
        viewModel.ShowUpdateNotification(mockUpdateInfo);

        // Act
        viewModel.DismissUpdateCommand.Execute(null);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNowAsync_WithSuccessfulDownload_CallsApplyUpdatesAndRestart()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");
        viewModel.ShowUpdateNotification(mockUpdateInfo);

        _mockUpdateService
            .Setup(x => x.DownloadUpdatesAsync(It.IsAny<UpdateInfo>(), It.IsAny<IProgress<int>>()))
            .ReturnsAsync(true);

        _mockUpdateService
            .Setup(x => x.ApplyUpdatesAndRestart(It.IsAny<UpdateInfo>()));

        // Act
        await viewModel.UpdateNowCommand.ExecuteAsync(null);

        // Assert
        _mockUpdateService.Verify(
            x => x.DownloadUpdatesAsync(mockUpdateInfo, It.IsAny<IProgress<int>>()),
            Times.Once);

        _mockUpdateService.Verify(
            x => x.ApplyUpdatesAndRestart(mockUpdateInfo),
            Times.Once);
    }

    [Fact]
    public async Task UpdateNowAsync_WithFailedDownload_KeepsBannerVisible()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");
        viewModel.ShowUpdateNotification(mockUpdateInfo);

        _mockUpdateService
            .Setup(x => x.DownloadUpdatesAsync(It.IsAny<UpdateInfo>(), It.IsAny<IProgress<int>>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.UpdateNowCommand.ExecuteAsync(null);

        // Assert - Banner should stay visible for retry (our UX fix!)
        viewModel.IsUpdateAvailable.Should().BeTrue();
        viewModel.IsDownloadingUpdate.Should().BeFalse();
        viewModel.StatusMessage.Should().Contain("Failed to download update");
    }

    [Fact]
    public async Task UpdateNowAsync_WithFailedDownload_ResetsDownloadingState()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");
        viewModel.ShowUpdateNotification(mockUpdateInfo);

        _mockUpdateService
            .Setup(x => x.DownloadUpdatesAsync(It.IsAny<UpdateInfo>(), It.IsAny<IProgress<int>>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.UpdateNowCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsDownloadingUpdate.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNowAsync_WithNoPendingUpdate_DoesNothing()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        // Don't call ShowUpdateNotification - no pending update

        // Act
        await viewModel.UpdateNowCommand.ExecuteAsync(null);

        // Assert
        _mockUpdateService.Verify(
            x => x.DownloadUpdatesAsync(It.IsAny<UpdateInfo>(), It.IsAny<IProgress<int>>()),
            Times.Never);
    }

    [Fact]
    public void ShowUpdateNotification_WithDifferentVersions_UpdatesLatestVersion()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);

        // Act
        var update1 = CreateMockUpdateInfo("1.0.0");
        viewModel.ShowUpdateNotification(update1);
        var version1 = viewModel.LatestVersion;

        var update2 = CreateMockUpdateInfo("2.0.0");
        viewModel.ShowUpdateNotification(update2);
        var version2 = viewModel.LatestVersion;

        // Assert
        version1.Should().Be("1.0.0");
        version2.Should().Be("2.0.0");
    }

    [Fact]
    public void IsUpdateAvailable_AfterDismiss_BecomesFalse()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mockHymnService.Object, _mockUpdateService.Object, _mockSearchService.Object, _mockProfileService.Object, _mockAudioPlayer.Object, _mockSettingsService.Object);
        var mockUpdateInfo = CreateMockUpdateInfo("1.2.3");
        viewModel.ShowUpdateNotification(mockUpdateInfo);
        viewModel.IsUpdateAvailable.Should().BeTrue();

        // Act
        viewModel.DismissUpdateCommand.Execute(null);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeFalse();
    }

    /// <summary>
    /// Helper method to create a mock UpdateInfo for testing
    /// </summary>
    private static UpdateInfo CreateMockUpdateInfo(string version)
    {
        // Create a minimal UpdateInfo for testing purposes
        // In real scenarios, this comes from Velopack's GitHub check
        var nugetVersion = NuGetVersion.Parse(version);
        var release = new VelopackAsset
        {
            PackageId = "SDAHymns",
            Version = nugetVersion,
            Type = VelopackAssetType.Full,
            FileName = $"SDAHymns-{version}-full.nupkg",
            SHA1 = "mock-sha1",
            Size = 1024
        };

        return new UpdateInfo(release, false);
    }
}
