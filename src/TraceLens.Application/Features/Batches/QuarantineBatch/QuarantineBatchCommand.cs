using MediatR;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.QuarantineBatch;

public sealed record QuarantineBatchCommand(Guid Id, string Reason) : IRequest<BatchDto>;
