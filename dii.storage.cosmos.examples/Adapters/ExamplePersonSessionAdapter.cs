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
        public async Task<PersonSession> FetchAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };
            return await base.GetAsync(sessionId, dic, cancellationToken: cancellationToken);
        }

        public async Task<List<PersonSession>> GetManyBySessionIdsAsync(IReadOnlyList<Tuple<string, Dictionary<string, string>>> idAndPks)
        {
            var results = await base.GetManyAsync(idAndPks).ConfigureAwait(false);
            return results.ToList();
        }

        public async Task<PagedList<PersonSession>> GetManyByClientIdAsync(string clientId, string personId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);

            return await base.GetPagedAsync(queryDefinition);
        }


        public async Task<PagedList<PersonSession>> SearchByRunDurationAsync(string clientId, string personId, long duration)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId AND c.duration = @duration");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);
            queryDefinition.WithParameter("@duration", duration);

            return await base.GetPagedAsync(queryDefinition);
        }


        public async Task<PersonSession> CreateAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            if (session.SessionEndDate != null)
            {
                session.Duration = (long)(session.SessionEndDate.Value - session.SessionStartDate).TotalMilliseconds;
            }
            return await base.CreateAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonSession> ReplaceAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            return await base.ReplaceAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonSession> ChangePersonIdAsync(PersonSession diiOldEntity, string newPersonId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", diiOldEntity.ClientId },
                { "PersonId", newPersonId }
            };
            return await base.ModifyHierarchicalPartitionKeyValueReplaceAsync(diiOldEntity, dic, cancellationToken: cancellationToken);
        }

        public async Task<PersonSession> UpsertAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            return await base.UpsertAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonSession> AddEndTimeAsync(string personId, string clientId, string sessionId, DateTime started, DateTime ended, CancellationToken cancellationToken = default)
        {
            var patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = true
            };

            //var patchOperations = new List<PatchOperation>()
            //{
            //    PatchOperation.Set("/ended", ended),
            //    PatchOperation.Set("/duration", (long)(ended - started).TotalMilliseconds)
            //};

            var patchOperations = new Dictionary<string, object>()
            {
                { "/ended", ended },
                { "/duration", (long)(ended - started).TotalMilliseconds }
            };

            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };

            var session = await base.PatchAsync(sessionId, dic, patchOperations, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
            return session;
        }

        public async Task<bool> DeleteEntityAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntityAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteEntityByIdAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };

            return await base.DeleteEntityByIdAsync(sessionId, dic, cancellationToken: cancellationToken);
        }
        
        public async Task<bool> DeleteBulkAsync(IReadOnlyList<PersonSession> sessions, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntitiesBulkAsync(sessions, cancellationToken: cancellationToken);
        }

    }

}
