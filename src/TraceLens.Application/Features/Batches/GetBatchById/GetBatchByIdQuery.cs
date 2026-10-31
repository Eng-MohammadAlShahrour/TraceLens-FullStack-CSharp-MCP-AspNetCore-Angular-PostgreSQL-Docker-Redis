using MediatR;

namespace TraceLens.Application.Features.Batches.GetBatchById;

public sealed record GetBatchByIdQuery(Guid Id) : IRequest<BatchDto?>;
