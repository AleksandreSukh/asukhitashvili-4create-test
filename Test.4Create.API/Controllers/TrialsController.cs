using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Test._4Create.API.Validation;
using Test._4Create.Domain.Models;
using Test._4Create.Domain.Models.Validation;
using Test._4Create.Domain.Services;

namespace Test._4Create.API.Controllers
{
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
        /// Get Specific Record by ID: Implement an endpoint to retrieve a specific record using its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("{id:string}")]
        public IEnumerable<TrialGetModel> Get([FromRoute] string id)
        {
            _logger.LogInformation("Get trial request initiated");

            _trialProcessingService.GetTrialMetadataById(id);

            _logger.LogInformation("Get trial request served");
        }

        /// <summary>
        /// Filtering Support: Allow filtering of records based on query parameters (e.g., status)
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>

        [HttpGet("search")]
        public IEnumerable<TrialGetModel> Get([FromQuery] string status)
        {
            _logger.LogInformation("Search request initiated");

            _logger.LogInformation("Search request served");
            throw new NotImplementedException();
        }

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

            var schemaJson = ResourceHelper.GetEmbeddedResource("Resources.ClinicalTrialMetadata.schema.json");
            var schema = await NJsonSchema.JsonSchema.FromJsonAsync(schemaJson);

            var jsonContent = await ReadJsonContent(file);

            var errors = schema.Validate(jsonContent);
            if (errors.Count > 0)
            {
                var errorString = string.Join("; ", errors.Select(e => e.ToString()));
                _logger.LogError($"JSON upload failed due to format errors:{errorString}");
                return BadRequest($"JSON format errors:{errorString}");
            }

            ClinicalTrialMetadata? clinicalTrialMetadata;
            try
            {
                clinicalTrialMetadata = JsonSerializer.Deserialize<ClinicalTrialMetadata>(jsonContent);
            }
            catch (JsonException ex)
            {
                return BadRequest(new { Message = "JSON parsing failed", Error = ex.Message });
            }

            var dataValidationResult = new ClinicalTrialMetadataValidator().Validate(clinicalTrialMetadata!);
            if (!dataValidationResult.IsValid)
            {
                var dataValidationErrors = dataValidationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "JSON data validatoin failed", Error = string.Join("; ", dataValidationErrors) });
            }

            var trialMetadataSavingResult = await _trialProcessingService.SaveTrialMetadata(clinicalTrialMetadata!);

            if (!trialMetadataSavingResult.Success)
            {
                _logger.LogInformation("JSON upload failed due to persistence error:" + trialMetadataSavingResult.Errors.First().Message);
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }

            _logger.LogInformation("JSON upload completed");

            return Ok(new
            {
                Message = "JSON uploaded successfully.",
            });
        }

        private static async Task<string> ReadJsonContent(IFormFile file)
        {
            using var streamReader = new StreamReader(file.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync();

            return jsonContent;
        }
    }

    public class TrialGetModel
    {
    }
}
