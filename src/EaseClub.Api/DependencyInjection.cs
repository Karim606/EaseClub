using EaseClub.Api.Common;
using EaseClub.Api.Infrastructure;
using EaseClub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace EaseClub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithJsonConfiguration()
                    .AddIdentityInfrastructure()
                    .AddCustomProblemDetails()
                    .AddCors(configuration)
                    .AddExceptionHandlers()
                    .AddApiVersioning()
                    .ConfigureSwagger()
                    .AddRateLimiting();
            
            return services;
        }

        private static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
        {

            services.AddExceptionHandler<GlobalExceptionHandler>();
            return services;
        }
        private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
        {

            services.AddProblemDetails(options => options.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Extensions["RequestId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

            });
            return services;
        }

        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentRequestContext, CurrentRequestContext>();
            return services;
        }

        private static IServiceCollection AddApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true; // Return the headers "api-supported-versions"
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            //This ensures Swagger generates separate docs per API version.
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        private static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider()
                                   .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = $"EaseClub API {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = "API documentation for EaseClub." + (description.IsDeprecated ? " (this Api version has been deprecated)" : ""),
                    });
                }

                // Register security scheme for Bearer token and add requirement
                AddSecurityScheme(options);

            });

            return services;
        }

        private static IServiceCollection AddControllersWithJsonConfiguration(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            return services;
        }

        private static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddSlidingWindowLimiter("slidingWindowPolicy", limiterOptions =>
                {
                    limiterOptions.AutoReplenishment = true;
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.QueueLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.SegmentsPerWindow = 6;
                });
            });
            return services;
        }
        private static IServiceCollection AddCors (this IServiceCollection services, IConfiguration configuration)
        {
            var corsOrigins = configuration["CORS_ORIGINS"]?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            services.AddCors (options =>
            {
                options.AddPolicy("DefaultCors", policy =>
                {
                    policy.WithOrigins(corsOrigins ?? Array.Empty<string>())
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            return services;
        }

        public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseExceptionHandler()
               .UseStatusCodePages()
               .UseCors("DefaultCors")
               .UseHttpsRedirection()
               .UseMiddleware<RequestLogContextMiddleware>()
               .UseSerilogRequestLogging()
               .UseAuthentication()
               .UseAuthorization();

            return app;
        }

        private static void AddSecurityScheme(SwaggerGenOptions options)
        {
            // Add token authentication option to pass bearer token
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            // Add global security requirement for Bearer token
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            //////////////////////end of function//////////////////////
        }
    }
}
