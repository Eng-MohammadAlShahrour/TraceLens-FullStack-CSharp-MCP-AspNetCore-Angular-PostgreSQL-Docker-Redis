using MediatR;

namespace TraceLens.Application.Features.Activity.GetActivity;

public sealed record GetActivityQuery(int Limit = 100) : IRequest<GetActivityResponse>;

public sealed record GetActivityResponse(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount);

public sealed record AuditLogDto(
    Guid Id,
    string EventType,
    Guid AggregateId,
    string BatchNumber,
    string? PreviousStatus,
    string? NewStatus,
    string? Reason,
    DateTime CreatedAt);
