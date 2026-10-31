using MediatR;

namespace TraceLens.Application.Features.Batches.GetBatchesList;

public sealed record GetBatchesListQuery(int Page = 1, int PageSize = 20)
    : IRequest<GetBatchesListResponse>;
