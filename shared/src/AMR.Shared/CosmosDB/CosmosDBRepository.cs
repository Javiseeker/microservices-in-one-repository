using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace AMR.Shared.CosmosDB;

public class CosmosDBRepository<T> : ICosmosDBRepository<T> where T : class
{
    private readonly CosmosDBSettings _cosmosDBSettings;
    private readonly CosmosClient _cosmosClient;

    public string DatabaseName { get; set; }

    public string ContainerName { get; set; } = null!;

    public CosmosDBRepository(IOptions<CosmosDBSettings> cosmosDBOptions, CosmosClient cosmosClient)
    {
        _cosmosDBSettings = cosmosDBOptions.Value;
        _cosmosClient = cosmosClient;
        DatabaseName = _cosmosDBSettings.DatabaseName;
    }

    public Task SetContainer(string container)
    {
        ContainerName = container;
        return Task.CompletedTask;
    }

    public async Task AddItemAsync(T item)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        await container.CreateItemAsync(item);
    }

    public async Task<T> GetItemAsync(string id, string partitionKey)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        try
        {
            ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        var query = container.GetItemLinqQueryable<T>(true)
            .Where(predicate)
            .ToFeedIterator();

        List<T> results = new();
        while (query.HasMoreResults)
        {
            FeedResponse<T> response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task RemoveItemAsync(string id, string partitionKey)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        await container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
    }

    public async Task UpdateItemAsync(string id, string partitionKey, T item)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        await container.ReplaceItemAsync<T>(item, id, new PartitionKey(partitionKey));
    }

    public async Task AddSubItemAsync(string id, string partitionKey, string key, T item)
    {
        var jsonItem = JObject.FromObject(item);
        var objectToInsert = jsonItem[key][0];
        var tasks = new List<Task>(_cosmosDBSettings.AmountToInsert);
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        var cosmosItem = new JObject();
        ItemResponse<JObject> response;

        tasks.Add(container.ReadItemAsync<JObject>(id, new PartitionKey(partitionKey))
            .ContinueWith(itemResponse =>
            {
                if (!itemResponse.IsCompletedSuccessfully)
                {
                    AggregateException innerExceptions = itemResponse.Exception.Flatten();
                    if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                    {
                        container.CreateItemAsync(item);
                    }
                    else
                    {
                        throw innerExceptions;
                    }
                }
                else
                {
                    response = itemResponse.Result;
                    cosmosItem = response.Resource;

                    if (cosmosItem.TryGetValue(key, StringComparison.Ordinal, out var newProperty))
                    {
                        List<object> actualCosmosValues = JsonConvert.DeserializeObject<List<object>>(newProperty.ToString());

                        actualCosmosValues.Add(objectToInsert);
                        cosmosItem[key] = JToken.FromObject(actualCosmosValues);

                        container.ReplaceItemAsync(cosmosItem, id, new PartitionKey(partitionKey));

                    }
                    else
                    {
                        List<object> newPropertyToInsert = new();
                        newPropertyToInsert.Add(objectToInsert);
                        cosmosItem[key] = JToken.FromObject(newPropertyToInsert);

                        container.ReplaceItemAsync(cosmosItem, id, new PartitionKey(partitionKey));
                    }
                }
            }
        ));

        // Wait until all are done
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task UpdateSubItemAsync(string id, string partitionKey, string key, T item)
    {
        var jsonItem = JObject.FromObject(item);
        var objectToInsert = jsonItem[key][0];

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        ItemResponse<JObject> response = await container.ReadItemAsync<JObject>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        var cosmosItem = response.Resource;

        List<object> actualCosmosValues = JsonConvert.DeserializeObject<List<object>>(cosmosItem[key].ToString());
        var cosmosValueIndex = actualCosmosValues.FindIndex(x =>
        {
            return JObject.Parse(x.ToString())["id"].Equals(objectToInsert["id"]);
        });

        actualCosmosValues[cosmosValueIndex] = objectToInsert;
        cosmosItem[key] = JToken.FromObject(actualCosmosValues);

        await container.ReplaceItemAsync(cosmosItem, id, new PartitionKey(partitionKey));
    }

    public async Task RemoveSubItemAsync(string id, string partitionKey, string key, string subItemId)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        ItemResponse<JObject> response = await container.ReadItemAsync<JObject>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        var cosmosItem = response.Resource;

        List<object> actualCosmosValues = JsonConvert.DeserializeObject<List<object>>(cosmosItem[key].ToString());
        var cosmosValueIndex = actualCosmosValues.FindIndex(x =>
        {
            return JObject.Parse(x.ToString())["id"].ToString().Equals(subItemId);
        });

        actualCosmosValues.RemoveAt(cosmosValueIndex);
        cosmosItem[key] = JToken.FromObject(actualCosmosValues);

        await container.ReplaceItemAsync(cosmosItem, id, new PartitionKey(partitionKey));
    }
}
