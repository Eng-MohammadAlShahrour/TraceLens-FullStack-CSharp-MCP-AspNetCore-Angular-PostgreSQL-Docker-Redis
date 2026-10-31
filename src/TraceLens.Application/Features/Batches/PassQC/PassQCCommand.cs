using MediatR;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.PassQC;

public sealed record PassQCCommand(Guid Id) : IRequest<BatchDto>;
