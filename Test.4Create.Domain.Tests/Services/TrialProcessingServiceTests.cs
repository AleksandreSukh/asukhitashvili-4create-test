using System.Linq.Expressions;
using FluentValidation.Results;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Test._4Create.Data;
using Test._4Create.Domain.Infrastructure;
using Test._4Create.Domain.Mappers;
using Test._4Create.Domain.Models;
using Test._4Create.Domain.Models.Validation;
using Test._4Create.Domain.Services;
using ClinicalTrialMetadata = Test._4Create.Data.Entities.ClinicalTrialMetadata;

namespace Test._4Create.Domain.Tests.Services;

[TestFixture]
public class TrialProcessingServiceTests
{
    private TrialProcessingService _service;
    private IGenericRepository<ClinicalTrialMetadata> _trialMetadataRepository;
    private IUnitOfWork _unitOfWork;
    private IClinicalTrialMetadataValidator _validator;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _trialMetadataRepository = Substitute.For<IGenericRepository<ClinicalTrialMetadata>>();
        _unitOfWork.ClinicalTrialMetadataGenericRepository
                   .Returns(_trialMetadataRepository);
        _validator = Substitute.For<IClinicalTrialMetadataValidator>();
        _service = new(_unitOfWork, _validator);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task ProcessTrialMetadata_ValidInput_SavesMetadataSuccessfully()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            Title = "Trial 1",
            TrialId = "trial-1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Status = TrialStatus.Completed,
            Participants = 100
        };

        _validator.Validate(input).Returns(new ValidationResult());

        // Act
        var result = await _service.ProcessTrialMetadata(input);

        // Assert
        Assert.That(result.IsSuccessful);
        _trialMetadataRepository.Received(1).Insert(Arg.Any<ClinicalTrialMetadata>());

        await _unitOfWork.Received(1).SaveAsync();
    }

    [Test]
    public async Task ProcessTrialMetadata_InvalidInput_ReturnsValidationError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            Title = string.Empty,
            TrialId = "trial-1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Status = TrialStatus.Completed,
            Participants = 100
        };

        var validationErrors = new List<ValidationFailure>
        {
            new("Title", "Title is required.")
        };

        _validator.Validate(input).Returns(new ValidationResult(validationErrors));

        // Act
        var result = await _service.ProcessTrialMetadata(input);

        // Assert
        Assert.That(result.IsSuccessful, Is.False);
        var error = result.Errors!.First();
        Assert.That(error.Code, Is.EqualTo(ErrorCodes.TrialMetadataProcessing.TrialMetadataValidationError));
        Assert.That(error.Message, Is.EqualTo("Title is required."));
        _unitOfWork.Received(0).ClinicalTrialMetadataGenericRepository.Insert(Arg.Any<ClinicalTrialMetadata>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Test]
    public async Task ProcessTrialMetadata_ExceptionDuringSave_ReturnsPersistenceError()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            Title = "Trial 1",
            TrialId = "trial-1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            Status = TrialStatus.Completed,
            Participants = 100
        };

        _validator.Validate(input).Returns(new ValidationResult());
        var e = new Exception("Database error");
        _unitOfWork.SaveAsync().ThrowsAsync(e);

        // Act
        var result = await _service.ProcessTrialMetadata(input);

        // Assert
        Assert.That(result.IsSuccessful, Is.False);
        var error = result.Errors!.First();
        Assert.That(error.Code, Is.EqualTo(ErrorCodes.TrialMetadataProcessing.TrialMetadataPersistenceError));
        Assert.That(error.Message.Contains("Database error"));
    }

    [Test]
    public async Task ProcessTrialMetadata_EndDateNullAndStatusOngoing_CalculatesDefaultEndDate()
    {
        // Arrange
        var input = new ClinicalTrialMetadataInputModel
        {
            Title = "Trial 1",
            TrialId = "trial-1",
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = null,
            Status = TrialStatus.Ongoing,
            Participants = 100
        };

        _validator.Validate(input).Returns(new ValidationResult());

        ClinicalTrialMetadata? savedMetadata = null;
        _trialMetadataRepository.When(r => r.Insert(Arg.Any<ClinicalTrialMetadata>()))
                                .Do(args => savedMetadata = args.Arg<ClinicalTrialMetadata>());

        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessTrialMetadata(input);

        // Assert
        Assert.That(result.IsSuccessful);
        Assert.That(savedMetadata, Is.Not.Null);
        Assert.That(savedMetadata.EndDate, Is.EqualTo(input.StartDate.AddMonths(1)));
        Assert.That(savedMetadata.DurationDays, Is.EqualTo((int) (input.StartDate.AddMonths(1) - input.StartDate).TotalDays));
    }

    [Test]
    public void GetTrialMetadataById_MetadataExists_ReturnsOkWithMetadata()
    {
        // Arrange
        var trialId = "test-id";
        var clinicalTrialEntity = new ClinicalTrialMetadata
        {
            Title = "Test Trial",
            TrialId = trialId,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow,
            Status = "Completed",
            Participants = 150
        };

        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    null,
                                    Arg.Any<string>())
                                .Returns(new List<ClinicalTrialMetadata> { clinicalTrialEntity });

        // Act
        var result = _service.GetTrialMetadataById(trialId);

        // Assert
        Assert.That(result.IsSuccessful);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo(clinicalTrialEntity.ToClinicalTrialMetadataReadModel()));
    }

    [Test]
    public void GetTrialMetadataById_MetadataNotFound_ReturnsError()
    {
        // Arrange
        var trialId = "non-existent-id";
        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    Arg.Any<Func<IQueryable<ClinicalTrialMetadata>, IOrderedQueryable<ClinicalTrialMetadata>>?>())
                                .Returns(Enumerable.Empty<ClinicalTrialMetadata>().AsQueryable());

        // Act
        var result = _service.GetTrialMetadataById(trialId);

        // Assert
        Assert.IsFalse(result.IsSuccessful);
        var error = result.Errors!.First();
        Assert.That(error.Code, Is.EqualTo(ErrorCodes.TrialMetadataProcessing.TrialMetadataWasNotFound));
        Assert.That(error.Message, Is.EqualTo($"Medatada with id:{trialId} wasn't found"));
    }

    [Test]
    public void GetTrialMetadataById_NullId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GetTrialMetadataById(null!));
    }

    [Test]
    public void GetTrialMetadataById_InvalidStatus_ThrowsException()
    {
        // Arrange
        var trialId = "test-id";
        var clinicalTrialEntity = new ClinicalTrialMetadata
        {
            Title = "Test Trial",
            TrialId = trialId,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow,
            Status = "InvalidStatus", // Invalid status for parsing
            Participants = 150
        };

        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    Arg.Any<Func<IQueryable<ClinicalTrialMetadata>, IOrderedQueryable<ClinicalTrialMetadata>>?>())
                                .Returns(new List<ClinicalTrialMetadata> { clinicalTrialEntity }.AsQueryable());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetTrialMetadataById(trialId));
    }

    //3

    [Test]
    public void SearchTrialMetadatas_ValidStatus_ReturnsMatchingResults()
    {
        // Arrange
        var searchParams = new ClinicalTrialMetadataSearchParams { Status = "Completed" };
        var clinicalTrialEntities = new List<ClinicalTrialMetadata>
        {
            new()
            {
                TrialId = "trial-1",
                Title = "Trial 1",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1),
                Status = "Completed",
                Participants = 50
            },
            new()
            {
                TrialId = "trial-2",
                Title = "Trial 2",
                StartDate = DateTime.UtcNow.AddMonths(-3),
                EndDate = DateTime.UtcNow.AddMonths(-2),
                Status = "Completed",
                Participants = 70
            }
        };

        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    Arg.Any<Func<IQueryable<ClinicalTrialMetadata>, IOrderedQueryable<ClinicalTrialMetadata>>?>())
                                .Returns(clinicalTrialEntities.AsQueryable());

        // Act
        var result = _service.SearchTrialMetadatas(searchParams);

        // Assert
        Assert.That(result.IsSuccessful);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(2));
        Assert.That(result.Data[0].Title, Is.EqualTo("Trial 1"));
        Assert.That(result.Data[1].Title, Is.EqualTo("Trial 2"));
    }

    [Test]
    public void SearchTrialMetadatas_InvalidStatus_ReturnsError()
    {
        // Arrange
        var searchParams = new ClinicalTrialMetadataSearchParams { Status = "InvalidStatus" };

        // Act
        var result = _service.SearchTrialMetadatas(searchParams);

        // Assert
        Assert.That(result.IsSuccessful, Is.False);
        var error = result.Errors!.First();
        Assert.That(error.Code, Is.EqualTo(ErrorCodes.TrialMetadataProcessing.ParseStatusError));
        Assert.That(error.Message.Contains("Invalid value for status:InvalidStatus"));
    }

    [Test]
    public void SearchTrialMetadatas_EmptyStatus_ReturnsAllResults()
    {
        // Arrange
        var searchParams = new ClinicalTrialMetadataSearchParams { Status = string.Empty };
        var clinicalTrialEntities = new List<ClinicalTrialMetadata>
        {
            new()
            {
                TrialId = "trial-1",
                Title = "Trial 1",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1),
                Status = "Completed",
                Participants = 50
            },
            new()
            {
                TrialId = "trial-2",
                Title = "Trial 2",
                StartDate = DateTime.UtcNow.AddMonths(-3),
                EndDate = DateTime.UtcNow.AddMonths(-2),
                Status = "Ongoing",
                Participants = 70
            }
        };

        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    Arg.Any<Func<IQueryable<ClinicalTrialMetadata>, IOrderedQueryable<ClinicalTrialMetadata>>?>())
                                .Returns(clinicalTrialEntities.AsQueryable());

        // Act
        var result = _service.SearchTrialMetadatas(searchParams);

        // Assert
        Assert.That(result.IsSuccessful);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(2));
    }

    [Test]
    public void SearchTrialMetadatas_NullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.SearchTrialMetadatas(null!));
    }

    [Test]
    public void SearchTrialMetadatas_NoMatchingStatus_ReturnsEmptyResults()
    {
        // Arrange
        var searchParams = new ClinicalTrialMetadataSearchParams { Status = "Completed" };
        _trialMetadataRepository.Get(
                                    Arg.Any<Expression<Func<ClinicalTrialMetadata, bool>>?>(),
                                    Arg.Any<Func<IQueryable<ClinicalTrialMetadata>, IOrderedQueryable<ClinicalTrialMetadata>>?>())
                                .Returns(Enumerable.Empty<ClinicalTrialMetadata>().AsQueryable());

        // Act
        var result = _service.SearchTrialMetadatas(searchParams);

        // Assert
        Assert.That(result.IsSuccessful);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(0));
    }
}