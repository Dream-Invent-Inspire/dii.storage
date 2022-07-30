using dii.cosmos.Models;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.cosmos.tests.Adapters
{
    public class FakeEntityAdapter : DiiCosmosAdapter<FakeEntity>, IFakeAdapter<FakeEntity>
    {
        Task<FakeEntity> IFakeAdapter<FakeEntity>.GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, partitionKey, requestOptions, cancellationToken);
        }

        Task<ICollection<FakeEntity>> IFakeAdapter<FakeEntity>.GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions, CancellationToken cancellationToken)
        {
            return base.GetManyAsync(idAndPks, readManyRequestOptions, cancellationToken);
        }

        Task<PagedList<FakeEntity>> IFakeAdapter<FakeEntity>.GetPagedAsync(QueryDefinition queryDefinition, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryDefinition, continuationToken, requestOptions);
        }

        Task<PagedList<FakeEntity>> IFakeAdapter<FakeEntity>.GetPagedAsync(string queryText, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryText, continuationToken, requestOptions);
        }

        Task<FakeEntity> IFakeAdapter<FakeEntity>.CreateAsync(FakeEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.CreateAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<ICollection<FakeEntity>> IFakeAdapter<FakeEntity>.CreateBulkAsync(IReadOnlyList<FakeEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.CreateBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeEntity> IFakeAdapter<FakeEntity>.ReplaceAsync(FakeEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.ReplaceAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<ICollection<FakeEntity>> IFakeAdapter<FakeEntity>.ReplaceBulkAsync(IReadOnlyList<FakeEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.ReplaceBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeEntity> IFakeAdapter<FakeEntity>.UpsertAsync(FakeEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.UpsertAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<ICollection<FakeEntity>> IFakeAdapter<FakeEntity>.UpsertBulkAsync(IReadOnlyList<FakeEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.UpsertBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<FakeEntity> IFakeAdapter<FakeEntity>.PatchAsync(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.PatchAsync(id, partitionKey, patchOperations, requestOptions, cancellationToken);
        }

        Task<ICollection<FakeEntity>> IFakeAdapter<FakeEntity>.PatchBulkAsync(IReadOnlyList<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.PatchBulkAsync(patchOperations, requestOptions, cancellationToken);
        }

        Task<bool> IFakeAdapter<FakeEntity>.DeleteEntityAsync(FakeEntity diiEntity, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteEntityAsync(diiEntity, requestOptions, cancellationToken);
        }

        Task<bool> IFakeAdapter<FakeEntity>.DeleteAsync(string id, string partitionKey, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteAsync(id, partitionKey, requestOptions, cancellationToken);
        }

        Task<bool> IFakeAdapter<FakeEntity>.DeleteEntitiesBulkAsync(IReadOnlyList<FakeEntity> diiEntities, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteEntitiesBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        Task<bool> IFakeAdapter<FakeEntity>.DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.DeleteBulkAsync(idAndPks, requestOptions, cancellationToken);
        }
    }
}