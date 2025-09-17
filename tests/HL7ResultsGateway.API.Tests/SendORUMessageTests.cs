using System.Text;
using System.Text.Json;

using FluentAssertions;

using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using HL7ResultsGateway.API.Factories;
using HL7ResultsGateway.Application.Validators;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging;

using Moq;

namespace HL7ResultsGateway.API.Tests;

public class SendORUMessageTests
{
    private readonly Mock<ISendORUMessageHandler> _mockHandler;
    private readonly Mock<IResponseDTOFactory> _mockResponseFactory;
    private readonly SendORURequestValidator _validator;
    private readonly Mock<ILogger<SendORUMessage>> _mockLogger;
    private readonly SendORUMessage _function;

    public SendORUMessageTests()
    {
        _mockHandler = new Mock<ISendORUMessageHandler>();
        _mockResponseFactory = new Mock<IResponseDTOFactory>();
        _validator = new SendORURequestValidator(); // Use real validator - it should pass with valid test data
        _mockLogger = new Mock<ILogger<SendORUMessage>>();

        // Setup the response factory to always return a non-null ApiResponse<SendORUResponseDTO>

        // Mock the factory to behave like the real implementation - always returns HTTP 200 but DTO reflects business result
        _mockResponseFactory.Setup(f => f.CreateSuccessResponse(It.IsAny<SendORUMessageResult>(), It.IsAny<string?>()))
            .Returns((SendORUMessageResult result, string? correlationId) =>
                HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>.CreateSuccess(
                    SendORUResponseDTO.FromSuccessResult(result), 200, correlationId));

        _mockResponseFactory.Setup(f => f.CreateErrorResponse(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string errorMessage, int statusCode, string? errorDetails, string? correlationId) =>
                HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>.CreateError(
                    errorMessage, statusCode, errorDetails, correlationId));

        _mockResponseFactory.Setup(f => f.CreateValidationErrorResponse(It.IsAny<IEnumerable<string>>(), It.IsAny<string?>()))
            .Returns((IEnumerable<string> errors, string? correlationId) =>
                HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>.CreateValidationError(
                    errors, correlationId));

        _mockResponseFactory.Setup(f => f.CreateInternalServerErrorResponse(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? errorMessage, string? errorDetails, string? correlationId) =>
                HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>.CreateInternalServerError(
                    errorMessage ?? "An internal server error occurred", errorDetails, correlationId));

        _mockResponseFactory.Setup(f => f.CreateExceptionResponse(It.IsAny<Exception>(), It.IsAny<string?>()))
            .Returns((Exception ex, string? correlationId) =>
                HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>.CreateInternalServerError(
                    ex.Message, ex.ToString(), correlationId));

        _function = new SendORUMessage(
            _mockHandler.Object,
            _mockResponseFactory.Object,
            _validator,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessage(
            null!,
            _mockResponseFactory.Object,
            _validator,
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
            _validator,
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
            DestinationEndpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTPS, // Match HTTPS endpoint
            TimeoutSeconds = 30,
            MessageData = new HL7ResultsGateway.Domain.Entities.HL7Result
            {
                MessageType = HL7ResultsGateway.Domain.ValueObjects.HL7MessageType.ORU_R01, // Required message type
                Patient = new HL7ResultsGateway.Domain.Entities.Patient
                {
                    PatientId = "12345",
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = HL7ResultsGateway.Domain.ValueObjects.Gender.Male,
                    DateOfBirth = new DateTime(1980, 1, 1)
                },
                Observations = new List<HL7ResultsGateway.Domain.Entities.Observation>
                {
                    new()
                    {
                        ObservationId = "OBS001",
                        Description = "Test Result",
                        Value = "Normal",
                        Status = HL7ResultsGateway.Domain.ValueObjects.ObservationStatus.Normal,
                        ValueType = "ST"
                    }
                }
            },
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } },
            Source = "UnitTest",
            Priority = HL7ResultsGateway.Application.DTOs.TransmissionPriority.Normal
        };

        var transmissionResult = HL7ResultsGateway.Domain.Models.TransmissionResult.CreateSuccess(
            "TRANS001",
            "ACK received",
            TimeSpan.FromSeconds(1.5));
        var handlerResult = SendORUMessageResult.CreateSuccess(
            "TRANS001",
            transmissionResult,
            "AUDIT001",
            requestDto.DestinationEndpoint,
            requestDto.Protocol,
            requestDto.Source ?? "UnitTest");

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequest = CreateHttpRequest(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(handlerResult);

        // Act
        var response = await _function.Run(httpRequest, default);

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        var okResult = response as Microsoft.AspNetCore.Mvc.OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        // Verify the response structure
        var apiResponse = okResult.Value as HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = "{invalid json}";
        var httpRequest = CreateHttpRequest(invalidJson);

        // Act
        var response = await _function.Run(httpRequest, default);

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
        var badRequestResult = response as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithEmptyRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var httpRequest = CreateHttpRequest("");

        // Act
        var response = await _function.Run(httpRequest, default);

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
        var badRequestResult = response as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_WithHandlerFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var requestDto = new SendORURequestDTO
        {
            DestinationEndpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTPS, // Match HTTPS endpoint
            TimeoutSeconds = 30,
            MessageData = new HL7ResultsGateway.Domain.Entities.HL7Result
            {
                MessageType = HL7ResultsGateway.Domain.ValueObjects.HL7MessageType.ORU_R01, // Required message type
                Patient = new HL7ResultsGateway.Domain.Entities.Patient
                {
                    PatientId = "12345",
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = HL7ResultsGateway.Domain.ValueObjects.Gender.Male,
                    DateOfBirth = new DateTime(1980, 1, 1)
                },
                Observations = new List<HL7ResultsGateway.Domain.Entities.Observation>
                {
                    new()
                    {
                        ObservationId = "OBS001",
                        Description = "Test Result",
                        Value = "Normal",
                        Status = HL7ResultsGateway.Domain.ValueObjects.ObservationStatus.Normal,
                        ValueType = "ST"
                    }
                }
            },
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } },
            Source = "UnitTest",
            Priority = HL7ResultsGateway.Application.DTOs.TransmissionPriority.Normal
        };

        var handlerResult = SendORUMessageResult.CreateFailure(
            "TRANS002",
            "Connection timeout",
            requestDto.DestinationEndpoint,
            requestDto.Protocol,
            requestDto.Source ?? "UnitTest");

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequest = CreateHttpRequest(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(handlerResult);

        // Act
        var response = await _function.Run(httpRequest, default);

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        var okResult = response as Microsoft.AspNetCore.Mvc.OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        // Verify the response indicates failure
        var apiResponse = okResult.Value as HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>;
        apiResponse.Should().NotBeNull();

        // HTTP API call succeeded (got valid response)
        apiResponse!.Success.Should().BeTrue();

        // Business operation failed (transmission timeout)
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Success.Should().BeFalse();
        apiResponse.Data.ErrorMessage.Should().Contain("Connection timeout");
        _mockHandler.Verify(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithUnhandledException_ShouldReturnInternalServerError()
    {
        // Arrange
        var requestDto = new SendORURequestDTO
        {
            DestinationEndpoint = "https://api.example.com/hl7",
            Protocol = HL7ResultsGateway.Domain.ValueObjects.TransmissionProtocol.HTTPS, // Match HTTPS endpoint
            TimeoutSeconds = 30,
            MessageData = new HL7ResultsGateway.Domain.Entities.HL7Result
            {
                MessageType = HL7ResultsGateway.Domain.ValueObjects.HL7MessageType.ORU_R01, // Required message type
                Patient = new HL7ResultsGateway.Domain.Entities.Patient
                {
                    PatientId = "12345",
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = HL7ResultsGateway.Domain.ValueObjects.Gender.Male,
                    DateOfBirth = new DateTime(1980, 1, 1)
                },
                Observations = new List<HL7ResultsGateway.Domain.Entities.Observation>
                {
                    new()
                    {
                        ObservationId = "OBS001",
                        Description = "Test Result",
                        Value = "Normal",
                        Status = HL7ResultsGateway.Domain.ValueObjects.ObservationStatus.Normal,
                        ValueType = "ST"
                    }
                }
            },
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } },
            Source = "UnitTest",
            Priority = HL7ResultsGateway.Application.DTOs.TransmissionPriority.Normal
        };

        var jsonContent = JsonSerializer.Serialize(requestDto);
        var httpRequest = CreateHttpRequest(jsonContent);

        _mockHandler
            .Setup(x => x.Handle(It.IsAny<SendORUMessageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await _function.Run(httpRequest, default);

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Mvc.ObjectResult>();
        var objectResult = response as Microsoft.AspNetCore.Mvc.ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().NotBeNull();
        // Verify error response structure
        var apiResponse = objectResult.Value as HL7ResultsGateway.API.Models.ApiResponse<SendORUResponseDTO>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    private static HttpRequest CreateHttpRequest(string body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = "POST";
        request.ContentType = "application/json";
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        request.Body = new MemoryStream(bodyBytes);
        return request;
    }
}
