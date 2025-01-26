using Test._4Create.Data.Entities;

namespace Test._4Create.Data;

public sealed class UnitOfWork(TrialDbContext context) : IDisposable
{
    private GenericRepository<ClinicalTrialMetadata>? _clinicalTrialMetadataRepository;

    private bool _disposed;

    public GenericRepository<ClinicalTrialMetadata> ClinicalTrialMetadataRepository => _clinicalTrialMetadataRepository ??= new(context);

    public void Save() => context.SaveChanges();
    public async Task SaveAsync() => await context.SaveChangesAsync();

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}



