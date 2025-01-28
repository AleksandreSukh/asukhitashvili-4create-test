using FluentValidation;

namespace Test._4Create.API.InputValidation;

public class UploadFileValidator : AbstractValidator<IFormFile>
{
    private const int MaxFileSizeInMb = 2;
    private const long MaxFileSizeInBytes = MaxFileSizeInMb * 1024 * 1024;

    public UploadFileValidator()
    {
        var invalidFileNameChars = Path.GetInvalidFileNameChars();

        RuleFor(f => f)
            .NotNull()
            .WithMessage("File is not provided.");

        RuleFor(f => f.Length)
            .NotEqual(0)
            .WithMessage("Provided file is empty")
            .LessThan(MaxFileSizeInBytes)
            .WithMessage($"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / (1024 * 1024)} MB.");

        RuleFor(f => f.ContentType)
            .Must(ct => ct.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only JSON files are allowed.");

        RuleFor(f => f.FileName)
            .NotNull()
            .NotEmpty()
            .WithMessage("File name shouldn't be empty")
            .Must(fn => fn.Length <= 255)
            .WithMessage("File name is too long.")
            .Must(fn => fn.IndexOfAny(invalidFileNameChars) < 0)
            .WithMessage("File name contains invalid characters.")
            .Must(fn => Path.GetExtension(fn).Equals(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only files with \".json\" extension are allowed.");
    }
}