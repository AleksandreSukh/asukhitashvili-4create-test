namespace Test._4Create.Domain.Models;

public record ClinicalTrialMetadataReadModel(
    string TrialId,
    string Title,
    DateTime StartDate,
    DateTime? EndDate,
    int Participants,
    TrialStatus Status);