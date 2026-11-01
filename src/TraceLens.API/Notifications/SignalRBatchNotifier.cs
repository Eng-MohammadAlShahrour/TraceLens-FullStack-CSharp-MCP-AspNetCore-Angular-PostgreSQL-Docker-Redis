using Microsoft.AspNetCore.SignalR;
using TraceLens.API.Hubs;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.API.Notifications;

public sealed class SignalRBatchNotifier : IBatchNotifier
{
    private readonly IHubContext<BatchHub> _hub;

    public SignalRBatchNotifier(IHubContext<BatchHub> hub) => _hub = hub;

    public Task NotifyBatchCreatedAsync(
        Guid batchId, string batchNumber, string status, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync(
            "BatchCreated",
            new { id = batchId, batchNumber, status },
            ct);

    public Task NotifyBatchStatusChangedAsync(
        Guid batchId, string batchNumber, string newStatus, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync(
            "BatchStatusChanged",
            new { id = batchId, batchNumber, status = newStatus },
            ct);
}
