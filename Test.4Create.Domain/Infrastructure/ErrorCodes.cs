namespace Test._4Create.Domain.Infrastructure;
/*
 * Info for the test reviewer:
 * Introducing ErrorCodes class (local for current solution)
 * for managing error codes for consistent logging (across different modules/microservices in case of a distributed application)
 * and log search capability (possibly used with Application Insights or other logging/monitoring services)
 */

public static class ErrorCodes
{
    public const int Undefined = 0;
    public class TrialMetadataProcessing //1-100 inclusive
    {
        public const int TrialMetadataPersistenceError = 1;
    }
}