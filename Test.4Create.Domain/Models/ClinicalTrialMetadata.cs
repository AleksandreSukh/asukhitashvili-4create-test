using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Test._4Create.Domain.Models;

//TODO: Create separate read model
public class ClinicalTrialMetadata
{
    [Required]
    [JsonPropertyName("trialId")]
    public string TrialId { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [Required]
    [JsonPropertyName("startDate")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [JsonPropertyName("participants")]
    public int Participants { get; set; }

    [Required]
    [JsonPropertyName("status")]
    [EnumDataType(typeof(TrialStatus))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrialStatus Status { get; set; }
}