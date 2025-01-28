using Test._4Create.Domain.Models.Validation;
using Test._4Create.Domain.Models;

namespace Test._4Create.Domain.Tests.Models.Validation;

[TestFixture]
public class ClinicalTrialMetadataValidatorTests
{
    private ClinicalTrialMetadataValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ClinicalTrialMetadataValidator();
    }

    [Test]
    public void Validate_ValidInput_ReturnsNoValidationErrors()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "Clinical Trial 1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Participants = 100,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_EmptyTrialId_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "",
            Title = "Clinical Trial 1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Participants = 100,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage,Is.EqualTo("Trial ID is required."));
    }

    [Test]
    public void Validate_EmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Participants = 100,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Title is required."));
    }

    [Test]
    public void Validate_InvalidStartDate_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "Clinical Trial 1",
            StartDate = default,
            EndDate = DateTime.UtcNow,
            Participants = 100,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Start date is required."));
    }

    [Test]
    public void Validate_EndDateBeforeStartDate_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "Clinical Trial 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1),
            Participants = 100,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage,Is.EqualTo("End date must be on or after the start date."));
    }

    [Test]
    public void Validate_ParticipantsLessThanOne_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "Clinical Trial 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Participants = 0,
            Status = TrialStatus.Ongoing
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage,Is.EqualTo("Participants must be at least 1."));
    }

    [Test]
    public void Validate_InvalidStatus_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            TrialId = "trial-1",
            Title = "Clinical Trial 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Participants = 100,
            Status = (TrialStatus)999 // Invalid enum value
        };

        // Act
        var result = _validator.Validate(input);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Status must be one of the valid values: NotStarted, Ongoing, or Completed."));
    }
}

