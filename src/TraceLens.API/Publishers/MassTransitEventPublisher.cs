using MassTransit;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.API.Publishers;

/// <summary>
/// Publishes integration events to RabbitMQ via MassTransit.
/// Uses IPublishEndpoint (scoped, handler-friendly) — not IBus (singleton).
/// Failures are logged and swallowed (best-effort): a failed publish must not
/// roll back the already-committed DB transaction. The batch is persisted and
/// SignalR has already been notified — RabbitMQ is treated as a downstream
/// concern, consistent with Redis cache usage.
/// </summary>
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;

    public MassTransitEventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            await _publishEndpoint.Publish(integrationEvent, cancellationToken);
            _logger.LogInformation(
                "Integration event published: {EventType}",
                typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to publish integration event {EventType}. Batch state remains committed.",
                typeof(T).Name);
        }
    }
}
