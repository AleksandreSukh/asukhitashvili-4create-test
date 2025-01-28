using Test._4Create.Data;
using Test._4Create.Data.Entities;
using Test._4Create.Domain.Infrastructure;
using Test._4Create.Domain.Mappers;
using Test._4Create.Domain.Models;
using Test._4Create.Domain.Models.Validation;

namespace Test._4Create.Domain.Services;

public class TrialProcessingService
{
    private readonly IClinicalTrialMetadataValidator _clinicalTrialMetadataValidator;
    private readonly IUnitOfWork _unitOfWork;

    public TrialProcessingService(IUnitOfWork unitOfWork,
                                  IClinicalTrialMetadataValidator clinicalTrialMetadataValidator)
    {
        _unitOfWork = unitOfWork;
        _clinicalTrialMetadataValidator = clinicalTrialMetadataValidator;
    }

    public async Task<CommandExecutionResult> ProcessTrialMetadata(ClinicalTrialMetadataInputModel clinicalTrialMetadata)
    {
        var dataValidationResult = _clinicalTrialMetadataValidator.Validate(clinicalTrialMetadata);
        if (!dataValidationResult.IsValid)
        {
            var dataValidationErrors = dataValidationResult.Errors.Select(e => e.ErrorMessage);
            return CommandExecutionResult.WithError(ErrorCodes.TrialMetadataProcessing.TrialMetadataValidationError,
                                                    string.Join("; ", dataValidationErrors));
        }

        var endDateOrDefault = clinicalTrialMetadata.EndDate == null && clinicalTrialMetadata.Status == TrialStatus.Ongoing
            ? clinicalTrialMetadata.StartDate.AddMonths(1)
            : clinicalTrialMetadata.EndDate;

        var durationDays = endDateOrDefault != null
            ? (int) (endDateOrDefault.Value - clinicalTrialMetadata.StartDate).TotalDays
            : (int?) null;

        var trialMetadata = new ClinicalTrialMetadata
        {
            Title = clinicalTrialMetadata.Title,
            Status = clinicalTrialMetadata.Status.ToString(),
            TrialId = clinicalTrialMetadata.TrialId,
            Participants = clinicalTrialMetadata.Participants,
            StartDate = clinicalTrialMetadata.StartDate,
            EndDate = endDateOrDefault,
            DurationDays = durationDays
        };

        _unitOfWork.ClinicalTrialMetadataGenericRepository.Insert(trialMetadata);
        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (Exception e)
        {
            return CommandExecutionResult.WithError(ErrorCodes.TrialMetadataProcessing.TrialMetadataPersistenceError, e.Message);
        }

        return CommandExecutionResult.Ok();
    }

    public QueryExecutionResult<ClinicalTrialMetadataReadModel> GetTrialMetadataById(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        var clinicalTrialMetadata = _unitOfWork.ClinicalTrialMetadataGenericRepository.Get(i => i.TrialId == id).FirstOrDefault();
        if (clinicalTrialMetadata == null)
        {
            return QueryExecutionResult<ClinicalTrialMetadataReadModel>.WithError(
                ErrorCodes.TrialMetadataProcessing.TrialMetadataWasNotFound, $"Medatada with id:{id} wasn't found");
        }

        return QueryExecutionResult<ClinicalTrialMetadataReadModel>.Ok(clinicalTrialMetadata.ToClinicalTrialMetadataReadModel());
    }

    public QueryExecutionResult<List<ClinicalTrialMetadataReadModel>> SearchTrialMetadatas(ClinicalTrialMetadataSearchParams filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        List<ClinicalTrialMetadata>? result;
        if (!string.IsNullOrEmpty(filter.Status))
        {
            if (Enum.TryParse(filter.Status, out TrialStatus trialStatus))
            {
                var trialStatusString = trialStatus.ToString();
                result = _unitOfWork.ClinicalTrialMetadataGenericRepository.Get(i => i.Status == trialStatusString).ToList();
            }
            else
            {
                return QueryExecutionResult<List<ClinicalTrialMetadataReadModel>>.WithError(
                    ErrorCodes.TrialMetadataProcessing.ParseStatusError,
                    $"Invalid value for status:{filter.Status}, Valid options are:{string.Join("; ", Enum.GetValues<TrialStatus>().Select(e => e.ToString()))}");
            }
        }
        else
        {
            result = _unitOfWork.ClinicalTrialMetadataGenericRepository.Get().ToList();
        }

        return QueryExecutionResult<List<ClinicalTrialMetadataReadModel>>.Ok(result.Select(e => e.ToClinicalTrialMetadataReadModel()).ToList());
    }
}