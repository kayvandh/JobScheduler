using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace JobScheduler.API.Common
{
    public static class Extensions
    {
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
    }
}
