using FluentValidation.Results;

namespace Test._4Create.Domain.Models.Validation;

public interface IClinicalTrialMetadataValidator
{
    ValidationResult Validate(ClinicalTrialMetadataInputModel clinicalTrialMetadata);
}