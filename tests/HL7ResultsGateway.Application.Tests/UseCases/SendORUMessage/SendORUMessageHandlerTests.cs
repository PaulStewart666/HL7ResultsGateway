using FluentAssertions;

using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Exceptions;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

using Moq;

namespace HL7ResultsGateway.Application.Tests.UseCases.SendORUMessage;

public class SendORUMessageHandlerTests
{
    private readonly Mock<IHL7TransmissionProviderFactory> _mockProviderFactory;
    private readonly Mock<IHL7TransmissionRepository> _mockTransmissionRepository;
    private readonly Mock<IJsonHL7Converter> _mockHL7Converter;
    private readonly Mock<ILogger<SendORUMessageHandler>> _mockLogger;
    private readonly Mock<IHL7TransmissionProvider> _mockTransmissionProvider;
    private readonly SendORUMessageHandler _handler;

    public SendORUMessageHandlerTests()
    {
        _mockProviderFactory = new Mock<IHL7TransmissionProviderFactory>();
        _mockTransmissionRepository = new Mock<IHL7TransmissionRepository>();
        _mockHL7Converter = new Mock<IJsonHL7Converter>();
        _mockLogger = new Mock<ILogger<SendORUMessageHandler>>();
        _mockTransmissionProvider = new Mock<IHL7TransmissionProvider>();

        _handler = new SendORUMessageHandler(
            _mockProviderFactory.Object,
            _mockTransmissionRepository.Object,
            _mockHL7Converter.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void Constructor_WithNullProviderFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessageHandler(
            null!,
            _mockTransmissionRepository.Object,
            _mockHL7Converter.Object,
            _mockLogger.Object
        );

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("providerFactory");
    }

    [Fact]
    public void Constructor_WithNullTransmissionRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessageHandler(
            _mockProviderFactory.Object,
            null!,
            _mockHL7Converter.Object,
            _mockLogger.Object
        );

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("transmissionRepository");
    }

    [Fact]
    public void Constructor_WithNullHL7Converter_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessageHandler(
            _mockProviderFactory.Object,
            _mockTransmissionRepository.Object,
            null!,
            _mockLogger.Object
        );

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("hl7Converter");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SendORUMessageHandler(
            _mockProviderFactory.Object,
            _mockTransmissionRepository.Object,
            _mockHL7Converter.Object,
            null!
        );

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _handler.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        var act = () => _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        var hl7Message = "MSH|^~\\&|SYS|FAC|REC|FAC|20240917120000||ORU^R01^ORU_R01|MSG001|P|2.5.1\r\nPID|1||12345||Doe^John||19800101|M\r\nOBX|1|ST|TEST^Test Result||Normal||||||F";
        var transmissionResult = TransmissionResult.CreateSuccess(
            "TRANS001",
            "ACK received",
            TimeSpan.FromSeconds(1.5)
        );

        _mockHL7Converter.Setup(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()))
            .ReturnsAsync(hl7Message);

        _mockProviderFactory.Setup(x => x.CreateProvider(It.IsAny<TransmissionProtocol>()))
            .Returns(_mockTransmissionProvider.Object);

        _mockTransmissionProvider.Setup(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        _mockTransmissionRepository.Setup(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HL7TransmissionLog log, CancellationToken ct) => log);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransmissionId.Should().NotBeEmpty();
        result.ErrorMessage.Should().BeNull();
        result.ResponseTime.Should().BePositive();

        _mockHL7Converter.Verify(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()), Times.Once);
        _mockProviderFactory.Verify(x => x.CreateProvider(TransmissionProtocol.HTTP), Times.Once);
        _mockTransmissionProvider.Verify(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockTransmissionRepository.Verify(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithHL7ConversionFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedException = new InvalidOperationException("HL7 conversion failed");

        _mockHL7Converter.Setup(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HL7 conversion failed");
        result.TransmissionId.Should().BeEmpty();

        _mockProviderFactory.Verify(x => x.CreateProvider(It.IsAny<TransmissionProtocol>()), Times.Never);
        _mockTransmissionProvider.Verify(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTransmissionProviderFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var hl7Message = "MSH|^~\\&|SYS|FAC|REC|FAC|20240917120000||ORU^R01^ORU_R01|MSG001|P|2.5.1\r\nPID|1||12345||Doe^John||19800101|M";
        var transmissionResult = TransmissionResult.CreateFailure(
            "TRANS002",
            "Connection timeout",
            TimeSpan.FromSeconds(30)
        );

        _mockHL7Converter.Setup(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()))
            .ReturnsAsync(hl7Message);

        _mockProviderFactory.Setup(x => x.CreateProvider(It.IsAny<TransmissionProtocol>()))
            .Returns(_mockTransmissionProvider.Object);

        _mockTransmissionProvider.Setup(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        _mockTransmissionRepository.Setup(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HL7TransmissionLog log, CancellationToken ct) => log);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Connection timeout");
        result.TransmissionId.Should().BeEmpty();

        _mockTransmissionRepository.Verify(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTransmissionException_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var hl7Message = "MSH|^~\\&|SYS|FAC|REC|FAC|20240917120000||ORU^R01^ORU_R01|MSG001|P|2.5.1\r\nPID|1||12345||Doe^John||19800101|M";
        var expectedException = new TransmissionException("Network error", TransmissionProtocol.HTTP, "https://api.example.com", "TRANS003");

        _mockHL7Converter.Setup(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()))
            .ReturnsAsync(hl7Message);

        _mockProviderFactory.Setup(x => x.CreateProvider(It.IsAny<TransmissionProtocol>()))
            .Returns(_mockTransmissionProvider.Object);

        _mockTransmissionProvider.Setup(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Network error");
        result.TransmissionId.Should().BeEmpty();

        _mockTransmissionRepository.Verify(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(TransmissionProtocol.HTTP)]
    [InlineData(TransmissionProtocol.HTTPS)]
    [InlineData(TransmissionProtocol.MLLP)]
    [InlineData(TransmissionProtocol.SFTP)]
    public async Task Handle_WithDifferentProtocols_ShouldUseCorrectProvider(TransmissionProtocol protocol)
    {
        // Arrange
        var command = CreateValidCommandWithProtocol(protocol);
        var hl7Message = "MSH|^~\\&|SYS|FAC|REC|FAC|20240917120000||ORU^R01^ORU_R01|MSG001|P|2.5.1\r\nPID|1||12345||Doe^John||19800101|M";
        var transmissionResult = TransmissionResult.CreateSuccess(
            "TRANS001",
            "ACK received",
            TimeSpan.FromSeconds(1.5)
        );

        _mockHL7Converter.Setup(x => x.ConvertToHL7StringAsync(It.IsAny<JsonHL7Input>()))
            .ReturnsAsync(hl7Message);

        _mockProviderFactory.Setup(x => x.CreateProvider(protocol))
            .Returns(_mockTransmissionProvider.Object);

        _mockTransmissionProvider.Setup(x => x.SendMessageAsync(It.IsAny<HL7TransmissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transmissionResult);

        _mockTransmissionRepository.Setup(x => x.SaveTransmissionLogAsync(It.IsAny<HL7TransmissionLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HL7TransmissionLog log, CancellationToken ct) => log);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _mockProviderFactory.Verify(x => x.CreateProvider(protocol), Times.Once);
    }

    private static SendORUMessageCommand CreateValidCommand()
    {
        return CreateValidCommandWithProtocol(TransmissionProtocol.HTTP);
    }

    private static SendORUMessageCommand CreateValidCommandWithProtocol(TransmissionProtocol protocol)
    {
        var hl7Result = new HL7Result
        {
            MessageType = HL7MessageType.ORU_R01,
            Patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = Gender.Male,
                PatientId = "12345"
            },
            Observations = new List<Observation>
            {
                new()
                {
                    ObservationId = "OBS001",
                    Description = "Test Result",
                    Value = "Normal",
                    Units = "",
                    Status = ObservationStatus.Normal,
                    ValueType = "ST"
                }
            }
        };

        return new SendORUMessageCommand(
            DestinationEndpoint: "https://api.example.com/hl7",
            MessageData: hl7Result,
            Source: "TestSystem",
            Protocol: protocol,
            Headers: new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" }
            },
            TimeoutSeconds: 30
        );
    }
}
