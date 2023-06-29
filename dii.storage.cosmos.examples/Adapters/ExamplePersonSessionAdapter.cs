using dii.storage.cosmos.examples.Models.Interfaces;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.cosmos.examples.Models.Enums;

namespace dii.storage.cosmos.examples.Adapters
{
    public class ExamplePersonSessionAdapter : DiiCosmosHierarchicalAdapter<PersonSession>, IExamplePersonSessionAdapter
    {
        public Task<PersonSession> FetchAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };
            return base.GetAsync(sessionId, dic, cancellationToken: cancellationToken);
        }

        public async Task<List<PersonSession>> GetManyBySessionIdsAsync(IReadOnlyList<string> personSessionIds)
        {
            var keysDict = new Dictionary<string, string>();

            for (var i = 0; i < personSessionIds.Count; i++)
            {
                keysDict.Add($"@id{i}", personSessionIds[i]);
            }

            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.id IN ({string.Join(", ", keysDict.Keys)})");

            foreach (var id in keysDict)
            {
                queryDefinition.WithParameter(id.Key, id.Value);
            }

            var results = await base.GetPagedAsync(queryDefinition).ConfigureAwait(false);

            return results.ToList();
        }

        public Task<PagedList<PersonSession>> GetManyByClientIdAsync(string clientId, string personId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.PK = @clientId");

            queryDefinition.WithParameter("@clientId", clientId);

            return base.GetPagedAsync(queryDefinition);
        }


        public Task<PagedList<PersonSession>> SearchByRunDurationAsync(string clientId, string personId, long duration)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM person p WHERE p.ClientId = @clientId AND p.duration = @duration");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@duration", duration);

            return base.GetPagedAsync(queryDefinition);
        }



        public Task<PersonSession> CreateAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            return base.CreateAsync(session, cancellationToken: cancellationToken);
        }

        public Task<PersonSession> UpsertAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            return base.UpsertAsync(session, cancellationToken: cancellationToken);
        }

        public async Task AddEndTimeAsync(string personId, string clientId, string sessionId, DateTime ended, CancellationToken cancellationToken = default)
        {
            var patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };

            var patchOperations = new List<PatchOperation>()
            {
                //PatchOperation.Increment("/age", 1)
                PatchOperation.Set("/ended", ended)
            };

            _ = await base.PatchAsync(personId, clientId, patchOperations, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
        }

        public Task<bool> DeleteBulkAsync(IReadOnlyList<PersonSession> sessions, CancellationToken cancellationToken = default)
        {
            return base.DeleteEntitiesBulkAsync(sessions, cancellationToken: cancellationToken);
        }

    }

}
