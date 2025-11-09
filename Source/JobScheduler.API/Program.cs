using Framework.ApiResponse;
using Framework.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using JobScheduler.API.Middlewares;
using JobScheduler.Application;
using JobScheduler.Infrastructure;
using JobScheduler.Infrastructure.Extensions;
using System.Text;

namespace JobScheduler.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context => context.ToApiResponse();
                });

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

            SwaggerOption.ConfigureSwagger(builder, "PEP Agent API", "V1");

            var app = builder.Build();

            app.ApplyMigrationsAndSeed<Infrastructure.Persistence.JobSchedulerDbContext>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PEP Agent API v1");
                });
            }

            app.UseCors(options =>
                options.AllowAnyHeader()
                       .AllowAnyOrigin()
                       .AllowAnyMethod());

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