using Framework.ApiResponse;
using JobScheduler.API.Common;
using JobScheduler.API.Middlewares;
using JobScheduler.Application;
using JobScheduler.Infrastructure;
using JobScheduler.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace JobScheduler.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(builder.Configuration) 
               .Enrich.FromLogContext()
               .Enrich.WithProperty("Application", "JobScheduler") 
               .CreateLogger();

            builder.Host.UseSerilog();


            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context => context.ToApiResponse();
                });

            builder.Services.AddAppSettings(builder.Configuration, out var appSettings);            

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration);

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtConfig = builder.Configuration.GetSection("Jwt");

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtConfig["Key"]!)
                        ),
                        ClockSkew = TimeSpan.FromSeconds(10)
                    };
                });

            builder.Services.ConfigureSwagger("PEP Agent API", "V1");

            var coresPolicyName = "ConfiguredCorsPolicy";
            builder.Services.AddConfiguredCors(builder.Configuration, coresPolicyName);

            var app = builder.Build();

            app.UseLogWithCorrelationId();
            app.ApplyMigrationsAndSeed<Infrastructure.Persistence.JobSchedulerDbContext>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PEP Agent API v1");
                });
            }

            app.UseCors(coresPolicyName);

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseGeneralExceptionHandling();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseApiPermissionAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}