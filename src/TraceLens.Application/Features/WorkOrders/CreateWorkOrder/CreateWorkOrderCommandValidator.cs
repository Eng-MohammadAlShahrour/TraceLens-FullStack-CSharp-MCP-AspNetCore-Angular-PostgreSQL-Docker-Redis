using FluentValidation;

namespace TraceLens.Application.Features.WorkOrders.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required.")
            .MaximumLength(50);

        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.PlannedQuantity).GreaterThan(0);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Due date must be in the future.");
    }
}
