using FluentValidation;

namespace TraceLens.Application.Features.Batches.CreateBatch;

public sealed class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchCommandValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required.")
            .MaximumLength(50);

        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.ProducedQuantity)
            .GreaterThan(0).WithMessage("Produced quantity must be greater than zero.");

        RuleFor(x => x.ProducedQuantityUnit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20);
    }
}
