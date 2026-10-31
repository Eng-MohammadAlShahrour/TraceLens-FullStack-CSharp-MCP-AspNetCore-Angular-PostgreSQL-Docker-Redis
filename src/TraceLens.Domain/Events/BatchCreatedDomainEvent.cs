using TraceLens.Domain.Common;

namespace TraceLens.Domain.Events;

public sealed record BatchCreatedDomainEvent(Guid BatchId, string BatchNumber) : IDomainEvent;
