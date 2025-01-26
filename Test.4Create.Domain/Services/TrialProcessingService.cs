using Microsoft.Extensions.Logging;
using Test._4Create.Data;
using Test._4Create.Domain.Infrastructure;
using Test._4Create.Domain.Models;

namespace Test._4Create.Domain.Services
{
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
            var endDateOrDefault = clinicalTrialMetadata.EndDate == null && clinicalTrialMetadata.Status == TrialStatus.Ongoing
                ? clinicalTrialMetadata.StartDate.AddMonths(1)
                : clinicalTrialMetadata.EndDate;

            var durationDays = clinicalTrialMetadata.EndDate != null
                ? (int)(clinicalTrialMetadata.EndDate.Value - clinicalTrialMetadata.StartDate).TotalDays
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

        public void GetTrialMetadataById(string id)
        {
            throw new NotImplementedException();
        }
    }
}
