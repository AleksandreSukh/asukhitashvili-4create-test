
using Microsoft.EntityFrameworkCore;
using Test._4Create.Data;

namespace Test._4Create.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped(c =>
            {
                var connectionString = c.GetRequiredService<IConfiguration>()
                    .GetValue<string>("DataAccess:SQLConnectionString");

                return TrialDbContextFactory.CreateDbContext(connectionString);
            });

            builder.Services.AddScoped(c => new UnitOfWork(c.GetRequiredService<TrialDbContext>()));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
