using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HL7ResultsGateway.Domain.Models;
using Xunit;

namespace HL7ResultsGateway.Domain.Tests.Models;

public class JsonHL7InputTests
{
    [Fact]
    public void JsonHL7Input_WhenValidData_ShouldPassValidation()
    {
        // Arrange
        var input = new JsonHL7Input
        {
            Patient = new JsonPatientData
            {
                PatientId = "P12345",
                FirstName = "John",
                LastName = "Doe",
                Gender = "M",
                DateOfBirth = "1990-01-15"
            },
            Observations = new List<JsonObservationData>
            {
                new()
                {
                    ObservationId = "OBS001",
                    Description = "Glucose",
                    Value = "95",
                    Units = "mg/dL",
                    Status = "N"
                }
            },
            MessageInfo = new JsonMessageInfo
            {
                SendingFacility = "LAB001",
                ReceivingFacility = "CLINIC001",
                MessageControlId = "MSG12345"
            }
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void JsonPatientData_WhenRequiredFieldsMissing_ShouldFailValidation()
    {
        // Arrange
        var patient = new JsonPatientData
        {
            // Missing required PatientId, FirstName, LastName
            Gender = "M"
        };

        // Act
        var validationResults = ValidateModel(patient);

        // Assert
        validationResults.Should().HaveCount(3);
        validationResults.Should().Contain(r => r.MemberNames.Contains("PatientId"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("FirstName"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("LastName"));
    }

    [Fact]
    public void JsonObservationData_WhenRequiredFieldsMissing_ShouldFailValidation()
    {
        // Arrange
        var observation = new JsonObservationData
        {
            // Missing required ObservationId, Description, Value
            Units = "mg/dL"
        };

        // Act
        var validationResults = ValidateModel(observation);

        // Assert
        validationResults.Should().HaveCount(3);
        validationResults.Should().Contain(r => r.MemberNames.Contains("ObservationId"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("Description"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("Value"));
    }

    [Fact]
    public void JsonHL7Input_WhenPatientNull_ShouldFailValidation()
    {
        // Arrange
        var input = new JsonHL7Input
        {
            Patient = null!,
            Observations = new List<JsonObservationData>(),
            MessageInfo = new JsonMessageInfo()
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults.Should().Contain(r => r.MemberNames.Contains("Patient"));
    }

    [Theory]
    [InlineData("M", true)]
    [InlineData("F", true)]
    [InlineData("O", true)]
    [InlineData("U", true)]
    [InlineData("", true)] // Optional field can be empty
    [InlineData(null, true)] // Optional field can be null
    public void JsonPatientData_Gender_ShouldAllowValidValues(string? gender, bool expectedValid)
    {
        // Arrange
        var patient = new JsonPatientData
        {
            PatientId = "P123",
            FirstName = "John",
            LastName = "Doe",
            Gender = gender
        };

        // Act
        var validationResults = ValidateModel(patient);

        // Assert
        if (expectedValid)
        {
            validationResults.Should().BeEmpty();
        }
        else
        {
            validationResults.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData("N", true)]
    [InlineData("A", true)]
    [InlineData("C", true)]
    [InlineData("P", true)]
    [InlineData("", true)] // Optional field can be empty
    [InlineData(null, true)] // Optional field can be null
    public void JsonObservationData_Status_ShouldAllowValidValues(string? status, bool expectedValid)
    {
        // Arrange
        var observation = new JsonObservationData
        {
            ObservationId = "OBS001",
            Description = "Test",
            Value = "123",
            Status = status
        };

        // Act
        var validationResults = ValidateModel(observation);

        // Assert
        if (expectedValid)
        {
            validationResults.Should().BeEmpty();
        }
        else
        {
            validationResults.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void JsonHL7Input_DefaultConstructor_ShouldInitializeCollections()
    {
        // Act
        var input = new JsonHL7Input();

        // Assert
        input.Patient.Should().NotBeNull();
        input.Observations.Should().NotBeNull().And.BeEmpty();
        input.MessageInfo.Should().NotBeNull();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}