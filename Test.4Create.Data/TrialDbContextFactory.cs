using Microsoft.EntityFrameworkCore;

namespace Test._4Create.Data;

public class TrialDbContextFactory
{
    public static TrialDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrialDbContext>();
        optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure().CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));

        return new TrialDbContext(optionsBuilder.Options);

    }
}