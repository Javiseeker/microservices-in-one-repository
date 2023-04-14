using System.Linq.Expressions;

namespace AMR.Shared.CosmosDB;

public interface ICosmosDBRepository<T> where T : class
{
    Task AddItemAsync(T item);

    Task AddSubItemAsync(string id, string partitionKey, string key, T item);

    Task<T> GetItemAsync(string id, string partitionKey);

    Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);

    Task UpdateItemAsync(string id, string partitionKey, T item);

    Task UpdateSubItemAsync(string id, string partitionKey, string key, T item);

    Task RemoveItemAsync(string id, string partitionKey);

    Task RemoveSubItemAsync(string id, string partitionKey, string key, string subItemId);

    Task SetContainer(string container);
}
