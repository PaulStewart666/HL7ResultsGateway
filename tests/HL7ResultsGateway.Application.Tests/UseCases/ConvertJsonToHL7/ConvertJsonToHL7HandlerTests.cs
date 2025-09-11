using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HL7ResultsGateway.Application.Tests.UseCases.ConvertJsonToHL7;

/// <summary>
/// Comprehensive tests for ConvertJsonToHL7Handler
/// Validates orchestration of JSON to HL7 conversion using domain services
/// </summary>
public class ConvertJsonToHL7HandlerTests
{
    private readonly Mock<IJsonHL7Converter> _mockConverter;
    private readonly ConvertJsonToHL7Handler _handler;

    public ConvertJsonToHL7HandlerTests()
    {
        _mockConverter = new Mock<IJsonHL7Converter>();
        _handler = new ConvertJsonToHL7Handler(_mockConverter.Object);
    }

    [Fact]
    public void Constructor_WithNullConverter_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ConvertJsonToHL7Handler(null!);
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("converter");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldReturnFailureResult()
    {
        // Act
        var result = await _handler.Handle(null!, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ConvertedMessage.Should().BeNull();
        result.HL7MessageString.Should().BeNull();
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().Contain("Command or JsonInput is null");
        result.ErrorMessage.Should().Be("Invalid input: Command or JsonInput cannot be null");
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockConverter.Verify(x => x.ValidateInputAsync(It.IsAny<JsonHL7Input>()), Times.Never);
        _mockConverter.Verify(x => x.ConvertToHL7Async(It.IsAny<JsonHL7Input>()), Times.Never);
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullJsonInput_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new ConvertJsonToHL7Command(null!, "TestSource");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ConvertedMessage.Should().BeNull();
        result.HL7MessageString.Should().BeNull();
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().Contain("Command or JsonInput is null");
        result.ErrorMessage.Should().Be("Invalid input: Command or JsonInput cannot be null");
    }

    [Fact]
    public async Task Handle_WithInvalidInput_ShouldReturnValidationFailure()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        var command = new ConvertJsonToHL7Command(jsonInput, "TestSource");

        var validationResult = ValidationResult.Failure("Required field missing", "Invalid date format");
        _mockConverter.Setup(x => x.ValidateInputAsync(jsonInput))
                     .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ConvertedMessage.Should().BeNull();
        result.HL7MessageString.Should().BeNull();
        result.ValidationResult.Should().Be(validationResult);
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().HaveCount(2);
        result.ErrorMessage.Should().Be("Validation failed: Required field missing, Invalid date format");
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockConverter.Verify(x => x.ValidateInputAsync(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7Async(It.IsAny<JsonHL7Input>()), Times.Never);
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldReturnSuccessResult()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        var command = new ConvertJsonToHL7Command(jsonInput, "TestSource");

        var validationResult = ValidationResult.Success();
        var expectedHL7Result = CreateExpectedHL7Result();
        var expectedHL7String = "MSH|^~\\&|LAB|HOSPITAL|EMR|CLINIC|20240101120000||ORU^R01|MSG001|P|2.4\r\n" +
                                "PID|1||12345^^^HOSPITAL^MR||DOE^JANE^||19800101|F\r\n" +
                                "OBR|1||ORDER123|CBC^COMPLETE BLOOD COUNT|||20240101120000\r\n" +
                                "OBX|1|NM|WBC^White Blood Cells|1|7.5|10*3/uL|4.0-11.0|N|||F";

        _mockConverter.Setup(x => x.ValidateInputAsync(jsonInput))
                     .ReturnsAsync(validationResult);
        _mockConverter.Setup(x => x.ConvertToHL7Async(jsonInput))
                     .ReturnsAsync(expectedHL7Result);
        _mockConverter.Setup(x => x.ConvertToHL7StringAsync(jsonInput))
                     .ReturnsAsync(expectedHL7String);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ConvertedMessage.Should().Be(expectedHL7Result);
        result.HL7MessageString.Should().Be(expectedHL7String);
        result.ValidationResult.Should().Be(validationResult);
        result.ValidationResult.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockConverter.Verify(x => x.ValidateInputAsync(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7Async(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(jsonInput), Times.Once);
    }

    [Fact]
    public async Task Handle_WithConverterException_ShouldReturnFailureWithException()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        var command = new ConvertJsonToHL7Command(jsonInput, "TestSource");

        var validationResult = ValidationResult.Success();
        var exceptionMessage = "Conversion failed due to invalid patient data";
        var exception = new InvalidOperationException(exceptionMessage);

        _mockConverter.Setup(x => x.ValidateInputAsync(jsonInput))
                     .ReturnsAsync(validationResult);
        _mockConverter.Setup(x => x.ConvertToHL7Async(jsonInput))
                     .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ConvertedMessage.Should().BeNull();
        result.HL7MessageString.Should().BeNull();
        result.ValidationResult.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().Contain($"Conversion error: {exceptionMessage}");
        result.ErrorMessage.Should().Be(exceptionMessage);
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockConverter.Verify(x => x.ValidateInputAsync(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7Async(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToConverter()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        var command = new ConvertJsonToHL7Command(jsonInput, "TestSource");
        var cancellationToken = new CancellationToken();

        var validationResult = ValidationResult.Success();
        _mockConverter.Setup(x => x.ValidateInputAsync(jsonInput))
                     .ReturnsAsync(validationResult);
        _mockConverter.Setup(x => x.ConvertToHL7Async(jsonInput))
                     .ReturnsAsync(CreateExpectedHL7Result());
        _mockConverter.Setup(x => x.ConvertToHL7StringAsync(jsonInput))
                     .ReturnsAsync("test-hl7-string");

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockConverter.Verify(x => x.ValidateInputAsync(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7Async(jsonInput), Times.Once);
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(jsonInput), Times.Once);
    }

    private static JsonHL7Input CreateValidJsonInput()
    {
        return new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "12345",
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = "1980-01-01",
                Gender = "F"
            },
            MessageInfo = new JsonMessageInfo
            {
                MessageControlId = "MSG001",
                Timestamp = "2024-01-01T12:00:00Z",
                SendingFacility = "HOSPITAL"
            },
            Observations = new List<JsonObservationData>
            {
                new JsonObservationData
                {
                    ObservationId = "WBC",
                    Description = "White Blood Cells",
                    Value = "7.5",
                    Units = "10*3/uL",
                    ReferenceRange = "4.0-11.0",
                    Status = "N",
                    ValueType = "NM"
                }
            }
        };
    }

    private static HL7Result CreateExpectedHL7Result()
    {
        return new HL7Result
        {
            MessageType = HL7MessageType.ORU_R01,
            Patient = new Patient
            {
                PatientId = "12345",
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1980-01-01"),
                Gender = Gender.Female
            },
            Observations = new List<Observation>
            {
                new Observation
                {
                    ObservationId = "WBC",
                    Description = "White Blood Cells",
                    Value = "7.5",
                    Units = "10*3/uL",
                    ReferenceRange = "4.0-11.0",
                    Status = ObservationStatus.Normal,
                    ValueType = "NM"
                }
            }
        };
    }
}