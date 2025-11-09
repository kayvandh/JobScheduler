using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Framework.Swagger;

public static class SwaggerOption
{
    public static void ConfigureSwagger(this WebApplicationBuilder builder,string title,string version, 
        string scheme = "Bearer",string bearerFormat = "JWT")
    {
        builder.Services.AddSwaggerGen(c =>
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