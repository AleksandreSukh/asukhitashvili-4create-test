using Microsoft.EntityFrameworkCore;
using Test._4Create.Data.Entities;

namespace Test._4Create.Data;

public class TrialDbContext : DbContext
{
    public TrialDbContext(DbContextOptions<TrialDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClinicalTrialMetadata> ClinicalTrialMetadatas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClinicalTrialMetadata>()
            .HasKey(e => e.TrialId);
    }
}