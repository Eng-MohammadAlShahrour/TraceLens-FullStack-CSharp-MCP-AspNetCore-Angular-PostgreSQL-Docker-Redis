using FluentValidation;

namespace TraceLens.Application.Features.Batches.QuarantineBatch;

public sealed class QuarantineBatchCommandValidator : AbstractValidator<QuarantineBatchCommand>
{
    public QuarantineBatchCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Quarantine reason is required.")
            .MaximumLength(1000);
    }
}
