using Test._4Create.Data.Entities;
using Test._4Create.Domain.Models;

namespace Test._4Create.Domain.Mappers;

public static class ClinicalTrialMetadataReadModelMapper
{
    public static ClinicalTrialMetadataReadModel ToClinicalTrialMetadataReadModel(this ClinicalTrialMetadata entity) =>
        new(
            entity.TrialId,
            entity.Title,
            entity.StartDate,
            entity.EndDate,
            entity.Participants,
            Enum.Parse<TrialStatus>(entity.Status));
}