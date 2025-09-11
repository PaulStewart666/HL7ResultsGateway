using FluentAssertions;

using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.ValueObjects;

using Moq;

using Xunit;

namespace HL7ResultsGateway.Domain.Tests.Services.Conversion;

public class JsonHL7ConverterTests
{
    private readonly Mock<IJsonHL7Converter> _mockConverter;
    private readonly JsonHL7Input _validJsonInput;

    public JsonHL7ConverterTests()
    {
        _mockConverter = new Mock<IJsonHL7Converter>();
        _validJsonInput = CreateValidJsonInput();
    }

    [Fact]
    public async Task ConvertToHL7Async_WithValidInput_ShouldReturnHL7Result()
    {
        // Arrange
        var expectedResult = CreateExpectedHL7Result();
        _mockConverter
            .Setup(x => x.ConvertToHL7Async(_validJsonInput))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockConverter.Object.ConvertToHL7Async(_validJsonInput);

        // Assert
        result.Should().NotBeNull();
        result.MessageType.Should().Be(HL7MessageType.ORU_R01);
        result.Patient.Should().NotBeNull();
        result.Patient.PatientId.Should().Be("P12345");
        result.Patient.FirstName.Should().Be("John");
        result.Patient.LastName.Should().Be("Doe");
        result.Observations.Should().HaveCount(1);
        result.Observations[0].ObservationId.Should().Be("OBS001");
        result.Observations[0].Description.Should().Be("Glucose");
        result.Observations[0].Value.Should().Be("95");
        _mockConverter.Verify(x => x.ConvertToHL7Async(_validJsonInput), Times.Once);
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithValidInput_ShouldReturnHL7String()
    {
        // Arrange
        var expectedHL7String = "MSH|^~\\&|LAB001|CLINIC001||||||ORU^R01^ORU_R01|MSG12345|P|2.5\r" +
                                "PID|1||P12345^^^HOSPITAL||DOE^JOHN^||19900115|M|||123 MAIN ST^^CITY^ST^12345|\r" +
                                "OBR|1||ORDER123|85427^GLUCOSE^LN|||20241109120000|||||||||||||||F\r" +
                                "OBX|1|NM|85427^GLUCOSE^LN||95|mg/dL|70-105|N|||F";

        _mockConverter
            .Setup(x => x.ConvertToHL7StringAsync(_validJsonInput))
            .ReturnsAsync(expectedHL7String);

        // Act
        var result = await _mockConverter.Object.ConvertToHL7StringAsync(_validJsonInput);

        // Assert
        result.Should().NotBeNull();
        result.Should().StartWith("MSH|");
        result.Should().Contain("P12345"); // Patient ID
        result.Should().Contain("DOE^JOHN"); // Patient name
        result.Should().Contain("GLUCOSE"); // Observation
        result.Should().Contain("95|mg/dL"); // Value and units
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(_validJsonInput), Times.Once);
    }

    [Fact]
    public async Task ValidateInputAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var validationResult = Domain.Services.Conversion.ValidationResult.Success();
        _mockConverter
            .Setup(x => x.ValidateInputAsync(_validJsonInput))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _mockConverter.Object.ValidateInputAsync(_validJsonInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        _mockConverter.Verify(x => x.ValidateInputAsync(_validJsonInput), Times.Once);
    }

    [Fact]
    public async Task ValidateInputAsync_WithInvalidInput_ShouldReturnFailure()
    {
        // Arrange
        var invalidInput = new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                // Missing required fields
                PatientId = "",
                FirstName = "",
                LastName = ""
            }
        };

        var expectedErrors = new[]
        {
            "Patient ID is required",
            "First name is required",
            "Last name is required"
        };

        var validationResult = Domain.Services.Conversion.ValidationResult.Failure(expectedErrors);
        _mockConverter
            .Setup(x => x.ValidateInputAsync(invalidInput))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _mockConverter.Object.ValidateInputAsync(invalidInput);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Patient ID is required");
        result.Errors.Should().Contain("First name is required");
        result.Errors.Should().Contain("Last name is required");
        _mockConverter.Verify(x => x.ValidateInputAsync(invalidInput), Times.Once);
    }

    [Fact]
    public async Task ConvertToHL7Async_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        _mockConverter
            .Setup(x => x.ConvertToHL7Async(null!))
            .ThrowsAsync(new ArgumentNullException("jsonInput"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _mockConverter.Object.ConvertToHL7Async(null!));

        exception.ParamName.Should().Be("jsonInput");
        _mockConverter.Verify(x => x.ConvertToHL7Async(null!), Times.Once);
    }

    [Fact]
    public async Task ConvertToHL7StringAsync_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        _mockConverter
            .Setup(x => x.ConvertToHL7StringAsync(null!))
            .ThrowsAsync(new ArgumentNullException("jsonInput"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _mockConverter.Object.ConvertToHL7StringAsync(null!));

        exception.ParamName.Should().Be("jsonInput");
        _mockConverter.Verify(x => x.ConvertToHL7StringAsync(null!), Times.Once);
    }

    [Fact]
    public async Task ConvertToHL7Async_WithInvalidData_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidInput = new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "INVALID",
                FirstName = "",
                LastName = ""
            }
        };

        _mockConverter
            .Setup(x => x.ConvertToHL7Async(invalidInput))
            .ThrowsAsync(new InvalidOperationException("Conversion failed due to invalid patient data"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockConverter.Object.ConvertToHL7Async(invalidInput));

        exception.Message.Should().Be("Conversion failed due to invalid patient data");
        _mockConverter.Verify(x => x.ConvertToHL7Async(invalidInput), Times.Once);
    }

    [Fact]
    public void ValidationResult_Success_ShouldCreateValidResult()
    {
        // Act
        var result = Domain.Services.Conversion.ValidationResult.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_Failure_WithStringArray_ShouldCreateInvalidResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = Domain.Services.Conversion.ValidationResult.Failure(errors);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void ValidationResult_Failure_WithEnumerable_ShouldCreateInvalidResult()
    {
        // Arrange
        var errors = new List<string> { "Error A", "Error B", "Error C" };

        // Act
        var result = Domain.Services.Conversion.ValidationResult.Failure(errors);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Error A");
        result.Errors.Should().Contain("Error B");
        result.Errors.Should().Contain("Error C");
    }

    private static JsonHL7Input CreateValidJsonInput()
    {
        return new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "P12345",
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "M",
                DateOfBirth = "1990-01-15",
                Gender = "M",
                Address = "123 Main St, City, ST 12345"
            },
            Observations = new List<JsonObservationData>
            {
                new()
                {
                    ObservationId = "OBS001",
                    Description = "Glucose",
                    Value = "95",
                    Units = "mg/dL",
                    ReferenceRange = "70-105",
                    Status = "N",
                    ValueType = "NM"
                }
            },
            MessageInfo = new JsonMessageInfo
            {
                SendingFacility = "LAB001",
                ReceivingFacility = "CLINIC001",
                MessageControlId = "MSG12345",
                Timestamp = "2024-11-09T12:00:00Z"
            }
        };
    }

    private static HL7Result CreateExpectedHL7Result()
    {
        var patient = new Patient
        {
            PatientId = "P12345",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "M",
            DateOfBirth = new DateTime(1990, 1, 15),
            Gender = Gender.Male,
            Address = "123 Main St, City, ST 12345"
        };

        var observation = new Observation
        {
            ObservationId = "OBS001",
            Description = "Glucose",
            Value = "95",
            Units = "mg/dL",
            ReferenceRange = "70-105",
            Status = ObservationStatus.Normal,
            ValueType = "NM"
        };

        return new HL7Result
        {
            MessageType = HL7MessageType.ORU_R01,
            Patient = patient,
            Observations = new List<Observation> { observation }
        };
    }
}
