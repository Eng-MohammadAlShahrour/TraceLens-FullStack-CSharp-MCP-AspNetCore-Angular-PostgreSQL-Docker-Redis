using MediatR;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.FailQC;

public sealed record FailQCCommand(Guid Id) : IRequest<BatchDto>;
