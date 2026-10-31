using TraceLens.Domain.Common;

namespace TraceLens.Domain.Events;

public sealed record QualityCheckPassedDomainEvent(Guid BatchId, string BatchNumber) : IDomainEvent;
