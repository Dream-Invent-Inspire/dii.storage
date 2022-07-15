using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dii.cosmos;
using dii.cosmos.tests.Models;
using System.Threading;
using dii.cosmos.Models;
using Microsoft.Azure.Cosmos;

namespace dii.cosmos.tests.Adapters
{
    //This goes in BLL
    public interface IFakeEntityAdapter
	{
        Task<FakeEntity> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(FakeEntity e, CancellationToken cancellationToken = default);

    }

    //This goes in cosmos.infra
    public class FakeEntitySampleAdapter : DiiCosmosAdapter<FakeEntity>, IFakeEntityAdapter
    {
        //This would be an example of ClientId and EntityId
        //ClientId (for example) would be your partition key and fakeId is your EntityId.
        public async Task<FakeEntity> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default)
		{
            return await base.GetAsync(id, fakeId, cancellationToken).ConfigureAwait(false);
		}

        public async Task<bool> DeleteAsync(FakeEntity e, CancellationToken cancellationToken = default)
		{
            return await base.DeleteAsync(e).ConfigureAwait(false);
		}
    }

    public enum ComparisonType
	{
        GreaterThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        LessThan
	}

    //This would belong in your onion BLL
    public interface IFakeEntityTwoAdapter
	{
        Task<FakeEntityTwo> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default);
        Task<ICollection<FakeEntityTwo>> GetManyByIdsAsync(IReadOnlyList<(string id, string fakeId)> idCollection, CancellationToken cancellationToken = default);
        Task<PagedList<FakeEntityTwo>> GetByLongComparison(long value, ComparisonType comparisonType);
        Task<FakeEntityTwo> CreateAsync(FakeEntityTwo diiEntity, CancellationToken cancellationToken = default);
        Task<ICollection<FakeEntityTwo>> CreateBulkAsync(IReadOnlyList<FakeEntityTwo> diiEntities, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, string fakeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteBulkAsync(CancellationToken cancellationToken = default, params FakeEntityTwo[] entities);
        Task<PagedList<FakeEntityTwo>> GetByIdsAsync(params string[] ids);

    }

    //This would belong in your infra for Cosmos implementing the interface.
    //DI in app start.
    public class FakeEntityTwoSampleAdapter : DiiCosmosAdapter<FakeEntityTwo>, IFakeEntityTwoAdapter
    {
        private Dictionary<ComparisonType, string> comparisons = new Dictionary<ComparisonType, string>
        {
            {ComparisonType.GreaterThan, ">" },
            {ComparisonType.GreaterThanOrEqual, ">=" },
            {ComparisonType.LessThanOrEqual, "<=" },
            {ComparisonType.LessThan, "<" }
        };

        public async Task<FakeEntityTwo> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default)
		{
            return await base.GetAsync(id, fakeId, cancellationToken).ConfigureAwait(false);
		}

        public async Task<ICollection<FakeEntityTwo>> GetManyByIdsAsync(IReadOnlyList<(string id, string fakeId)> idCollection, CancellationToken cancellationToken = default)
		{
            return await base.GetManyAsync(idCollection, cancellationToken).ConfigureAwait(false);
		}

        public async Task<PagedList<FakeEntityTwo>> GetByLongComparison(long value, ComparisonType comparisonType)
		{
            var query = new QueryDefinition($"SELECT * FROM fakeentitytwo fet WHERE fet.long {comparisons[comparisonType]} @comp");
            query.WithParameter("@comp", value);
            return await base.GetPagedAsync(query).ConfigureAwait(false);
        }

        public new async Task<bool> DeleteAsync(string id, string fakeId, CancellationToken cancellationToken = default)
		{
            return await base.DeleteAsync(id, fakeId, cancellationToken).ConfigureAwait(false);
		}

        public async Task<PagedList<FakeEntityTwo>> GetByIdsAsync(params string[] ids)
        {
            var keysDict = new Dictionary<string, string>();
            for (var i = 0; i < ids.Length; i++)
            {
                keysDict.Add($"@id{i}", ids[i]);
            }
            var queryDefinition = new QueryDefinition($"SELECT * FROM fakeentitytwo fet WHERE fet.id IN ({string.Join(", ", keysDict.Keys)})");
            foreach (var id in keysDict)
            {
                queryDefinition.WithParameter(id.Key, id.Value);
            }

            return await base.GetPagedAsync(queryDefinition).ConfigureAwait(false);
        }

        public async Task<bool> DeleteBulkAsync(CancellationToken cancellationToken = default, params FakeEntityTwo[] entities)
		{
            return await base.DeleteBulkAsync(entities.Select(x => (id: x.Id, partitionKey: x.FakeEntityTwoId )).ToList(), cancellationToken).ConfigureAwait(false);
		}
    }
}
