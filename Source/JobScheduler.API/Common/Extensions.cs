using JobScheduler.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace JobScheduler.API.Common
{
    public static class Extensions
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration, out AppSettings? appSettings)
        {
            appSettings = configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();

            if (appSettings == null)
                throw new InvalidOperationException(Resource.Messages.AppSettingsMissing);

            services.AddSingleton(appSettings);
            services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
            return services;
        }

        public static void ConfigureSwagger(this IServiceCollection services, string title, string version,
            string scheme = "Bearer", string bearerFormat = "JWT")
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = scheme,
                    BearerFormat = bearerFormat,
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Secured API, Insert JWT Token Here. Sample: Bearer eyJhbGciOiJIUzI1NiIsInR...",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme,
                        Array.Empty<string>()
                    }
                });
            });
        }

        public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration,string policyName)
        {
            var corsSettings = configuration.Get<AppSettings>()?.CorsSettings;

            if (corsSettings == null)
                throw new InvalidOperationException(Resource.Messages.CorsSettingsMissing);

            services.AddCors(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    
                    if (corsSettings.AllowedOrigins.Count ==  1 && corsSettings.AllowedOrigins[0] == "*")
                        policy.AllowAnyOrigin();
                    else
                        policy.WithOrigins(corsSettings.AllowedOrigins.ToArray());

                    if (corsSettings.AllowedMethods.Count == 1 && corsSettings.AllowedMethods[0] == "*")
                        policy.AllowAnyMethod();
                    else
                        policy.WithMethods(corsSettings.AllowedMethods.ToArray());

                    if (corsSettings.AllowedHeaders.Count == 1 && corsSettings.AllowedHeaders[0] == "*")
                        policy.AllowAnyHeader();
                    else
                        policy.WithHeaders(corsSettings.AllowedHeaders.ToArray());

                    if (corsSettings.AllowCredentials)
                        policy.AllowCredentials();
                });
            });

            return services;
        }


    }
}
