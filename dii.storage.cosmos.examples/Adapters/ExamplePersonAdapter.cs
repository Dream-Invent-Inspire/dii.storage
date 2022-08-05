using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.examples.Models.Interfaces;
using dii.storage.cosmos.Models;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.cosmos.examples.Models.Enums;

namespace dii.storage.cosmos.examples.Adapters
{
    public class ExamplePersonAdapter : DiiCosmosAdapter<Person>, IExamplePersonAdapter
    {
        public Task<Person> FetchAsync(string personId, string clientId, CancellationToken cancellationToken = default)
        {
            return base.GetAsync(personId, clientId, cancellationToken: cancellationToken);
        }

        public async Task<List<Person>> GetManyByPersonIdsAsync(IReadOnlyList<string> personIds)
        {
            var keysDict = new Dictionary<string, string>();

            for (var i = 0; i < personIds.Count; i++)
            {
                keysDict.Add($"@id{i}", personIds[i]);
            }

            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.id IN ({string.Join(", ", keysDict.Keys)})");

            foreach (var id in keysDict)
            {
                queryDefinition.WithParameter(id.Key, id.Value);
            }

            var results = await base.GetPagedAsync(queryDefinition).ConfigureAwait(false);

            return results.ToList();
        }

        public Task<PagedList<Person>> GetManyByClientIdAsync(string clientId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.PK = @clientId");

            queryDefinition.WithParameter("@clientId", clientId);

            return base.GetPagedAsync(queryDefinition);
        }

        public Task<PagedList<Person>> GetByAgeComparisonAsync(long age, ComparisonType comparisonType)
        {
            var query = new QueryDefinition($"SELECT * FROM person p WHERE p.age {_comparisons[comparisonType]} @age");
            query.WithParameter("@age", age);

            return base.GetPagedAsync(query);
        }

        public Task<PagedList<Person>> SearchByZipCodeAsync(string clientId, string zipCode)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.PK = @clientId AND p.address.zip = @zipCode");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@zipCode", zipCode);

            return base.GetPagedAsync(queryDefinition);
        }

        public Task<PagedList<Person>> SearchByAreaCodeAsync(string clientId, string areaCode)
        {
            var queryRequestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(clientId)
            };

            var queryDefinition = new QueryDefinition(@"SELECT
					*
				FROM person p
				WHERE STARTSWITH(p.address.phone.number, @areaCode, true)");

            queryDefinition.WithParameter("@areaCode", areaCode);

            return base.GetPagedAsync(queryDefinition, requestOptions: queryRequestOptions);
        }

        public Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default)
        {
            return base.CreateAsync(person, cancellationToken: cancellationToken);
        }

        public Task<Person> UpsertAsync(Person person, CancellationToken cancellationToken = default)
        {
            return base.UpsertAsync(person, cancellationToken: cancellationToken);
        }

        public async Task AddYearToAgeAsync(string personId, string clientId, CancellationToken cancellationToken = default)
        {
            var patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };

            var patchOperations = new List<PatchOperation>()
            {
                PatchOperation.Increment("/age", 1)
            };

            _ = await base.PatchAsync(personId, clientId, patchOperations, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
        }

        public Task<bool> DeleteBulkAsync(IReadOnlyList<Person> people, CancellationToken cancellationToken = default)
        {
            return base.DeleteEntitiesBulkAsync(people, cancellationToken: cancellationToken);
        }

        private readonly Dictionary<ComparisonType, string> _comparisons = new Dictionary<ComparisonType, string>
        {
            {ComparisonType.GreaterThan, ">" },
            {ComparisonType.GreaterThanOrEqual, ">=" },
            {ComparisonType.LessThanOrEqual, "<=" },
            {ComparisonType.LessThan, "<" }
        };
    }
}