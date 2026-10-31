using MediatR;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.StartProduction;

public sealed record StartProductionCommand(Guid Id) : IRequest<BatchDto>;
