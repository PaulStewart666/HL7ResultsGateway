namespace HL7ResultsGateway.Client.Features.JsonToHL7.Models;

/// <summary>
/// Predefined templates for JSON to HL7 conversion testing
/// </summary>
public class JsonTestTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonToHL7Request Template { get; set; } = new();

    /// <summary>
    /// Collection of predefined templates for common scenarios
    /// </summary>
    public static readonly JsonTestTemplate[] PredefinedTemplates = new[]
    {
        new JsonTestTemplate
        {
            Name = "Basic Lab Results",
            Description = "Simple glucose and cholesterol test results",
            Template = new JsonToHL7Request
            {
                Patient = new JsonPatientData
                {
                    PatientId = "P12345",
                    FirstName = "John",
                    LastName = "Doe",
                    MiddleName = "Michael",
                    DateOfBirth = new DateOnly(1985, 6, 15),
                    Gender = "M",
                    Address = "123 Main St, Anytown, AN 12345",
                    PhoneNumber = "(555) 123-4567"
                },
                MessageInfo = new JsonMessageInfo
                {
                    SendingApplication = "LAB_SYSTEM",
                    SendingFacility = "Central Lab",
                    ReceivingApplication = "EMR",
                    ReceivingFacility = "General Hospital",
                    MessageControlId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now
                },
                Observations = new List<JsonObservationData>
                {
                    new()
                    {
                        ObservationId = "GLU",
                        Description = "Glucose",
                        Value = "95",
                        Units = "mg/dL",
                        ReferenceRange = "70-99",
                        Status = "N",
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "CHOL",
                        Description = "Total Cholesterol",
                        Value = "185",
                        Units = "mg/dL",
                        ReferenceRange = "< 200",
                        Status = "N",
                        ValueType = "NM"
                    }
                }
            }
        },

        new JsonTestTemplate
        {
            Name = "Complete Blood Count",
            Description = "Comprehensive CBC panel with differential",
            Template = new JsonToHL7Request
            {
                Patient = new JsonPatientData
                {
                    PatientId = "P67890",
                    FirstName = "Jane",
                    LastName = "Smith",
                    MiddleName = "Elizabeth",
                    DateOfBirth = new DateOnly(1978, 12, 3),
                    Gender = "F",
                    Address = "456 Oak Ave, Springfield, SP 54321",
                    PhoneNumber = "(555) 987-6543"
                },
                MessageInfo = new JsonMessageInfo
                {
                    SendingApplication = "HEMATOLOGY_LAB",
                    SendingFacility = "University Hospital Lab",
                    ReceivingApplication = "EPIC",
                    ReceivingFacility = "University Hospital",
                    MessageControlId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now
                },
                Observations = new List<JsonObservationData>
                {
                    new()
                    {
                        ObservationId = "WBC",
                        Description = "White Blood Cell Count",
                        Value = "7.2",
                        Units = "K/uL",
                        ReferenceRange = "4.0-11.0",
                        Status = "N",
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "RBC",
                        Description = "Red Blood Cell Count",
                        Value = "4.5",
                        Units = "M/uL",
                        ReferenceRange = "4.2-5.4",
                        Status = "N",
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "HGB",
                        Description = "Hemoglobin",
                        Value = "13.8",
                        Units = "g/dL",
                        ReferenceRange = "12.0-15.5",
                        Status = "N",
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "HCT",
                        Description = "Hematocrit",
                        Value = "41.2",
                        Units = "%",
                        ReferenceRange = "36-46",
                        Status = "N",
                        ValueType = "NM"
                    }
                }
            }
        },

        new JsonTestTemplate
        {
            Name = "Critical Values",
            Description = "Laboratory results with critical/abnormal values",
            Template = new JsonToHL7Request
            {
                Patient = new JsonPatientData
                {
                    PatientId = "P11111",
                    FirstName = "Robert",
                    LastName = "Johnson",
                    DateOfBirth = new DateOnly(1965, 4, 22),
                    Gender = "M",
                    Address = "789 Pine Rd, Metro City, MC 98765",
                    PhoneNumber = "(555) 246-8135"
                },
                MessageInfo = new JsonMessageInfo
                {
                    SendingApplication = "STAT_LAB",
                    SendingFacility = "Emergency Lab",
                    ReceivingApplication = "MEDITECH",
                    ReceivingFacility = "Metro General Hospital",
                    MessageControlId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now
                },
                Observations = new List<JsonObservationData>
                {
                    new()
                    {
                        ObservationId = "K",
                        Description = "Potassium",
                        Value = "6.8",
                        Units = "mEq/L",
                        ReferenceRange = "3.5-5.0",
                        Status = "C", // Critical
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "TROP",
                        Description = "Troponin I",
                        Value = "15.2",
                        Units = "ng/mL",
                        ReferenceRange = "< 0.04",
                        Status = "C", // Critical
                        ValueType = "NM"
                    },
                    new()
                    {
                        ObservationId = "CK",
                        Description = "Creatine Kinase",
                        Value = "1250",
                        Units = "U/L",
                        ReferenceRange = "30-200",
                        Status = "A", // Abnormal
                        ValueType = "NM"
                    }
                }
            }
        },

        new JsonTestTemplate
        {
            Name = "Microbiology Culture",
            Description = "Microbiology culture results with text values",
            Template = new JsonToHL7Request
            {
                Patient = new JsonPatientData
                {
                    PatientId = "P22222",
                    FirstName = "Maria",
                    LastName = "Garcia",
                    DateOfBirth = new DateOnly(1992, 8, 10),
                    Gender = "F",
                    Address = "321 Elm St, Riverside, RS 13579",
                    PhoneNumber = "(555) 369-2580"
                },
                MessageInfo = new JsonMessageInfo
                {
                    SendingApplication = "MICRO_LAB",
                    SendingFacility = "Microbiology Laboratory",
                    ReceivingApplication = "CERNER",
                    ReceivingFacility = "Regional Medical Center",
                    MessageControlId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now
                },
                Observations = new List<JsonObservationData>
                {
                    new()
                    {
                        ObservationId = "CULT_URINE",
                        Description = "Urine Culture",
                        Value = "Escherichia coli >100,000 CFU/mL",
                        Units = "",
                        ReferenceRange = "< 10,000 CFU/mL",
                        Status = "A", // Abnormal
                        ValueType = "ST" // String Text
                    },
                    new()
                    {
                        ObservationId = "SENS_ECOLI",
                        Description = "E. coli Sensitivities",
                        Value = "Ampicillin: R, Ciprofloxacin: S, Nitrofurantoin: S, Trimethoprim-Sulfamethoxazole: R",
                        Units = "",
                        ReferenceRange = "",
                        Status = "N",
                        ValueType = "ST" // String Text
                    }
                }
            }
        }
    };
}
