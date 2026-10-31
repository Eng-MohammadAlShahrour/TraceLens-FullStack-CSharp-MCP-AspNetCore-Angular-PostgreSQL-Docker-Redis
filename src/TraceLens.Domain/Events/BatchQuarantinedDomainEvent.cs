using TraceLens.Domain.Common;

namespace TraceLens.Domain.Events;

public sealed record BatchQuarantinedDomainEvent(Guid BatchId, string BatchNumber, string Reason) : IDomainEvent;
