using Test._4Create.Data.Entities;

namespace Test._4Create.Data;

public sealed class UnitOfWork(TrialDbContext context) : IUnitOfWork
{
    private GenericRepository<ClinicalTrialMetadata>? _clinicalTrialMetadataRepository;
    private bool _disposed;

    public IGenericRepository<ClinicalTrialMetadata> ClinicalTrialMetadataGenericRepository => _clinicalTrialMetadataRepository ??= new(context);

    public void Save() => context.SaveChanges();

    public async Task SaveAsync() => await context.SaveChangesAsync();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            context.Dispose();
        }
        _disposed = true;
    }
}