namespace HL7ResultsGateway.Domain.ValueObjects;

/// <summary>
/// Defines the supported protocols for HL7 message transmission
/// </summary>
public enum TransmissionProtocol
{
    /// <summary>
    /// HTTP protocol (not recommended for production)
    /// </summary>
    HTTP,

    /// <summary>
    /// HTTPS protocol with SSL/TLS encryption
    /// </summary>
    HTTPS,

    /// <summary>
    /// Minimal Lower Layer Protocol - HL7 standard for TCP-based transmission
    /// </summary>
    MLLP,

    /// <summary>
    /// SSH File Transfer Protocol for secure file-based transmission
    /// </summary>
    SFTP
}
