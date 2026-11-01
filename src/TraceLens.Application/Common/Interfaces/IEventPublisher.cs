namespace TraceLens.Application.Common.Interfaces;

/// <summary>
/// Abstraction for publishing integration events to an external message broker.
/// Implemented in TraceLens.API using MassTransit + RabbitMQ.
/// Handlers depend on this interface (not on IBus or MassTransit directly) to keep the
/// Application layer free of infrastructure concerns.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class;
}
