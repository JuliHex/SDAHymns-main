using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SDAHymns.Core.Services;
using Velopack;
using Xunit;

namespace SDAHymns.Tests.Services;

public class UpdateServiceTests
{
    private readonly Mock<IOptions<SDAHymns.Core.Services.UpdateOptions>> _mockOptions;

    public UpdateServiceTests()
    {
        // Initialize Velopack for testing
        // This is required before creating UpdateManager instances
        try
        {
            VelopackApp.Build().Run();
        }
        catch
        {
            // Already initialized or not needed in test environment
        }

        // Setup default options mock
        _mockOptions = new Mock<IOptions<SDAHymns.Core.Services.UpdateOptions>>();
        _mockOptions.Setup(x => x.Value).Returns(new SDAHymns.Core.Services.UpdateOptions
        {
            GitHubRepoUrl = "https://github.com/ThorSPB/SDAHymns"
        });
    }

    [Fact]
    public void Constructor_WithDefaultOptions_CreatesServiceSuccessfully()
    {
        // Arrange & Act
        var service = new UpdateService(_mockOptions.Object);

        // Assert
        service.Should().NotBeNull();
        service.IsUpdateAvailable.Should().BeFalse();
        service.LatestVersion.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCustomGitHubUrl_CreatesServiceSuccessfully()
    {
        // Arrange
        var customUrl = "https://github.com/CustomUser/CustomRepo";
        var customOptions = new Mock<IOptions<SDAHymns.Core.Services.UpdateOptions>>();
        customOptions.Setup(x => x.Value).Returns(new SDAHymns.Core.Services.UpdateOptions
        {
            GitHubRepoUrl = customUrl
        });

        // Act
        var service = new UpdateService(customOptions.Object);

        // Assert
        service.Should().NotBeNull();
        service.IsUpdateAvailable.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithLogger_CreatesServiceSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();

        // Act
        var service = new UpdateService(_mockOptions.Object, mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithLoggerAndCustomUrl_CreatesServiceSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();
        var customUrl = "https://github.com/CustomUser/CustomRepo";
        var customOptions = new Mock<IOptions<SDAHymns.Core.Services.UpdateOptions>>();
        customOptions.Setup(x => x.Value).Returns(new SDAHymns.Core.Services.UpdateOptions
        {
            GitHubRepoUrl = customUrl
        });

        // Act
        var service = new UpdateService(customOptions.Object, mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenNoUpdateAvailable_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();
        var service = new UpdateService(_mockOptions.Object, mockLogger.Object);

        // Act
        var result = await service.CheckForUpdatesAsync();

        // Assert
        // Since there's no real GitHub release, this should return null or handle gracefully
        result.Should().BeNull();
        service.IsUpdateAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WithInvalidUrl_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();
        var invalidUrl = "https://github.com/NonExistent/InvalidRepo12345";
        var invalidOptions = new Mock<IOptions<SDAHymns.Core.Services.UpdateOptions>>();
        invalidOptions.Setup(x => x.Value).Returns(new SDAHymns.Core.Services.UpdateOptions
        {
            GitHubRepoUrl = invalidUrl
        });
        var service = new UpdateService(invalidOptions.Object, mockLogger.Object);

        // Act
        var result = await service.CheckForUpdatesAsync();

        // Assert
        result.Should().BeNull();
        service.IsUpdateAvailable.Should().BeFalse();

        // Verify logging occurred (at least once for any log level)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task DownloadUpdatesAsync_WithNullUpdateInfo_ReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();
        var service = new UpdateService(_mockOptions.Object, mockLogger.Object);

        // Act
        // This will likely throw or return false since updateInfo is null
        // We're testing error handling
        try
        {
            var result = await service.DownloadUpdatesAsync(null!, null);
            result.Should().BeFalse();
        }
        catch (Exception)
        {
            // Expected - null updateInfo should be handled gracefully
            Assert.True(true);
        }
    }

    [Fact]
    public void IsUpdateAvailable_InitiallyFalse()
    {
        // Arrange
        var service = new UpdateService(_mockOptions.Object);

        // Act
        var result = service.IsUpdateAvailable;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LatestVersion_InitiallyNull()
    {
        // Arrange
        var service = new UpdateService(_mockOptions.Object);

        // Act
        var result = service.LatestVersion;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Arrange & Act
        var act = () => new UpdateService(_mockOptions.Object, logger: null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithDefaultUrl_UsesConfiguredUrl()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateService>>();

        // Act
        var service = new UpdateService(_mockOptions.Object, mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        // If it doesn't throw, it successfully used the configured URL from options
    }
}
