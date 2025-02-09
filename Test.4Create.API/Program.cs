using System.Data.Common;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Test._4Create.Data;
using Test._4Create.Data.Migrations;
using Test._4Create.Domain.Models.Validation;
using Test._4Create.Domain.Services;

namespace Test._4Create.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        Func<IServiceProvider, string> connectionStringResolver = c => c.GetRequiredService<IConfiguration>()
                                                                        .GetValue<string>("SQLConnectionString") ?? throw new InvalidOperationException("Connection string must be defined in configuration");

        builder.Services.AddScoped(c =>
            {
                var connectionString = connectionStringResolver(c);
                var optionsBuilder = new DbContextOptionsBuilder<TrialDbContext>();
                optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure().CommandTimeout((int) TimeSpan.FromMinutes(5).TotalSeconds));

                return new TrialDbContext(optionsBuilder.Options);
            }
        );

        builder.Services.AddFluentMigratorCore()
               .ConfigureRunner(rb => rb
                                      .AddSqlServer()
                                      .WithGlobalConnectionString(connectionStringResolver)
                                      .ScanIn(typeof(InitializeDb).Assembly).For.Migrations())
               .AddLogging(lb => lb.AddFluentMigratorConsole());

        builder.Services.AddScoped<IUnitOfWork>(c => new UnitOfWork(c.GetRequiredService<TrialDbContext>()));
        builder.Services.AddScoped<TrialProcessingService>();
        builder.Services.AddSingleton<IClinicalTrialMetadataValidator, ClinicalTrialMetadataValidator>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();

        using (var scope = app.Services.CreateScope())
        {
            CreateDatabaseIfNotExists(connectionStringResolver(scope.ServiceProvider));
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        app.MapControllers();

        app.Run();
    }

    //Used for testing only - should be removed in production code
    private static void CreateDatabaseIfNotExists(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var databaseName = builder["Database"] as string;
        builder["Database"] = null;

        using (var connection = new SqlConnection(builder.ConnectionString))
        {
            connection.Open();

            var cmd = new SqlCommand($"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') BEGIN CREATE DATABASE {databaseName}; END", connection);
            cmd.ExecuteNonQuery();
        }
    }
}