namespace TraceLens.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public string BatchNumber { get; private set; } = string.Empty;
    public string? PreviousStatus { get; private set; }
    public string? NewStatus { get; private set; }
    public string? Reason { get; private set; }
    public string Payload { get; private set; } = "{}";
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        string eventType,
        Guid aggregateId,
        string aggregateType,
        string batchNumber,
        string payload,
        string? previousStatus = null,
        string? newStatus = null,
        string? reason = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            BatchNumber = batchNumber,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Reason = reason,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
