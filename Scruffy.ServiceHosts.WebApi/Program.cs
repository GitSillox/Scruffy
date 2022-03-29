using Microsoft.OpenApi.Models;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;

using Serilog;

namespace Scruffy.ServiceHosts.WebApi;

/// <summary>
/// Program
/// </summary>
public class Program
{
    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">Arguments</param>
    public static void Main(string[] args)
    {
        LoggingService.Initialize(config => config.Enrich.FromLogContext().CreateBootstrapLogger());
        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Starting up", null);

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
                                           {
                                               c.SwaggerDoc("v1", new OpenApiInfo { Title = "Scruffy WebAPI", Version = "v1" });

                                               c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                                                                                 {
                                                                                     Name = "Authorization",
                                                                                     Type = SecuritySchemeType.ApiKey,
                                                                                     Scheme = "Bearer",
                                                                                     BearerFormat = "JWT",
                                                                                     In = ParameterLocation.Header,
                                                                                     Description = "JWT Authorization header using the Bearer scheme."
                                                                                 });
                                               c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                                                                                new string[] { }
                                                                            }
                                                                        });
                                           });
            builder.Services.AddAuthentication("Bearer")
                   .AddJwtBearer(options =>
                                 {
                                     options.Authority = Environment.GetEnvironmentVariable("SCRUFFY_AUTHORITY");
                                     options.TokenValidationParameters.ValidateAudience = false;
                                     options.RequireHttpsMetadata = false;
                                 });
            builder.Services.AddAuthorization(options => options.AddPolicy("ApiScope", policy =>
                                                                                       {
                                                                                           policy.RequireAuthenticatedUser();
                                                                                           policy.RequireClaim("scope", "api_v1");
                                                                                       }));

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers().RequireAuthorization("ApiScope");
            app.Run();
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(Program), "Unhandled exception", null, ex);
        }
        finally
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Shut down complete", null);
            LoggingService.CloseAndFlush();
        }
    }
}