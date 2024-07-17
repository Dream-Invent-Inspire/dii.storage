using dii.storage.cosmos.Models;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.tests.Adapters
{
    public class FakeHPKEntityAdapter : DiiCosmosHierarchicalAdapter<FakeHPKEntity>, IFakeHierarchicalAdapter<FakeHPKEntity>
    {
        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.GetAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, partitionKeys, requestOptions, cancellationToken);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.GetAsync(string id, Dictionary<string, object> partitionKeys, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, partitionKeys, requestOptions, cancellationToken);
        }

        Task<PagedList<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.GetManyAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys)> idAndPks, QueryRequestOptions readManyRequestOptions, CancellationToken cancellationToken)
        {
            return base.GetManyAsync(idAndPks, null, readManyRequestOptions, cancellationToken);
        }

        Task<PagedList<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.GetPagedAsync(QueryDefinition queryDefinition, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryDefinition, continuationToken, requestOptions);
        }

        Task<PagedList<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.GetPagedAsync(string queryText, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryText, continuationToken, requestOptions);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.CreateAsync(FakeHPKEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.CreateAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<List<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.CreateBulkAsync(IReadOnlyList<FakeHPKEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.CreateBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.ReplaceAsync(FakeHPKEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.ReplaceAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<List<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.ReplaceBulkAsync(IReadOnlyList<FakeHPKEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.ReplaceBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.UpsertAsync(FakeHPKEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.UpsertAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<List<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.UpsertBulkAsync(IReadOnlyList<FakeHPKEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.UpsertBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.PatchAsync(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.PatchAsync(id, partitionKeys, patchOperations, requestOptions, cancellationToken);
        }

        Task<FakeHPKEntity> IFakeHierarchicalAdapter<FakeHPKEntity>.PatchAsync(string id, Dictionary<string, object> partitionKeys, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.PatchAsync(id, partitionKeys, patchOperations, requestOptions, cancellationToken);
        }

        Task<List<FakeHPKEntity>> IFakeHierarchicalAdapter<FakeHPKEntity>.PatchBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.PatchBulkAsync(patchOperations, requestOptions, cancellationToken);
        }

        Task<bool> IFakeHierarchicalAdapter<FakeHPKEntity>.DeleteEntityAsync(FakeHPKEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteEntityAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<bool> IFakeHierarchicalAdapter<FakeHPKEntity>.DeleteAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteEntityByIdAsync(id, partitionKeys, requestOptions, cancellationToken);
        }

        Task<bool> IFakeHierarchicalAdapter<FakeHPKEntity>.DeleteEntitiesBulkAsync(IReadOnlyList<FakeHPKEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteEntitiesBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<bool> IFakeHierarchicalAdapter<FakeHPKEntity>.DeleteBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys)> idAndPks, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteBulkAsync(idAndPks, requestOptions, cancellationToken);
        }
    }

}
