using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.tests.Models.Interfaces
{
    public interface IFakeReadOnlyAdapter<T> where T : IDiiEntity, new()
    {
        Task<T> GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<List<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default);
        Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null);
    }
}