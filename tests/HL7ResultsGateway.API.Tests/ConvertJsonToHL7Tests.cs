using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using HL7ResultsGateway.API;
using HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;
using HL7ResultsGateway.Domain.Services.Conversion;

namespace HL7ResultsGateway.API.Tests;

public class ConvertJsonToHL7Tests
{
    private readonly Mock<IConvertJsonToHL7Handler> _mockHandler;
    private readonly Mock<ILogger<ConvertJsonToHL7>> _mockLogger;
    private readonly ConvertJsonToHL7 _function;

    public ConvertJsonToHL7Tests()
    {
        _mockHandler = new Mock<IConvertJsonToHL7Handler>();
        _mockLogger = new Mock<ILogger<ConvertJsonToHL7>>();
        _function = new ConvertJsonToHL7(_mockLogger.Object, _mockHandler.Object);
    }

    [Fact]
    public async Task Run_ValidJsonInput_ReturnsSuccessResult()
    {
        // Arrange
        var inputJson = new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "12345",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = "1985-06-15",
                Gender = "M"
            },
            Observations = new List<JsonObservationData>
            {
                new JsonObservationData
                {
                    ObservationId = "OBS001",
                    Description = "Blood Glucose",
                    Value = "95",
                    Units = "mg/dL",
                    ReferenceRange = "70-100",
                    Status = "Normal"
                }
            },
            MessageInfo = new JsonMessageInfo
            {
                MessageControlId = "MSG001",
                Timestamp = "2024-11-09T12:00:00",
                SendingFacility = "Lab System"
            }
        };

        var hl7Message = "MSH|^~\\&|Lab System||HIS||20241109120000||ORU^R01|MSG001|P|2.5\r\nPID|1||12345||Doe^John||19850615|M\r\nOBX|1|TX|OBS001||95|mg/dL|70-100|Normal|||F";

        var mockResult = new ConvertJsonToHL7Result(
            Success: true,
            ConvertedMessage: new HL7Result
            {
                MessageType = HL7MessageType.ORU_R01,
                Patient = new Patient
                {
                    PatientId = "12345",
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = DateTime.Parse("1985-06-15"),
                    Gender = Gender.Male
                },
                Observations = new List<Observation>
                {
                    new Observation
                    {
                        ObservationId = "OBS001",
                        Description = "Blood Glucose",
                        Value = "95",
                        Units = "mg/dL",
                        ReferenceRange = "70-100",
                        Status = ObservationStatus.Normal
                    }
                }
            },
            HL7MessageString: hl7Message,
            ValidationResult: new ValidationResult { IsValid = true, Errors = new List<string>() },
            ErrorMessage: null,
            ProcessedAt: DateTime.UtcNow
        );

        _mockHandler.Setup(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(mockResult);

        var requestBody = JsonSerializer.Serialize(inputJson);
        var httpRequest = CreateMockHttpRequest(requestBody);

        // Act
        var result = await _function.Run(httpRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value;
        response.Should().NotBeNull();

        _mockHandler.Verify(x => x.Handle(It.Is<ConvertJsonToHL7Command>(
            c => c.JsonInput.Patient.PatientId == "12345" &&
                 c.JsonInput.Patient.FirstName == "John" &&
                 c.JsonInput.Patient.LastName == "Doe"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var httpRequest = CreateMockHttpRequest(invalidJson);

        // Act
        var result = await _function.Run(httpRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value;
        response.Should().NotBeNull();

        _mockHandler.Verify(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_HandlerReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var inputJson = new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "12345",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = "1985-06-15",
                Gender = "M"
            },
            Observations = new List<JsonObservationData>
            {
                new JsonObservationData
                {
                    ObservationId = "OBS001",
                    Description = "Test",
                    Value = "Normal"
                }
            }
        };

        var mockResult = new ConvertJsonToHL7Result(
            Success: false,
            ConvertedMessage: null,
            HL7MessageString: null,
            ValidationResult: new ValidationResult { IsValid = false, Errors = new List<string> { "Validation error" } },
            ErrorMessage: "Conversion failed",
            ProcessedAt: DateTime.UtcNow
        );

        _mockHandler.Setup(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(mockResult);

        var requestBody = JsonSerializer.Serialize(inputJson);
        var httpRequest = CreateMockHttpRequest(requestBody);

        // Act
        var result = await _function.Run(httpRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value;
        response.Should().NotBeNull();

        _mockHandler.Verify(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_EmptyInput_ReturnsBadRequest()
    {
        // Arrange
        var emptyInput = "";
        var httpRequest = CreateMockHttpRequest(emptyInput);

        // Act
        var result = await _function.Run(httpRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mockHandler.Verify(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_NullInput_ReturnsBadRequest()
    {
        // Arrange
        var httpRequest = CreateMockHttpRequest("");

        // Act
        var result = await _function.Run(httpRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mockHandler.Verify(x => x.Handle(It.IsAny<ConvertJsonToHL7Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static HttpRequest CreateMockHttpRequest(string requestBody)
    {
        var mockRequest = new Mock<HttpRequest>();
        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        mockRequest.Setup(r => r.Body).Returns(bodyStream);
        mockRequest.Setup(r => r.Query).Returns(Mock.Of<IQueryCollection>());
        mockRequest.Setup(r => r.Headers).Returns(Mock.Of<IHeaderDictionary>());

        return mockRequest.Object;
    }
}
