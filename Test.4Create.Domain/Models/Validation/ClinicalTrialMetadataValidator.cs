using FluentValidation;

namespace Test._4Create.Domain.Models.Validation;

public class ClinicalTrialMetadataValidator : AbstractValidator<ClinicalTrialMetadata>
{
    public ClinicalTrialMetadataValidator()
    {
        RuleFor(x => x.TrialId)
            .NotEmpty()
            .WithMessage("Trial ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.")
            .Must(BeAValidDate)
            .WithMessage("Start date must be a valid date.");

        RuleFor(x => x.EndDate)
            .Must(BeAValidDate)
            .WithMessage("End date must be a valid date.")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Participants)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Participants must be at least 1.")
            .When(x => x.Participants.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of the valid values: NotStarted, Ongoing, or Completed.");

        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate >= x.StartDate)
            .WithMessage("End date must be on or after the start date.")
            .When(x => x.EndDate.HasValue);
    }

    private static bool BeAValidDate(DateTime? date)
    {
        return date != default;
    }

    private static bool BeAValidDate(DateTime date)
    {
        return date != default;
    }
}