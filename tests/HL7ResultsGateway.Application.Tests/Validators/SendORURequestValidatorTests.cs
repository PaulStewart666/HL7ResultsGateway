using FluentAssertions;

using FluentValidation.TestHelper;

using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.Validators;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.Tests.Validators;

public class SendORURequestValidatorTests
{
    private readonly SendORURequestValidator _validator;

    public SendORURequestValidatorTests()
    {
        _validator = new SendORURequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyDestinationEndpoint_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DestinationEndpoint = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DestinationEndpoint)
            .WithErrorMessage("Destination endpoint is required");
    }

    [Fact]
    public void Validate_WithInvalidUrl_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DestinationEndpoint = "invalid-url";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DestinationEndpoint)
            .WithErrorMessage("Destination endpoint must be a valid URL");
    }

    [Theory]
    [InlineData("http://api.example.com/hl7")]
    [InlineData("https://api.example.com/hl7")]
    public void Validate_WithValidUrl_ShouldPass(string url)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DestinationEndpoint = url;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DestinationEndpoint);
    }

    [Fact]
    public void Validate_WithNullMessageData_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData = null!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MessageData)
            .WithErrorMessage("Message data is required");
    }

    [Fact]
    public void Validate_WithNullPatient_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Patient = null!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Patient")
            .WithErrorMessage("Patient data is required");
    }

    [Fact]
    public void Validate_WithEmptyPatientId_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Patient.PatientId = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Patient.PatientId")
            .WithErrorMessage("Patient ID is required");
    }

    [Fact]
    public void Validate_WithEmptyPatientFirstName_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Patient.FirstName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Patient.FirstName")
            .WithErrorMessage("Patient first name is required");
    }

    [Fact]
    public void Validate_WithEmptyPatientLastName_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Patient.LastName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Patient.LastName")
            .WithErrorMessage("Patient last name is required");
    }

    [Fact]
    public void Validate_WithNullObservations_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations = null!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations")
            .WithErrorMessage("Observations are required");
    }

    [Fact]
    public void Validate_WithEmptyObservations_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations = new List<Observation>();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations")
            .WithErrorMessage("At least one observation is required");
    }

    [Fact]
    public void Validate_WithTooManyObservations_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations = Enumerable.Range(1, 101)
            .Select(i => new Observation { ObservationId = $"OBS{i:000}", Description = "Test", Value = "Normal" })
            .ToList();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations")
            .WithErrorMessage("Cannot exceed 100 observations per message");
    }

    [Fact]
    public void Validate_WithEmptyObservationId_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations[0].ObservationId = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations[0].ObservationId")
            .WithErrorMessage("Observation ID is required");
    }

    [Fact]
    public void Validate_WithEmptyObservationDescription_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations[0].Description = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations[0].Description")
            .WithErrorMessage("Observation description is required");
    }

    [Fact]
    public void Validate_WithEmptyObservationValue_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations[0].Value = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations[0].Value")
            .WithErrorMessage("Observation value is required");
    }

    [Theory]
    [InlineData("NM")]
    [InlineData("ST")]
    [InlineData("TX")]
    [InlineData("DT")]
    [InlineData("TM")]
    [InlineData("TS")]
    public void Validate_WithValidValueType_ShouldPass(string valueType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations[0].ValueType = valueType;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor("MessageData.Observations[0].ValueType");
    }

    [Fact]
    public void Validate_WithInvalidValueType_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.Observations[0].ValueType = "INVALID";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.Observations[0].ValueType")
            .WithErrorMessage("Observation value type must be one of: NM, ST, TX, DT, TM, TS");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(301)]
    public void Validate_WithInvalidTimeout_ShouldFail(int timeoutSeconds)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TimeoutSeconds = timeoutSeconds;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(30)]
    [InlineData(300)]
    public void Validate_WithValidTimeout_ShouldPass(int timeoutSeconds)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TimeoutSeconds = timeoutSeconds;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TimeoutSeconds);
    }

    [Fact]
    public void Validate_WithWrongMessageType_ShouldFail()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MessageData.MessageType = HL7MessageType.ADT_A01;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("MessageData.MessageType")
            .WithErrorMessage("Message type must be ORU_R01 for ORU message transmission");
    }

    [Theory]
    [InlineData(TransmissionProtocol.HTTP)]
    [InlineData(TransmissionProtocol.HTTPS)]
    [InlineData(TransmissionProtocol.MLLP)]
    [InlineData(TransmissionProtocol.SFTP)]
    public void Validate_WithValidProtocol_ShouldPass(TransmissionProtocol protocol)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Protocol = protocol;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Protocol);
    }

    private static SendORURequestDTO CreateValidRequest()
    {
        return new SendORURequestDTO
        {
            DestinationEndpoint = "https://api.example.com/hl7",
            Protocol = TransmissionProtocol.HTTPS,
            MessageData = new HL7Result
            {
                MessageType = HL7MessageType.ORU_R01,
                Patient = new Patient
                {
                    PatientId = "12345",
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1980, 1, 1)
                },
                Observations = new List<Observation>
                {
                    new()
                    {
                        ObservationId = "OBS001",
                        Description = "Test Result",
                        Value = "Normal",
                        Status = ObservationStatus.Normal,
                        ValueType = "ST"
                    }
                }
            },
            TimeoutSeconds = 30,
            Source = "TestSystem",
            Priority = TransmissionPriority.Normal
        };
    }
}
