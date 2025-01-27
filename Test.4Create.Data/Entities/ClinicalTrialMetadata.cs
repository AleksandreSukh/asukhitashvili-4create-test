using System.ComponentModel.DataAnnotations;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Test._4Create.Data.Entities;

public class ClinicalTrialMetadata
{
    [Required]
    public string TrialId { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Participants { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    public int? DurationDays { get; set; }
}