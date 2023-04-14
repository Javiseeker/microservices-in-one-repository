using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AMR.Shared.Logging;

public static class LoggingConfig
{

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        builder.Services.AddApplicationInsightsTelemetry();

    }
}
