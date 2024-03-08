using System.Reflection;
using AMR.Shared.Common.Auth;
using AMR.Shared.CosmosDB;
using AMR.Shared.Logging;
using AMR.Shared.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using static AMR.Shared.Constants.Policies;

namespace AMR.Shared;

public static class ProgramConfig
{
    public static WebApplication Initialize<TProgram>(string[] args, Action<WebApplicationBuilder> serviceConfig, string apiName)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Setup local files and appsettings
        Constants.ApiName = apiName;

        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        builder.Configuration
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", false);

        builder.Configuration.AddEnvironmentVariables();


        // Setup logging to application insights using serilog
        builder.ConfigureLogging();

        // Setup the microservice services
        serviceConfig(builder);

        // Setup versioning and swagger versions
        builder.Services.UseVersioning();
        builder.Services.UseSwagger<TProgram>();

        // Setup CosmosDB
        // builder.ConfigureCosmosDB();

        // Setup Mapster Mapper
        builder.Services.ConfigureMapster();

        // Setup authentication and authorization
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //.AddMicrosoftIdentityWebApi(builder.Configuration);

        builder.Services.AddAuthorization(options =>
        {

            foreach (KeyValuePair<RolesEnum, string[]> p in Roles)
            {
                options.AddPolicy(p.Key.ToString(), policy =>
                {
                    policy.RequireRole(p.Value);
                });
            }
        });

        // Setup auth helper
        builder.Services.AddSingleton<IAuthHelper, AuthHelper>();

        // Setup health checks
        builder.Services.AddHealthChecks();

        //// Add response compression
        builder.Services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/json" });
            options.EnableForHttps = true;
        });

        var app = builder.Build();

        // Setup serilog global logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override($"{apiName}", LogEventLevel.Warning)
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(app.Services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
            .CreateLogger();

        app.UseHttpsRedirection();
        //app.UseAuthentication();
        //app.UseAuthorization();
        app.UseResponseCompression();

        var env = app.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        if (env == "Development" || env == "Staging")
        {
            app.UseSwaggerUi();
        }

        var initialize = app.NewVersionedApi("_");
        initialize.MapGet("/", () => $"{apiName} Web API " + DateTime.Now.ToString()).IsApiVersionNeutral();

        app.MapHealthChecks("/health");

        return app;
    }
}
