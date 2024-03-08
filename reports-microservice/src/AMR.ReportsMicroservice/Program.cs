using AMR.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Net;
using System.Reflection;
using static AMR.Shared.Constants.Policies;
using static System.Net.Mime.MediaTypeNames;

var app = ProgramConfig.Initialize<Program>(args, host =>
{
    // Add microservice secrets
    host.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

    // Add configuration and services related to the microservice here

}, "Reports Microservice");

// Custom Error Handler per microservice
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is Microsoft.Azure.Cosmos.CosmosException e)
        {
            context.Response.StatusCode = (int)e!.StatusCode;
            context.Response.ContentType = Text.Plain;
            Log.Logger.Error(e!.Message);
            await context.Response.WriteAsync(Enum.GetName(typeof(HttpStatusCode), (int)e!.StatusCode));
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = Text.Plain;
            Log.Logger.Error(exceptionHandlerPathFeature?.Error!.Message);
            await context.Response.WriteAsync("An exception was thrown.");
        }
    });
});

// Add endpoints to the microservice here

#region Dummy

var dummySettingsRegion = app.NewVersionedApi("Dummy");
var dummyV1 = dummySettingsRegion.MapGroup("ping").HasApiVersion(1.0);
dummyV1.MapGet("", () => "pong").RequireAuthorization(GetRole(RolesEnum.GeneralUserReadPolicy));
var dummyV2 = dummySettingsRegion.MapGroup("ping").HasApiVersion(2.0);
dummyV2.MapGet("", () =>
{
    return "pong v2";
});

#endregion Dummy

app.Run();

namespace AMR.ReportsMicroservice
{
    public partial class Program
    {
    }
}