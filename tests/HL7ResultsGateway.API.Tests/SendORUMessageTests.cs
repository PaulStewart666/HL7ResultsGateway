using System.Net;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using HL7ResultsGateway.API;
using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Moq;

namespace HL7ResultsGateway.API.Tests;

public class SendORUMessageTests
{
    private readonly Mock<ISendORUMessageHandler> _mockHandler;
    private readonly Mock<IResponseDTOFactory> _mockResponseFactory;
    private readonly Mock<SendORURequestValidator> _mockValidator;
    private readonly Mock<ILogger<SendORUMessage>> _mockLogger;
    private readonly SendORUMessage _function;

    public SendORUMessageTests()
    {
        _mockHandler = new Mock<ISendORUMessageHandler>();
        _mockResponseFactory = new Mock<IResponseDTOFactory>();
        _mockValidator = new Mock<SendORURequestValidator>();
        _mockLogger = new Mock<ILogger<SendORUMessage>>();
        _function = new SendORUMessage(
            _mockHandler.Object,
            _mockResponseFactory.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessage(
            null!,
            _mockResponseFactory.Object,
            _mockValidator.Object,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("handler");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessage(
            _mockHandler.Object,
            _mockResponseFactory.Object,
            _mockValidator.Object,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Run_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var requestDto = new SendORURequestDTO
        {
            Endpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTP,
            PatientId = "12345",
            TimeoutSeconds = 30,
            // Add other properties as required by the actual DTO
        };

        var handlerResult = SendORUMessageResult.CreateSuccess(
            "TRANS001",
            "ACK received",
            TimeSpan.FromSeconds(1.5),
            requestDto.Endpoint,
            requestDto.Protocol,
            requestDto.PatientId
        );

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequestData = CreateMockHttpRequestData(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(handlerResult);

        // Act
        var response = await _function.Run(httpRequestData.Object, Mock.Of<FunctionContext>());

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{invalid json}";
        var httpRequestData = CreateMockHttpRequestData(invalidJson);

        // Act
        var response = await _function.Run(httpRequestData.Object, Mock.Of<FunctionContext>());

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithEmptyRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var httpRequestData = CreateMockHttpRequestData("");

        // Act
        var response = await _function.Run(httpRequestData.Object, Mock.Of<FunctionContext>());

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithHandlerFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var requestDto = new SendORURequestDTO
        {
            Endpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTP,
            PatientId = "12345"
        };

        var handlerResult = SendORUMessageResult.CreateFailure("Connection timeout", requestDto.Endpoint, requestDto.Protocol, requestDto.PatientId);

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequestData = CreateMockHttpRequestData(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(handlerResult);

        // Act
        var response = await _function.Run(httpRequestData.Object, Mock.Of<FunctionContext>());

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithUnhandledException_ShouldReturnInternalServerError()
    {
        // Arrange
        var requestDto = new SendORURequestDTO
        {
            Endpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTP,
            PatientId = "12345"
        };

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequestData = CreateMockHttpRequestData(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await _function.Run(httpRequestData.Object, Mock.Of<FunctionContext>());

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private static Mock<HttpRequestData> CreateMockHttpRequestData(string body)
    {
        var mockRequest = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var bodyStream = new MemoryStream(bodyBytes);

        mockRequest.Setup(x => x.Body).Returns(bodyStream);
        mockRequest.Setup(x => x.Method).Returns("POST");
        mockRequest.Setup(x => x.Url).Returns(new Uri("https://localhost:7071/api/send-oru"));

        // Use HttpHeadersCollection for headers
        var headers = new HttpHeadersCollection();
        headers.Add("Content-Type", "application/json");
        mockRequest.Setup(x => x.Headers).Returns(headers);

        var mockResponse = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        mockResponse.SetupProperty(x => x.StatusCode);
        mockResponse.Setup(x => x.Headers).Returns(new HttpHeadersCollection());

        var responseBodyStream = new MemoryStream();
        mockResponse.Setup(x => x.Body).Returns(responseBodyStream);

        mockRequest.Setup(x => x.CreateResponse()).Returns(mockResponse.Object);

        return mockRequest;
    }
}
