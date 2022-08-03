using dii.storage.Models;
using dii.storage.tests.Models;
using dii.storage.tests.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.tests.Models.Enums;

namespace dii.storage.tests.Adapters
{
    public class ExampleAdapter : DiiCosmosAdapter<FakeEntityTwo>, IExampleAdapter
    {
        private Dictionary<ComparisonType, string> _comparisons = new Dictionary<ComparisonType, string>
        {
            {ComparisonType.GreaterThan, ">" },
            {ComparisonType.GreaterThanOrEqual, ">=" },
            {ComparisonType.LessThanOrEqual, "<=" },
            {ComparisonType.LessThan, "<" }
        };

        public Task<FakeEntityTwo> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default)
        {
            return base.GetAsync(id, fakeId, cancellationToken: cancellationToken);
        }

        public Task<ICollection<FakeEntityTwo>> GetManyByIdsAsync(IReadOnlyList<(string id, string fakeId)> idCollection, CancellationToken cancellationToken = default)
        {
            return base.GetManyAsync(idCollection, cancellationToken: cancellationToken);
        }

        public Task<PagedList<FakeEntityTwo>> GetByLongComparison(long value, ComparisonType comparisonType)
        {
            var query = new QueryDefinition($"SELECT * FROM fakeentitytwo fet WHERE fet.long {_comparisons[comparisonType]} @comp");
            query.WithParameter("@comp", value);

            return base.GetPagedAsync(query);
        }

        public Task<bool> DeleteAsync(string id, string fakeId, CancellationToken cancellationToken = default)
        {
            return base.DeleteAsync(id, fakeId, cancellationToken: cancellationToken);
        }

        public Task<PagedList<FakeEntityTwo>> GetByIdsAsync(params string[] ids)
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

            return base.GetPagedAsync(queryDefinition);
        }

        public Task<bool> DeleteBulkAsync(CancellationToken cancellationToken = default, params FakeEntityTwo[] entities)
        {
            return base.DeleteBulkAsync(entities.Select(x => (id: x.Id, partitionKey: x.FakeEntityTwoId)).ToList(), cancellationToken: cancellationToken);
        }

        public Task<FakeEntityTwo> CreateAsync(FakeEntityTwo diiEntity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<FakeEntityTwo>> CreateBulkAsync(IReadOnlyList<FakeEntityTwo> diiEntities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}