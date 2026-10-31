using MediatR;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.ShipBatch;

public sealed record ShipBatchCommand(Guid Id) : IRequest<BatchDto>;
