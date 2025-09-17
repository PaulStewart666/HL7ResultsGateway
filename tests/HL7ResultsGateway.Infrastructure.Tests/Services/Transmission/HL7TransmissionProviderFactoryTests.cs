using FluentAssertions;

using HL7ResultsGateway.Domain.ValueObjects;
using HL7ResultsGateway.Infrastructure.Services.Transmission;

using Microsoft.Extensions.Logging;

using Moq;

namespace HL7ResultsGateway.Infrastructure.Tests.Services.Transmission;

public class HL7TransmissionProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<HL7TransmissionProviderFactory>> _mockLogger;
    private readonly Mock<ILogger<HttpHL7TransmissionProvider>> _mockHttpLogger;
    private readonly Mock<ILogger<MLLPTransmissionProvider>> _mockMllpLogger;
    private readonly Mock<ILogger<SftpTransmissionProvider>> _mockSftpLogger;
    private readonly HttpClient _httpClient;
    private readonly HL7TransmissionProviderFactory _factory;

    public HL7TransmissionProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<HL7TransmissionProviderFactory>>();
        _mockHttpLogger = new Mock<ILogger<HttpHL7TransmissionProvider>>();
        _mockMllpLogger = new Mock<ILogger<MLLPTransmissionProvider>>();
        _mockSftpLogger = new Mock<ILogger<SftpTransmissionProvider>>();
        _httpClient = new HttpClient();

        _factory = new HL7TransmissionProviderFactory(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HL7TransmissionProviderFactory(null!, _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HL7TransmissionProviderFactory(_mockServiceProvider.Object, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void CreateProvider_WithHttpProtocol_ShouldReturnHttpProvider()
    {
        // Arrange
        var expectedProvider = new HttpHL7TransmissionProvider(_mockHttpLogger.Object, _httpClient);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpHL7TransmissionProvider)))
            .Returns(expectedProvider);

        // Act
        var provider = _factory.CreateProvider(TransmissionProtocol.HTTP);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<HttpHL7TransmissionProvider>();
        provider.SupportedProtocol.Should().Be(TransmissionProtocol.HTTP);
    }

    [Fact]
    public void CreateProvider_WithHttpsProtocol_ShouldReturnHttpProvider()
    {
        // Arrange
        var expectedProvider = new HttpHL7TransmissionProvider(_mockHttpLogger.Object, _httpClient);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpHL7TransmissionProvider)))
            .Returns(expectedProvider);

        // Act
        var provider = _factory.CreateProvider(TransmissionProtocol.HTTPS);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<HttpHL7TransmissionProvider>();
        provider.SupportedProtocol.Should().Be(TransmissionProtocol.HTTP); // HTTP provider handles both HTTP and HTTPS
    }

    [Fact]
    public void CreateProvider_WithMllpProtocol_ShouldReturnMllpProvider()
    {
        // Arrange
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(ILogger<MLLPTransmissionProvider>)))
            .Returns(_mockMllpLogger.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpClient)))
            .Returns(_httpClient);

        // Act
        var provider = _factory.CreateProvider(TransmissionProtocol.MLLP);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<MLLPTransmissionProvider>();
        provider.SupportedProtocol.Should().Be(TransmissionProtocol.MLLP);
    }

    [Fact]
    public void CreateProvider_WithSftpProtocol_ShouldReturnSftpProvider()
    {
        // Arrange
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(ILogger<SftpTransmissionProvider>)))
            .Returns(_mockSftpLogger.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpClient)))
            .Returns(_httpClient);

        // Act
        var provider = _factory.CreateProvider(TransmissionProtocol.SFTP);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<SftpTransmissionProvider>();
        provider.SupportedProtocol.Should().Be(TransmissionProtocol.SFTP);
    }

    [Fact]
    public void CreateProvider_WithUnsupportedProtocol_ShouldThrowArgumentException()
    {
        // Arrange
        var unsupportedProtocol = (TransmissionProtocol)999;

        // Act & Assert
        var act = () => _factory.CreateProvider(unsupportedProtocol);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported transmission protocol*");
    }

    [Fact]
    public void CreateProvider_WithMissingDependency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(ILogger<HttpHL7TransmissionProvider>)))
            .Returns((object?)null); // Missing logger dependency
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpClient)))
            .Returns(_httpClient);

        // Act & Assert
        var act = () => _factory.CreateProvider(TransmissionProtocol.HTTP);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed to create HTTP transmission provider*");
    }

    [Theory]
    [InlineData(TransmissionProtocol.HTTP)]
    [InlineData(TransmissionProtocol.HTTPS)]
    [InlineData(TransmissionProtocol.MLLP)]
    [InlineData(TransmissionProtocol.SFTP)]
    public void CreateProvider_MultipleCalls_ShouldReturnNewInstances(TransmissionProtocol protocol)
    {
        // Arrange
        SetupServiceProviderForProtocol(protocol);

        // Act
        var provider1 = _factory.CreateProvider(protocol);
        var provider2 = _factory.CreateProvider(protocol);

        // Assert
        provider1.Should().NotBeNull();
        provider2.Should().NotBeNull();
        provider1.Should().NotBeSameAs(provider2); // Factory should create new instances
    }

    private void SetupServiceProviderForProtocol(TransmissionProtocol protocol)
    {
        switch (protocol)
        {
            case TransmissionProtocol.HTTP:
            case TransmissionProtocol.HTTPS:
                _mockServiceProvider
                    .Setup(x => x.GetService(typeof(ILogger<HttpHL7TransmissionProvider>)))
                    .Returns(_mockHttpLogger.Object);
                break;
            case TransmissionProtocol.MLLP:
                _mockServiceProvider
                    .Setup(x => x.GetService(typeof(ILogger<MLLPTransmissionProvider>)))
                    .Returns(_mockMllpLogger.Object);
                break;
            case TransmissionProtocol.SFTP:
                _mockServiceProvider
                    .Setup(x => x.GetService(typeof(ILogger<SftpTransmissionProvider>)))
                    .Returns(_mockSftpLogger.Object);
                break;
        }

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(HttpClient)))
            .Returns(_httpClient);
    }
}
