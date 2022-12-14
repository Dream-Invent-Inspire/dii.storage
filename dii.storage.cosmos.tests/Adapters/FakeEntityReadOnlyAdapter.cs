using dii.storage.cosmos.Models;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.tests.Adapters
{
    public class FakeEntityReadOnlyAdapter : DiiCosmosReadOnlyAdapter<FakeEntity>, IFakeReadOnlyAdapter<FakeEntity>
    {
        public FakeEntityReadOnlyAdapter(string databaseId)
            : base(databaseId) { }

        Task<FakeEntity> IFakeReadOnlyAdapter<FakeEntity>.GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, partitionKey, requestOptions, cancellationToken);
        }

        Task<List<FakeEntity>> IFakeReadOnlyAdapter<FakeEntity>.GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions, CancellationToken cancellationToken)
        {
            return base.GetManyAsync(idAndPks, readManyRequestOptions, cancellationToken);
        }

        Task<PagedList<FakeEntity>> IFakeReadOnlyAdapter<FakeEntity>.GetPagedAsync(QueryDefinition queryDefinition, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryDefinition, continuationToken, requestOptions);
        }

        Task<PagedList<FakeEntity>> IFakeReadOnlyAdapter<FakeEntity>.GetPagedAsync(string queryText, string continuationToken, QueryRequestOptions requestOptions)
        {
            return base.GetPagedAsync(queryText, continuationToken, requestOptions);
        }
    }
}