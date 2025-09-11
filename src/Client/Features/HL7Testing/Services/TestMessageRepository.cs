using HL7ResultsGateway.Client.Features.HL7Testing.Models;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Services;

public interface ITestMessageRepository
{
    IEnumerable<HL7TestMessage> GetAllTestMessages();
    IEnumerable<HL7TestMessage> GetTestMessagesByCategory(string category);
    HL7TestMessage? GetTestMessageByName(string name);
}

/// <summary>
/// Repository for predefined HL7 test messages based on process-hl7-message.http examples
/// </summary>
public class TestMessageRepository : ITestMessageRepository
{
    private static readonly List<HL7TestMessage> _testMessages = new()
    {
        new HL7TestMessage
        {
            Name = "Valid ORU^R01 Lab Results",
            Description = "Standard lab results message with glucose observation",
            MessageType = "ORU^R01",
            Category = "Lab Results",
            IsValid = true,
            ExpectedSource = "GHH LAB",
            Content = "MSH|^~\\&|GHH LAB|ELAB-3|GHH OE|BLDG4|200202150930||ORU^R01|CNTRL-3456|P|2.4\r" +
                     "PID|||555-44-4444||EVERYWOMAN^EVE^E^^^^L|JONES|196203520|F|||153 FERNWOOD DR^^STATESVILLE^OH^35292||(206)3345232|(206)752-121||||AC555444444||67-A4335^OH^20030520\r" +
                     "OBR|1|845439^GHH OE|1045813^GHH LAB|1554-5^GLUCOSE^POST 12H CFST:MCNC:PT:SER/PLAS:QN||200202150730|||||||||DOBBS^MARK^A||||||||200202150930||F|||^^^^^R\r" +
                     "OBX|1|NM|1554-5^GLUCOSE^POST 12H CFST:MCNC:PT:SER/PLAS:QN|182|mg/dl|70_105|H|||F"
        },

        new HL7TestMessage
        {
            Name = "ADT^A01 Patient Admission",
            Description = "Patient admission message with demographic information",
            MessageType = "ADT^A01",
            Category = "Patient Administration",
            IsValid = true,
            ExpectedSource = "SENDING_APPLICATION",
            Content = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|20110613083617||ADT^A01|123456789|P|2.5\r" +
                     "EVN|A01|201106130836|||JSMITH\r" +
                     "PID|1||123456789^^^MR^MR||DOE^JOHN^MIDDLE||19800101|M||W|123 MAIN ST^^ANYTOWN^NY^12345^USA||(555)555-1234|(555)555-5678|EN|M|NONE|12345678|123456789|||||||||||||||||\r" +
                     "NK1|1|DOE^JANE|SPO|123 MAIN ST^^ANYTOWN^NY^12345^USA|(555)555-1234\r" +
                     "PV1|1|I|2000^2012^01||||123456^SMITH^JOHN|||SUR|||A|||123456^SMITH^JOHN|INP|CAT|2|||||||||||||||||||||201106130836"
        },

        new HL7TestMessage
        {
            Name = "Lab Results with Multiple Observations",
            Description = "Complete Blood Count (CBC) with multiple test results",
            MessageType = "ORU^R01",
            Category = "Lab Results",
            IsValid = true,
            ExpectedSource = "LAB_FACILITY",
            Content = "MSH|^~\\&|LAB|LAB_FACILITY|EMR|EMR_FACILITY|20110613083617||ORU^R01|MSG123456|P|2.5\r" +
                     "PID|1||MRN12345^^^MR^MR||PATIENT^TEST^M||19750315|M||W|456 OAK ST^^TESTCITY^CA^90210^USA||(555)123-4567|(555)765-4321|EN|M|NONE|SSN123456789|DL987654321\r" +
                     "OBR|1|ORD123456^EMR|LAB123456^LAB|CBC^COMPLETE BLOOD COUNT||20110613083000|||||||20110613083000|BLOOD^Blood|DOC123^DOCTOR^ORDERING|||||||20110613084500||F|||^^^^^R\r" +
                     "OBX|1|NM|WBC^WHITE BLOOD COUNT|7.5|10*3/uL|4.5-11.0|N|||F|||20110613084500\r" +
                     "OBX|2|NM|RBC^RED BLOOD COUNT|4.2|10*6/uL|4.2-5.4|N|||F|||20110613084500\r" +
                     "OBX|3|NM|HGB^HEMOGLOBIN|13.5|g/dL|12.0-15.5|N|||F|||20110613084500\r" +
                     "OBX|4|NM|HCT^HEMATOCRIT|40.5|%|37.0-48.0|N|||F|||20110613084500"
        },

        new HL7TestMessage
        {
            Name = "Minimal Valid Message",
            Description = "Minimal ACK message for basic connectivity testing",
            MessageType = "ACK^A01",
            Category = "System",
            IsValid = true,
            ExpectedSource = "TEST",
            Content = "MSH|^~\\&|TEST|TEST|TEST|TEST|20250831120000||ACK^A01|123|P|2.4"
        },

        new HL7TestMessage
        {
            Name = "Empty Message",
            Description = "Empty message body to test error handling",
            MessageType = "Invalid",
            Category = "Error Testing",
            IsValid = false,
            ExpectedSource = "Test",
            Content = ""
        },

        new HL7TestMessage
        {
            Name = "Invalid HL7 Format",
            Description = "Non-HL7 text to test parsing error handling",
            MessageType = "Invalid",
            Category = "Error Testing",
            IsValid = false,
            ExpectedSource = "Test",
            Content = "This is not a valid HL7 message format and should cause a parsing error"
        }
    };

    public IEnumerable<HL7TestMessage> GetAllTestMessages()
    {
        return _testMessages;
    }

    public IEnumerable<HL7TestMessage> GetTestMessagesByCategory(string category)
    {
        return _testMessages.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public HL7TestMessage? GetTestMessageByName(string name)
    {
        return _testMessages.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
