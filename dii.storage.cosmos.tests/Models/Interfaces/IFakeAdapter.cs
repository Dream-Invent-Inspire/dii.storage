using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.tests.Models.Interfaces
{
    public interface IFakeAdapter<T> where T : IDiiEntity, new()
    {
        Task<T> GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default);
        Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<T> CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> ReplaceAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> UpsertAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> PatchAsync(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> PatchBulkAsync(IReadOnlyList<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteEntitiesBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
    }
}