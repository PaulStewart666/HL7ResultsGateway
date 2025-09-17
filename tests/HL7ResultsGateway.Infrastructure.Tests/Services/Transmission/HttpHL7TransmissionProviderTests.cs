using FluentAssertions;

using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;
using HL7ResultsGateway.Infrastructure.Services.Transmission;

using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

using System.Net;

namespace HL7ResultsGateway.Infrastructure.Tests.Services.Transmission;

public class HttpHL7TransmissionProviderTests
{
    private readonly Mock<ILogger<HttpHL7TransmissionProvider>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HttpHL7TransmissionProvider _provider;

    public HttpHL7TransmissionProviderTests()
    {
        _mockLogger = new Mock<ILogger<HttpHL7TransmissionProvider>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _provider = new HttpHL7TransmissionProvider(_mockLogger.Object, _httpClient);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HttpHL7TransmissionProvider(null!, _httpClient);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HttpHL7TransmissionProvider(_mockLogger.Object, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void SupportedProtocol_ShouldReturnHTTP()
    {
        // Act & Assert
        _provider.SupportedProtocol.Should().Be(TransmissionProtocol.HTTP);
    }

    [Fact]
    public void ProviderName_ShouldReturnCorrectName()
    {
        // Act & Assert
        _provider.ProviderName.Should().Be("HTTP/HTTPS Transmission Provider");
    }

    [Fact]
    public async Task SendMessageAsync_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var expectedResponse = "MSA|AA|MSG001|Message accepted";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _provider.SendMessageAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransmissionId.Should().NotBeEmpty();
        result.AcknowledgmentMessage.Should().Be(expectedResponse);
        result.ErrorMessage.Should().BeNull();
        result.ResponseTime.Should().BePositive();
    }

    [Fact]
    public async Task SendMessageAsync_WithHttpError_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Invalid HL7 format")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _provider.SendMessageAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("BadRequest");
        result.AcknowledgmentMessage.Should().BeNull();
    }

    [Fact]
    public async Task SendMessageAsync_WithNetworkException_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var expectedException = new HttpRequestException("Network error");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act
        var result = await _provider.SendMessageAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Network error");
    }

    [Fact]
    public async Task SendMessageAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var request = CreateValidRequest();
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        var act = () => _provider.SendMessageAsync(request, cancellationToken);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendMessageAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _provider.SendMessageAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-url")]
    [InlineData("ftp://invalid.com")]
    public async Task SendMessageAsync_WithInvalidEndpoint_ShouldReturnFailure(string invalidEndpoint)
    {
        // Arrange
        var request = new HL7TransmissionRequest(
            invalidEndpoint,
            "MSH|^~\\&|TEST|FAC|REC|FAC|20240917120000||ORU^R01|MSG001|P|2.5.1",
            new Dictionary<string, string>(),
            30,
            TransmissionProtocol.HTTP
        );

        // Act
        var result = await _provider.SendMessageAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SendMessageAsync_WithCustomHeaders_ShouldIncludeHeaders()
    {
        // Arrange
        var customHeaders = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token123" },
            { "X-Custom-Header", "CustomValue" }
        };

        var request = new HL7TransmissionRequest(
            "https://api.example.com/hl7",
            "MSH|^~\\&|TEST|FAC|REC|FAC|20240917120000||ORU^R01|MSG001|P|2.5.1",
            customHeaders,
            30,
            TransmissionProtocol.HTTP
        );

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("MSA|AA|MSG001")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse)
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req);

        // Act
        var result = await _provider.SendMessageAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().Contain(h => h.Key == "Authorization");
        capturedRequest.Headers.Should().Contain(h => h.Key == "X-Custom-Header");
    }

    [Fact]
    public async Task ValidateEndpointAsync_WithValidHttpsUrl_ShouldReturnTrue()
    {
        // Arrange
        var endpoint = "https://api.example.com/hl7";

        // Act
        var result = await _provider.ValidateEndpointAsync(endpoint, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-url")]
    [InlineData("ftp://invalid.com")]
    [InlineData("file://local-file")]
    public async Task ValidateEndpointAsync_WithInvalidUrl_ShouldReturnFalse(string invalidUrl)
    {
        // Act
        var result = await _provider.ValidateEndpointAsync(invalidUrl, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WithReachableEndpoint_ShouldReturnTrue()
    {
        // Arrange
        var endpoint = "https://api.example.com/health";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _provider.TestConnectionAsync(endpoint, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithUnreachableEndpoint_ShouldReturnFalse()
    {
        // Arrange
        var endpoint = "https://unreachable.example.com/health";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _provider.TestConnectionAsync(endpoint, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    private static HL7TransmissionRequest CreateValidRequest()
    {
        return new HL7TransmissionRequest(
            "https://api.example.com/hl7",
            "MSH|^~\\&|TEST|FAC|REC|FAC|20240917120000||ORU^R01|MSG001|P|2.5.1\r\nPID|1||12345||Doe^John||19800101|M\r\nOBX|1|ST|TEST||Normal||||||F",
            new Dictionary<string, string> { { "Content-Type", "application/x-hl7" } },
            30,
            TransmissionProtocol.HTTP
        );
    }
}
