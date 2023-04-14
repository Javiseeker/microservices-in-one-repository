using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMR.Shared.CosmosDB;

public class CosmosDBSettings
{
    public const string SectionName = "CosmosDBSettings";
    public string EndpointUrl { get; set; } = null!;
    public string AuthorizationKey { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public int AmountToInsert { get; set; }

    // To fill these container names, add them on appsettings.json of each microservice
    public string MaterialContainerName { get; set; } = string.Empty;
    public string MaterialSettingsContainerName { get; set; } = string.Empty;
    public string ProfileContainerName { get; set; } = string.Empty;
    public string ProfileSettingsContainerName { get; set; } = string.Empty;
    public string TrussDesignContainerName { get; set; } = string.Empty;
    public string UserSettingsContainerName { get; set; } = string.Empty;
    public string ProjectContainerName { get; set; } = string.Empty;

}
