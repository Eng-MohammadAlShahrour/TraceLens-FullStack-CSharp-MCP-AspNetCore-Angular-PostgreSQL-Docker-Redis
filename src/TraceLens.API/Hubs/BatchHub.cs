using Microsoft.AspNetCore.SignalR;

namespace TraceLens.API.Hubs;

/// <summary>
/// SignalR hub for real-time batch events.
/// Clients connect to /hubs/batches and listen for:
///  - BatchCreated { id, batchNumber, status }
///  - BatchStatusChanged { id, batchNumber, status }
/// </summary>
public sealed class BatchHub : Hub
{
    // No client-invokable methods — server-to-client broadcasting only.
}
