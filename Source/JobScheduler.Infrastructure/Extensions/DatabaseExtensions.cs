using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobScheduler.Infrastructure.Persistence.Interfaces;

namespace JobScheduler.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        public static void ApplyMigrationsAndSeed<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();

            db.Database.Migrate();

            if (db is ISeeder seeder)
            {
                seeder.Seed();
            }
        }
    }
}