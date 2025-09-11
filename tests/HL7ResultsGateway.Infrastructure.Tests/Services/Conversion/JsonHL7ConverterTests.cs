using FluentAssertions;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;
using HL7ResultsGateway.Infrastructure.Logging;
using HL7ResultsGateway.Infrastructure.Services.Conversion;
using Moq;
using Xunit;

namespace HL7ResultsGateway.Infrastructure.Tests.Services.Conversion;

public class JsonHL7ConverterTests
{
    private readonly Mock<ILoggingService> _mockLogger;
    private readonly JsonHL7Converter _converter;

    public JsonHL7ConverterTests()
    {
        _mockLogger = new Mock<ILoggingService>();
        _converter = new JsonHL7Converter(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new JsonHL7Converter(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task ConvertToHL7Async_WithValidInput_ShouldReturnHL7Result()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();

        // Act
        var result = await _converter.ConvertToHL7Async(jsonInput);

        // Assert
        result.Should().NotBeNull();
        result.MessageType.Should().Be(HL7MessageType.ORU_R01);
        result.Patient.Should().NotBeNull();
        result.Patient.PatientId.Should().Be("12345");
        result.Patient.FirstName.Should().Be("John");
        result.Patient.LastName.Should().Be("Doe");
        result.Patient.Gender.Should().Be(Gender.Male);
        result.Observations.Should().HaveCount(2);
        
        var firstObs = result.Observations.First();
        firstObs.ObservationId.Should().Be("WBC");
        firstObs.Description.Should().Be("White Blood Cell Count");
        firstObs.Value.Should().Be("8.5");
        firstObs.Status.Should().Be(ObservationStatus.Normal);
    }

    [Fact]
    public async Task ConvertToHL7Async_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = async () => await _converter.ConvertToHL7Async(null!);
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ConvertToHL7Async_WithInvalidInput_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidInput = new JsonHL7Input
        {
            Patient = new JsonPatientData { PatientId = "", FirstName = "", LastName = "" },
            Observations = new List<JsonObservationData>(),
            MessageInfo = new JsonMessageInfo()
        };

        // Act & Assert
        var action = async () => await _converter.ConvertToHL7Async(invalidInput);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("JSON input validation failed:*");
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithValidInput_ShouldReturnHL7String()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();

        // Act
        var result = await _converter.ConvertToHL7StringAsync(jsonInput);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("MSH|^~\\&|");
        result.Should().Contain("ORU^R01^ORU_R01");
        result.Should().Contain("PID|1||12345");
        result.Should().Contain("DOE^JOHN");
        result.Should().Contain("OBR|1|");
        result.Should().Contain("OBX|1|NM|WBC^White Blood Cell Count^L||8.5|K/uL|4.0-11.0|N|||F");
        result.Should().Contain("OBX|2|NM|HGB^Hemoglobin^L||14.2|g/dL|12.0-16.0|N|||F");
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = async () => await _converter.ConvertToHL7StringAsync(null!);
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateInputAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var validInput = CreateValidJsonInput();

        // Act
        var result = await _converter.ValidateInputAsync(validInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidPatient_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = new JsonHL7Input
        {
            Patient = new JsonPatientData { PatientId = "", FirstName = "", LastName = "" },
            Observations = new List<JsonObservationData>
            {
                new() { ObservationId = "WBC", Description = "Test", Value = "8.5" }
            },
            MessageInfo = new JsonMessageInfo()
        };

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Patient ID is required");
        result.Errors.Should().Contain("First name is required");
        result.Errors.Should().Contain("Last name is required");
    }

    [Fact]
    public async Task ValidateInputAsync_WithNoObservations_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = new JsonHL7Input
        {
            Patient = new JsonPatientData { PatientId = "123", FirstName = "John", LastName = "Doe" },
            Observations = new List<JsonObservationData>(),
            MessageInfo = new JsonMessageInfo()
        };

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("At least one observation is required");
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidGender_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = CreateValidJsonInput();
        invalidInput.Patient.Gender = "X"; // Invalid gender

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Gender must be M, F, O, or U");
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidDateOfBirth_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = CreateValidJsonInput();
        invalidInput.Patient.DateOfBirth = "invalid-date"; // Invalid date format

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Date of birth must be in yyyy-MM-dd format");
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidObservationStatus_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = CreateValidJsonInput();
        invalidInput.Observations[0].Status = "X"; // Invalid status

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Observation 1: Status must be N, A, H, L, C, or P");
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidValueType_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = CreateValidJsonInput();
        invalidInput.Observations[0].ValueType = "XX"; // Invalid value type

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Observation 1: Value type must be NM, ST, TX, DT, TM, or TS");
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidTimestamp_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = CreateValidJsonInput();
        invalidInput.MessageInfo.Timestamp = "invalid-timestamp";

        // Act
        var result = await _converter.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Timestamp must be a valid date/time format");
    }

    [Theory]
    [InlineData("M", Gender.Male)]
    [InlineData("F", Gender.Female)]
    [InlineData("O", Gender.Other)]
    [InlineData("U", Gender.Unknown)]
    [InlineData("", Gender.Unknown)]
    public async Task ConvertToHL7Async_WithDifferentGenders_ShouldMapCorrectly(string inputGender, Gender expectedGender)
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        jsonInput.Patient.Gender = inputGender;

        // Act
        var result = await _converter.ConvertToHL7Async(jsonInput);

        // Assert
        result.Patient.Gender.Should().Be(expectedGender);
    }

    [Theory]
    [InlineData("N", ObservationStatus.Normal)]
    [InlineData("A", ObservationStatus.Abnormal)]
    [InlineData("H", ObservationStatus.Abnormal)]
    [InlineData("L", ObservationStatus.Abnormal)]
    [InlineData("C", ObservationStatus.Critical)]
    [InlineData("P", ObservationStatus.Pending)]
    [InlineData("", ObservationStatus.Unknown)]
    public async Task ConvertToHL7Async_WithDifferentStatuses_ShouldMapCorrectly(string inputStatus, ObservationStatus expectedStatus)
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        jsonInput.Observations[0].Status = inputStatus;

        // Act
        var result = await _converter.ConvertToHL7Async(jsonInput);

        // Assert
        result.Observations[0].Status.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task ConvertToHL7Async_WithMiddleName_ShouldIncludeMiddleName()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        jsonInput.Patient.MiddleName = "William";

        // Act
        var result = await _converter.ConvertToHL7Async(jsonInput);

        // Assert
        result.Patient.MiddleName.Should().Be("William");
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithMiddleName_ShouldIncludeInPIDSegment()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        jsonInput.Patient.MiddleName = "William";

        // Act
        var result = await _converter.ConvertToHL7StringAsync(jsonInput);

        // Assert
        result.Should().Contain("DOE^JOHN^WILLIAM");
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithCustomMessageInfo_ShouldUseCustomValues()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();
        jsonInput.MessageInfo.SendingFacility = "TESTLAB";
        jsonInput.MessageInfo.ReceivingFacility = "HOSPITAL";
        jsonInput.MessageInfo.MessageControlId = "MSG123";
        jsonInput.MessageInfo.Timestamp = "2023-12-01T10:30:00Z";

        // Act
        var result = await _converter.ConvertToHL7StringAsync(jsonInput);

        // Assert
        result.Should().Contain("MSH|^~\\&|TESTLAB|HOSPITAL");
        result.Should().Contain("|MSG123|");
    }

    [Fact]
    public void Constructor_CallsLoggerCorrectly()
    {
        // Arrange & Act
        var converter = new JsonHL7Converter(_mockLogger.Object);

        // Assert
        converter.Should().NotBeNull();
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConvertToHL7Async_LogsCorrectly()
    {
        // Arrange
        var jsonInput = CreateValidJsonInput();

        // Act
        await _converter.ConvertToHL7Async(jsonInput);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Starting JSON to HL7 conversion")),
                It.IsAny<object>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("JSON to HL7 conversion completed successfully")),
                It.IsAny<object>()),
            Times.Once);
    }

    private static JsonHL7Input CreateValidJsonInput()
    {
        return new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "12345",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = "1990-01-15",
                Gender = "M",
                Address = "123 Main St, Anytown, NY 12345"
            },
            Observations = new List<JsonObservationData>
            {
                new()
                {
                    ObservationId = "WBC",
                    Description = "White Blood Cell Count",
                    Value = "8.5",
                    Units = "K/uL",
                    ReferenceRange = "4.0-11.0",
                    Status = "N",
                    ValueType = "NM"
                },
                new()
                {
                    ObservationId = "HGB",
                    Description = "Hemoglobin",
                    Value = "14.2",
                    Units = "g/dL",
                    ReferenceRange = "12.0-16.0",
                    Status = "N",
                    ValueType = "NM"
                }
            },
            MessageInfo = new JsonMessageInfo
            {
                SendingFacility = "LAB",
                ReceivingFacility = "HOSPITAL",
                MessageControlId = "CTL001",
                Timestamp = "2023-12-01T10:30:00Z"
            }
        };
    }
}