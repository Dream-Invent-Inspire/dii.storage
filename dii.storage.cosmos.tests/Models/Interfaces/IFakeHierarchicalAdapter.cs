using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.tests.Models.Interfaces
{
    public interface IFakeHierarchicalAdapter<T> where T : IDiiEntity, new()
    {
        Task<T> GetAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<PagedList<T>> GetManyAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys)> idAndPks, QueryRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default);
        Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null);
        Task<T> CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<List<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> ReplaceAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<List<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> UpsertAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<List<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<T> PatchAsync(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<List<T>> PatchBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteEntitiesBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
    }

}
