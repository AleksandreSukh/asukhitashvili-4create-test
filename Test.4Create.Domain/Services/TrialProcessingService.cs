using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Test._4Create.Data;
using Test._4Create.Domain.Infrastructure;
using Test._4Create.Domain.Models;

namespace Test._4Create.Domain.Services;

public class TrialProcessingService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<TrialProcessingService> _logger;
    public TrialProcessingService(UnitOfWork unitOfWork, ILogger<TrialProcessingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CommandExecutionResult> SaveTrialMetadata(ClinicalTrialMetadata clinicalTrialMetadata)
    {
        //TODO:Move data validation here and add unit tests
        var endDateOrDefault = clinicalTrialMetadata.EndDate == null && clinicalTrialMetadata.Status == TrialStatus.Ongoing
            ? clinicalTrialMetadata.StartDate.AddMonths(1)
            : clinicalTrialMetadata.EndDate;

        var durationDays = endDateOrDefault != null
            ? (int)(endDateOrDefault.Value - clinicalTrialMetadata.StartDate).TotalDays
            : (int?)null;

        var trialMetadata = new Data.Entities.ClinicalTrialMetadata
        {
            Title = clinicalTrialMetadata.Title,
            Status = clinicalTrialMetadata.Status.ToString(),
            TrialId = clinicalTrialMetadata.TrialId,
            Participants = clinicalTrialMetadata.Participants,
            StartDate = clinicalTrialMetadata.StartDate,
            EndDate = endDateOrDefault,
            DurationDays = durationDays
        };

        _unitOfWork.ClinicalTrialMetadataRepository.Insert(trialMetadata);
        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error occured while trying to save {nameof(Data.Entities.ClinicalTrialMetadata)} in database. Error:{e}");
            return CommandExecutionResult.WithError(ErrorCodes.TrialMetadataProcessing.TrialMetadataPersistenceError, e.Message);
        }

        return CommandExecutionResult.Ok();
    }

    public QueryExecutionResult<ClinicalTrialMetadata> GetTrialMetadataById(string id)
    {
        var clinicalTrialMetadata = _unitOfWork.ClinicalTrialMetadataRepository.Get(i => i.TrialId == id);
        if (!clinicalTrialMetadata.Any())
        {
            return QueryExecutionResult<ClinicalTrialMetadata>.WithError(
                ErrorCodes.TrialMetadataProcessing.TrialMetadataWasNotFound, $"Medatada with id:{id} wasn't found");
        }

        var foundItem = clinicalTrialMetadata.First();

        var result = new ClinicalTrialMetadata()
        {
            Title = foundItem.Title,
            TrialId = foundItem.TrialId,
            StartDate = foundItem.StartDate,
            EndDate = foundItem.EndDate,
            Status = Enum.Parse<TrialStatus>(foundItem.Status),
            Participants = foundItem.Participants
        };

        return QueryExecutionResult<ClinicalTrialMetadata>.Ok(result);
    }

    public QueryExecutionResult<List<ClinicalTrialMetadata>> SearchTrialMetadatas(ClinicalTrialMetadataSearchParams clinicalTrialMetadataSearchData)
    {
        List<Data.Entities.ClinicalTrialMetadata>? result = null;
        if (!string.IsNullOrEmpty(clinicalTrialMetadataSearchData.Status))
        {
            if (Enum.TryParse<TrialStatus>(clinicalTrialMetadataSearchData.Status, out TrialStatus trialStatus))
            {
                var trialStatusString = trialStatus.ToString();
                result = _unitOfWork.ClinicalTrialMetadataRepository.Get(i => i.Status == trialStatusString).ToList();
            }
            else
            {
                return QueryExecutionResult<List<ClinicalTrialMetadata>>.WithError(
                    ErrorCodes.TrialMetadataProcessing.ParseStatusError,
                    $"Invalid value for status:{clinicalTrialMetadataSearchData.Status}, Valid options are:{string.Join("; ", Enum.GetValues<TrialStatus>().Select(e => e.ToString()))}");
            }
        }
        else
        {
            result = _unitOfWork.ClinicalTrialMetadataRepository.Get().ToList();
        }
        
        //TODO: extract mappers 
        var converted = result.Select(e => new ClinicalTrialMetadata()
        {
            TrialId = e.TrialId,
            Title = e.Title,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Status = Enum.Parse<TrialStatus>(e.Status),
            Participants = e.Participants
        });

        return QueryExecutionResult<List<ClinicalTrialMetadata>>.Ok(converted.ToList());
    }
}