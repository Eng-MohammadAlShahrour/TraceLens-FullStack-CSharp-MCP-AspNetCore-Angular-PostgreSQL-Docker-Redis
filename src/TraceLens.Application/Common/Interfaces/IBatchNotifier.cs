namespace TraceLens.Application.Common.Interfaces;

public interface IBatchNotifier
{
    Task NotifyBatchCreatedAsync(
        Guid batchId,
        string batchNumber,
        string status,
        CancellationToken cancellationToken = default);

    Task NotifyBatchStatusChangedAsync(
        Guid batchId,
        string batchNumber,
        string newStatus,
        CancellationToken cancellationToken = default);
}
