using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AMR.Shared.CosmosDB;

public static class CosmosDBConfig
{
    public static void ConfigureCosmosDB(this WebApplicationBuilder builder)
    {
        var cosmosDBSettings = new CosmosDBSettings();
        builder.Configuration.Bind(CosmosDBSettings.SectionName, cosmosDBSettings);
        builder.Services.AddSingleton(Options.Create(cosmosDBSettings));

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
            {
                HttpClientFactory = httpClientFactory.CreateClient,
                AllowBulkExecution = true,
                ConnectionMode = ConnectionMode.Gateway
            };

            return new CosmosClient(cosmosDBSettings.EndpointUrl, cosmosDBSettings.AuthorizationKey, cosmosClientOptions);
        });
        builder.Services.AddSingleton(typeof(ICosmosDBRepository<>), typeof(CosmosDBRepository<>));
    }
}
