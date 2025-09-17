using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Services;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Net;

namespace HL7ResultsGateway.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB repository for HL7 transmission audit logging
/// </summary>
public sealed class CosmosHL7TransmissionRepository : IHL7TransmissionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosHL7TransmissionRepository> _logger;
    private const string ContainerName = "hl7-transmissions";
    private const string PartitionKeyPath = "/partitionKey";

    public CosmosHL7TransmissionRepository(
        CosmosClient cosmosClient,
        ILogger<CosmosHL7TransmissionRepository> logger,
        string databaseName = "HL7ResultsGateway")
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var database = cosmosClient.GetDatabase(databaseName);
        _container = database.GetContainer(ContainerName);
    }

    public async Task<HL7TransmissionLog> SaveTransmissionLogAsync(
        HL7TransmissionLog transmissionLog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transmissionLog);

        try
        {
            _logger.LogDebug(
                "Saving transmission log {TransmissionId} to Cosmos DB",
                transmissionLog.TransmissionId);

            // Create a document with partition key
            var document = CreateDocumentFromLog(transmissionLog);

            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully saved transmission log {TransmissionId} with request charge {RequestCharge}",
                transmissionLog.TransmissionId, response.RequestCharge);

            return transmissionLog;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning(
                "Transmission log {TransmissionId} already exists in Cosmos DB",
                transmissionLog.TransmissionId);

            throw new InvalidOperationException(
                $"Transmission log with ID {transmissionLog.TransmissionId} already exists", ex);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex,
                "Failed to save transmission log {TransmissionId} to Cosmos DB with status {StatusCode}",
                transmissionLog.TransmissionId, ex.StatusCode);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error saving transmission log {TransmissionId} to Cosmos DB",
                transmissionLog.TransmissionId);

            throw;
        }
    }

    public async Task<HL7TransmissionLog?> GetTransmissionLogAsync(
        string transmissionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transmissionId))
            throw new ArgumentException("Transmission ID cannot be null or empty", nameof(transmissionId));

        try
        {
            _logger.LogDebug(
                "Retrieving transmission log {TransmissionId} from Cosmos DB",
                transmissionId);

            // Use the transmission ID as both the document ID and partition key
            var partitionKey = new PartitionKey(GetPartitionKey(transmissionId));

            var response = await _container.ReadItemAsync<TransmissionLogDocument>(
                transmissionId,
                partitionKey,
                cancellationToken: cancellationToken);

            _logger.LogDebug(
                "Successfully retrieved transmission log {TransmissionId} with request charge {RequestCharge}",
                transmissionId, response.RequestCharge);

            return CreateLogFromDocument(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug(
                "Transmission log {TransmissionId} not found in Cosmos DB",
                transmissionId);

            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve transmission log {TransmissionId} from Cosmos DB with status {StatusCode}",
                transmissionId, ex.StatusCode);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error retrieving transmission log {TransmissionId} from Cosmos DB",
                transmissionId);

            throw;
        }
    }

    public async Task<IEnumerable<HL7TransmissionLog>> GetTransmissionHistoryAsync(
        string? patientId = null,
        DateTime? from = null,
        DateTime? to = null,
        TransmissionProtocol? protocol = null,
        bool? success = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Querying transmission logs - PatientId: {PatientId}, From: {From}, To: {To}, Protocol: {Protocol}, Success: {Success}, Limit: {Limit}",
                patientId, from, to, protocol, success, limit);

            var queryDefinition = BuildHistoryQuery(patientId, from, to, protocol, success, limit);

            var results = new List<HL7TransmissionLog>();
            using var feedIterator = _container.GetItemQueryIterator<TransmissionLogDocument>(queryDefinition);

            while (feedIterator.HasMoreResults && results.Count < limit)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);

                foreach (var document in response)
                {
                    if (results.Count >= limit)
                        break;

                    results.Add(CreateLogFromDocument(document));
                }

                _logger.LogDebug(
                    "Retrieved {Count} transmission logs with request charge {RequestCharge}",
                    response.Count, response.RequestCharge);
            }

            _logger.LogInformation(
                "Successfully retrieved {Count} transmission logs from Cosmos DB",
                results.Count);

            return results;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex,
                "Failed to query transmission logs from Cosmos DB with status {StatusCode}",
                ex.StatusCode);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error querying transmission logs from Cosmos DB");

            throw;
        }
    }

    public async Task<TransmissionStatistics> GetTransmissionStatisticsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Calculating transmission statistics from {From} to {To}",
                from, to);

            var query = @"
                SELECT
                    COUNT(1) as totalCount,
                    SUM(c.success ? 1 : 0) as successCount,
                    SUM(c.success ? 0 : 1) as failureCount,
                    AVG(c.responseTimeMs) as avgResponseTime,
                    c.protocol
                FROM c
                WHERE c.sentAt >= @from AND c.sentAt <= @to
                GROUP BY c.protocol";

            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@from", from)
                .WithParameter("@to", to);

            var protocolStats = new Dictionary<TransmissionProtocol, int>();
            var totalTransmissions = 0;
            var successfulTransmissions = 0;
            var totalResponseTimeMs = 0.0;
            var responseTimeCount = 0;

            using var feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);

                foreach (var item in response)
                {
                    var protocol = Enum.Parse<TransmissionProtocol>(item.protocol);
                    var count = (int)item.totalCount;
                    var successCount = (int)item.successCount;
                    var avgResponseTime = (double?)item.avgResponseTime ?? 0.0;

                    protocolStats[protocol] = count;
                    totalTransmissions += count;
                    successfulTransmissions += successCount;

                    if (avgResponseTime > 0)
                    {
                        totalResponseTimeMs += avgResponseTime * count;
                        responseTimeCount += count;
                    }
                }
            }

            var failedTransmissions = totalTransmissions - successfulTransmissions;
            var successRate = totalTransmissions > 0 ? (double)successfulTransmissions / totalTransmissions : 0.0;
            var averageResponseTime = responseTimeCount > 0
                ? TimeSpan.FromMilliseconds(totalResponseTimeMs / responseTimeCount)
                : TimeSpan.Zero;

            return new TransmissionStatistics(
                TotalTransmissions: totalTransmissions,
                SuccessfulTransmissions: successfulTransmissions,
                FailedTransmissions: failedTransmissions,
                SuccessRate: successRate,
                AverageResponseTime: averageResponseTime,
                PeriodStart: from,
                PeriodEnd: to,
                TransmissionsByProtocol: protocolStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to calculate transmission statistics from {From} to {To}",
                from, to);

            throw;
        }
    }

    public async Task<int> DeleteOldLogsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            _logger.LogInformation(
                "Deleting transmission logs older than {CutoffDate} (retention: {RetentionDays} days)",
                cutoffDate, retentionDays);

            var query = "SELECT c.id, c.partitionKey FROM c WHERE c.sentAt < @cutoffDate";
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@cutoffDate", cutoffDate);

            var deletedCount = 0;
            using var feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);

                foreach (var item in response)
                {
                    var id = (string)item.id;
                    var partitionKey = (string)item.partitionKey;

                    await _container.DeleteItemAsync<TransmissionLogDocument>(
                        id,
                        new PartitionKey(partitionKey),
                        cancellationToken: cancellationToken);

                    deletedCount++;
                }
            }

            _logger.LogInformation(
                "Successfully deleted {DeletedCount} old transmission logs",
                deletedCount);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete old transmission logs with retention {RetentionDays} days",
                retentionDays);

            throw;
        }
    }

    private static TransmissionLogDocument CreateDocumentFromLog(HL7TransmissionLog log)
    {
        return new TransmissionLogDocument
        {
            Id = log.TransmissionId,
            PartitionKey = GetPartitionKey(log.TransmissionId),
            TransmissionId = log.TransmissionId,
            Endpoint = log.Endpoint,
            Protocol = log.Protocol.ToString(),
            HL7MessageType = log.HL7MessageType,
            PatientId = log.PatientId,
            Success = log.Success,
            ErrorMessage = log.ErrorMessage,
            AcknowledgmentMessage = log.AcknowledgmentMessage,
            SentAt = log.SentAt,
            ResponseTimeMs = log.ResponseTime.TotalMilliseconds,
            Source = log.Source,
            HttpStatusCode = log.HttpStatusCode,
            Metadata = log.Metadata,
            CreatedAt = log.CreatedAt,
            TTL = (int)TimeSpan.FromDays(365).TotalSeconds // Auto-delete after 1 year
        };
    }

    private static HL7TransmissionLog CreateLogFromDocument(TransmissionLogDocument document)
    {
        if (!Enum.TryParse<TransmissionProtocol>(document.Protocol, out var protocol))
        {
            protocol = TransmissionProtocol.HTTP; // Default fallback
        }

        return new HL7TransmissionLog
        {
            TransmissionId = document.TransmissionId,
            Endpoint = document.Endpoint,
            Protocol = protocol,
            HL7MessageType = document.HL7MessageType,
            PatientId = document.PatientId,
            Success = document.Success,
            ErrorMessage = document.ErrorMessage,
            AcknowledgmentMessage = document.AcknowledgmentMessage,
            SentAt = document.SentAt,
            ResponseTime = TimeSpan.FromMilliseconds(document.ResponseTimeMs),
            Source = document.Source,
            HttpStatusCode = document.HttpStatusCode,
            Metadata = document.Metadata,
            CreatedAt = document.CreatedAt
        };
    }

    private static string GetPartitionKey(string transmissionId)
    {
        // Use first 8 characters of transmission ID for partitioning
        // This provides reasonable distribution while keeping related logs together
        return transmissionId.Length >= 8 ? transmissionId.Substring(0, 8) : transmissionId;
    }

    private static QueryDefinition BuildHistoryQuery(
        string? patientId,
        DateTime? from,
        DateTime? to,
        TransmissionProtocol? protocol,
        bool? success,
        int limit)
    {
        var query = "SELECT * FROM c WHERE 1=1";
        var parameters = new List<(string, object)>();

        if (!string.IsNullOrWhiteSpace(patientId))
        {
            query += " AND c.patientId = @patientId";
            parameters.Add(("@patientId", patientId));
        }

        if (from.HasValue)
        {
            query += " AND c.sentAt >= @from";
            parameters.Add(("@from", from.Value));
        }

        if (to.HasValue)
        {
            query += " AND c.sentAt <= @to";
            parameters.Add(("@to", to.Value));
        }

        if (protocol.HasValue)
        {
            query += " AND c.protocol = @protocol";
            parameters.Add(("@protocol", protocol.Value.ToString()));
        }

        if (success.HasValue)
        {
            query += " AND c.success = @success";
            parameters.Add(("@success", success.Value));
        }

        query += " ORDER BY c.sentAt DESC";

        var queryDefinition = new QueryDefinition(query);
        foreach (var (name, value) in parameters)
        {
            queryDefinition.WithParameter(name, value);
        }

        return queryDefinition;
    }

    // Internal document model for Cosmos DB
    private class TransmissionLogDocument
    {
        public string Id { get; set; } = string.Empty;
        public string PartitionKey { get; set; } = string.Empty;
        public string TransmissionId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string HL7MessageType { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AcknowledgmentMessage { get; set; }
        public DateTime SentAt { get; set; }
        public double ResponseTimeMs { get; set; }
        public string Source { get; set; } = string.Empty;
        public int? HttpStatusCode { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TTL { get; set; } // Time-to-live for auto-deletion
    }
}

/// <summary>
/// In-memory implementation of HL7 transmission repository for development and testing
/// </summary>
public sealed class InMemoryHL7TransmissionRepository : IHL7TransmissionRepository
{
    private readonly ConcurrentDictionary<string, HL7TransmissionLog> _logs = new();
    private readonly ILogger<InMemoryHL7TransmissionRepository> _logger;

    public InMemoryHL7TransmissionRepository(ILogger<InMemoryHL7TransmissionRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("Using in-memory HL7 transmission repository for development");
    }

    public Task<HL7TransmissionLog> SaveTransmissionLogAsync(
        HL7TransmissionLog transmissionLog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transmissionLog);

        // Ensure transmission has an ID
        if (string.IsNullOrWhiteSpace(transmissionLog.TransmissionId))
        {
            transmissionLog.TransmissionId = Guid.NewGuid().ToString();
        }

        _logs.AddOrUpdate(transmissionLog.TransmissionId, transmissionLog, (_, _) => transmissionLog);

        _logger.LogDebug("Saved transmission log {TransmissionId} to in-memory store", transmissionLog.TransmissionId);
        return Task.FromResult(transmissionLog);
    }

    public Task<HL7TransmissionLog?> GetTransmissionLogAsync(
        string transmissionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transmissionId))
            return Task.FromResult<HL7TransmissionLog?>(null);

        _logs.TryGetValue(transmissionId, out var log);
        return Task.FromResult(log);
    }

    public Task<IEnumerable<HL7TransmissionLog>> GetTransmissionHistoryAsync(
        string? patientId = null,
        DateTime? from = null,
        DateTime? to = null,
        TransmissionProtocol? protocol = null,
        bool? success = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _logs.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(patientId))
            query = query.Where(log => log.PatientId == patientId);

        if (from.HasValue)
            query = query.Where(log => log.SentAt >= from.Value);

        if (to.HasValue)
            query = query.Where(log => log.SentAt <= to.Value);

        if (protocol.HasValue)
            query = query.Where(log => log.Protocol == protocol.Value);

        if (success.HasValue)
            query = query.Where(log => log.Success == success.Value);

        var results = query
            .OrderByDescending(log => log.SentAt)
            .Take(limit)
            .ToList();

        _logger.LogDebug("Retrieved {Count} transmission logs from in-memory store", results.Count);
        return Task.FromResult<IEnumerable<HL7TransmissionLog>>(results);
    }

    public Task<TransmissionStatistics> GetTransmissionStatisticsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var logs = _logs.Values
            .Where(log => log.SentAt >= from && log.SentAt <= to)
            .ToList();

        var totalTransmissions = logs.Count;
        var successfulTransmissions = logs.Count(log => log.Success);
        var failedTransmissions = totalTransmissions - successfulTransmissions;
        var successRate = totalTransmissions > 0 ? (double)successfulTransmissions / totalTransmissions : 0.0;
        var averageResponseTime = logs.Any()
            ? TimeSpan.FromMilliseconds(logs.Average(log => log.ResponseTime.TotalMilliseconds))
            : TimeSpan.Zero;

        var transmissionsByProtocol = logs
            .GroupBy(log => log.Protocol)
            .ToDictionary(g => g.Key, g => g.Count());

        var statistics = new TransmissionStatistics(
            totalTransmissions,
            successfulTransmissions,
            failedTransmissions,
            successRate,
            averageResponseTime,
            from,
            to,
            transmissionsByProtocol);

        _logger.LogDebug("Calculated transmission statistics for {From} to {To}: {Total} total, {Success} successful",
            from, to, totalTransmissions, successfulTransmissions);

        return Task.FromResult(statistics);
    }

    public Task<int> DeleteOldLogsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var logsToDelete = _logs.Values
            .Where(log => log.SentAt < cutoffDate)
            .ToList();

        foreach (var log in logsToDelete)
        {
            _logs.TryRemove(log.TransmissionId, out _);
        }

        _logger.LogDebug("Deleted {Count} old transmission logs from in-memory store", logsToDelete.Count);
        return Task.FromResult(logsToDelete.Count);
    }
}
