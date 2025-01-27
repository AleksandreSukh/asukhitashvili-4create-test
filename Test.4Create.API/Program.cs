using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Test._4Create.Data;
using Test._4Create.Data.Migrations;
using Test._4Create.Domain.Services;

namespace Test._4Create.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine("Current Env:" + environment);


            Func<IServiceProvider, string> connectionStringResolver = (c) => c.GetRequiredService<IConfiguration>()
                .GetValue<string>("DataAccess:SQLConnectionString") ?? throw new InvalidOperationException("Connection string must be defined in configuration");

            builder.Services.AddScoped(c =>
            {
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    Console.WriteLine("Current Env:"+environment);
                    var connectionString = connectionStringResolver(c);
                    var optionsBuilder = new DbContextOptionsBuilder<TrialDbContext>();
                    optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure().CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));

                    return new TrialDbContext(optionsBuilder.Options);
                }
            );

            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionStringResolver)
                    .ScanIn(typeof(InitializeDb).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            builder.Services.AddScoped(c => new UnitOfWork(c.GetRequiredService<TrialDbContext>()));
            builder.Services.AddScoped<TrialProcessingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
