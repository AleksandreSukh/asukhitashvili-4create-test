using Test._4Create.Data;
using Test._4Create.Domain.Models;

namespace Test._4Create.Domain.Services
{
    public class TrialProcessingService
    {
        private readonly UnitOfWork _unitOfWork;

        public TrialProcessingService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SaveTrialMetadata(ClinicalTrialMetadata clinicalTrialMetadata)
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
            await _unitOfWork.SaveAsync();
        }
    }
}
