using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace ShareBook.Api.Configuration
{
    public static class ConfigureSwagger
    {
        public static IServiceCollection ConfigureSwaggerData(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "SHAREBOOK API",
                    Version = "v2",
                    Description = "A Backend Open Source for Sharebook app. Using Asp.Net Core 3.1",
                    Contact = new OpenApiContact
                    {
                        Name = "Sharebook",
                        //Url = new Uri("https://www.linkedin.com/company/sharebook-br/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Open Source Project.",
                    }
                });
                swagger.ResolveConflictingActions(x => x.First());
                swagger.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    }
                );
                swagger.AddSecurityRequirement(
                    new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.ApiKey,
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    }
                );
            });

            return services;
        }
    }
}
