using Test._4Create.Data.Entities;

namespace Test._4Create.Data;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<ClinicalTrialMetadata> ClinicalTrialMetadataGenericRepository { get; }
    void Save();
    Task SaveAsync();
}