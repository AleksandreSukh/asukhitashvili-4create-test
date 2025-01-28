using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using Test._4Create.API.InputValidation;
using Test._4Create.Domain.Infrastructure;
using Test._4Create.Domain.Models;
using Test._4Create.Domain.Services;

namespace Test._4Create.API.Controllers;

[ApiController]
[Route("trials")]
public class TrialsController : ControllerBase
{
    private readonly ILogger<TrialsController> _logger;
    private readonly TrialProcessingService _trialProcessingService;

    public TrialsController(ILogger<TrialsController> logger, TrialProcessingService trialProcessingService)
    {
        _logger = logger;
        _trialProcessingService = trialProcessingService;
    }

    /// <summary>
    ///     Returns specific records by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public ActionResult<ClinicalTrialMetadataReadModel> Get([FromRoute] string id)
    {
        _logger.LogInformation("Get trial request initiated");

        var result = _trialProcessingService.GetTrialMetadataById(id);

        if (!result.IsSuccessful)
        {
            var error = result.Errors.First();
            if (error.Code == ErrorCodes.TrialMetadataProcessing.TrialMetadataWasNotFound)
            {
                return NotFound();
            }

            _logger.LogError($"Get trial data failed with unexpected error:{error}");
            return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation("Get trial request served");

        return Ok(result.Data);
    }

    /// <summary>
    ///     Search records based on query parameters (e.g., status)
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpGet("search")]
    public ActionResult<List<ClinicalTrialMetadataReadModel>> Search([FromQuery] string? status)
    {
        _logger.LogInformation("Search request initiated");

        var result = _trialProcessingService.SearchTrialMetadatas(new() { Status = status });

        if (!result.IsSuccessful)
        {
            var error = result.Errors.First();
            _logger.LogError($"Search trial data failed with unexpected error:{error}");

            return BadRequest(error);
        }

        _logger.LogInformation("Search request served");

        return Ok(result.Data);
    }

    /// <summary>
    ///     Upload metadata JSON file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("upload-json")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        _logger.LogInformation("JSON upload initiated");

        var fileValidationResult = new UploadFileValidator().Validate(file);

        if (!fileValidationResult.IsValid)
        {
            var errorString = string.Join("; ", fileValidationResult.Errors.Select(e => e.ToString()));
            _logger.LogError($"JSON upload failed due to validation error:{errorString}");
            return BadRequest(errorString);
        }

        var jsonContent = await ReadJsonContent(file);

        var schemaJson = ResourceHelper.GetEmbeddedResource("Resources.ClinicalTrialMetadata.schema.json");
        var schema = await JsonSchema.FromJsonAsync(schemaJson);

        var errors = schema.Validate(jsonContent);
        if (errors.Count > 0)
        {
            var errorString = string.Join("; ", errors.Select(e => e.ToString()));
            _logger.LogError($"JSON upload failed due to format errors:{errorString}");
            return BadRequest($"JSON format errors:{errorString}");
        }

        ClinicalTrialMetadataInputModel? clinicalTrialMetadata;
        try
        {
            clinicalTrialMetadata = JsonSerializer.Deserialize<ClinicalTrialMetadataInputModel>(jsonContent);
        }
        catch (JsonException ex)
        {
            return BadRequest(new { Message = "JSON parsing failed", Error = ex.Message });
        }

        var trialMetadataSavingResult = await _trialProcessingService.ProcessTrialMetadata(clinicalTrialMetadata!);

        if (!trialMetadataSavingResult.IsSuccessful)
        {
            var error = trialMetadataSavingResult.Errors!.First();
            if (error.Code == ErrorCodes.TrialMetadataProcessing.TrialMetadataValidationError)
            {
                return BadRequest(new { Message = "JSON data validatoin failed", Error = error.Message });
            }

            _logger.LogInformation("JSON upload failed due to persistence error:" + error.Message);
            return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation("JSON upload completed");

        return Ok(new
        {
            Message = "JSON uploaded successfully."
        });
    }

    private static async Task<string> ReadJsonContent(IFormFile file)
    {
        using var streamReader = new StreamReader(file.OpenReadStream());
        var jsonContent = await streamReader.ReadToEndAsync();

        return jsonContent;
    }
}