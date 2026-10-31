using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Exceptions;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Domain.Entities;

namespace TraceLens.Application.Features.WorkOrders.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler
    : IRequestHandler<CreateWorkOrderCommand, CreateWorkOrderResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<CreateWorkOrderResponse> Handle(CreateWorkOrderCommand request, CancellationToken ct)
    {
        var normalized = request.OrderNumber.Trim().ToUpperInvariant();

        var exists = await _context.WorkOrders
            .AnyAsync(w => w.OrderNumber == normalized, ct);

        if (exists)
            throw new DuplicateWorkOrderException(normalized);

        var workOrder = WorkOrder.Create(
            request.OrderNumber,
            request.ProductId,
            request.PlannedQuantity,
            request.DueDate);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(ct);

        return new CreateWorkOrderResponse(
            workOrder.Id,
            workOrder.OrderNumber,
            workOrder.PlannedQuantity,
            workOrder.DueDate,
            workOrder.IsClosed,
            workOrder.CreatedAt);
    }
}
