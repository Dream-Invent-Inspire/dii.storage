﻿using dii.storage.cosmos.examples.Models.Interfaces;
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
using System.Text.Json;

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

        public async Task<List<PersonSession>> GetManyBySessionIdsAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks)
        {
            var results = await base.GetManyAsync(idAndPks, null, new QueryRequestOptions { MaxItemCount=5 }).ConfigureAwait(false);
            return results.ToList();
        }

        public async Task<PagedList<PersonSession>> GetManyByClientIdAsync(string clientId, string personId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);

            return await base.GetPagedAsync(queryDefinition);
        }

        public async Task<PagedList<PersonSession>> GetManyAsync(string clientId, string continuationToken = null)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId");

            queryDefinition.WithParameter("@clientId", clientId);

            return await base.GetPagedAsync(queryDefinition, continuationToken);
        }


        public async Task<PagedList<PersonSession>> SearchByRunDurationAsync(string clientId, string personId, long duration)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId AND c.duration = @duration");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);
            queryDefinition.WithParameter("@duration", duration);

            return await base.GetPagedAsync(queryDefinition);
        }

        public async Task<PagedList<PersonSession>> GetLastSessionsByPersonAsync(string clientId)
        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT c.PersonId, max(c.ended) as \"ended\" From c WHERE c.ClientId = @clientId group by c.PersonId");
                queryDefinition.WithParameter("@clientId", clientId);

                var res = await base.QueryAsync(queryDefinition);
                if (res?.Any() ?? false)
                {
                    var list = new List<dynamic>();
                    foreach (JsonElement element in res)
                    {
                        string personId = element.GetProperty("PersonId").GetString();
                        string ended = element.GetProperty("ended").GetString();

                        var obj = new { PersonId = personId, Ended = ended };
                        list.Add(obj);
                    }
                    if (list.Any())
                    {
                        var sb = new StringBuilder("SELECT * FROM c WHERE c.ClientId = @clientId AND (");
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.Append($"(c.PersonId = '{list[i].PersonId}' AND c.ended = '{list[i].Ended}')");
                            if (i < list.Count - 1)
                            {
                                sb.Append(" OR ");
                            }
                        }
                        sb.Append(")");

                        var queryDefinition2 = new QueryDefinition(sb.ToString());
                        queryDefinition2.WithParameter("@clientId", clientId);
                        var returnresults = await base.GetPagedAsync(queryDefinition2, null, new QueryRequestOptions { MaxItemCount = 1000 });
                        return returnresults;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return new PagedList<PersonSession>();
        }


        public async Task<PersonSession> CreateAsync(PersonSession session, CancellationToken cancellationToken = default)
        {
            if (session.SessionEndDate != null)
            {
                session.Duration = (long)(session.SessionEndDate.Value - session.SessionStartDate).TotalMilliseconds;
            }
            return await base.CreateAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<List<PersonSession>> CreateBulkAsync(List<PersonSession> sessions, CancellationToken cancellationToken = default)
        {
            sessions.ForEach(s =>
            {
                if (s.SessionEndDate != null)
                {
                    s.Duration = (long)(s.SessionEndDate.Value - s.SessionStartDate).TotalMilliseconds;
                }
            });

            return await base.CreateBulkAsync(sessions.AsReadOnly(), cancellationToken: cancellationToken);
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

        public async Task<List<PersonSession>> UpsertManyAsync(List<PersonSession> sessions, CancellationToken cancellationToken = default)
        {
            return await base.UpsertBulkAsync(sessions.AsReadOnly(), cancellationToken: cancellationToken);
        }

        public async Task<PersonSession> AddEndTimeAsync(string personId, string clientId, string sessionId, DateTime started, DateTime ended, CancellationToken cancellationToken = default)
        {
            var patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = true
            };

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

        public Task<List<PersonSession>> ReplaceBulkAsync(IReadOnlyList<PersonSession> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return base.ReplaceBulkAsync(diiEntities, requestOptions, cancellationToken);
        }

        public async Task<bool> DeleteBulkAsync(IReadOnlyList<PersonSession> sessions, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntitiesBulkAsync(sessions, cancellationToken: cancellationToken);
        }

        public async Task<bool> PatchBulkAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks, CancellationToken cancellationToken = default)
        {
            var ops = idAndPks.Select(x => (x.Item1, x.Item2, new Dictionary<string, object> { { "/duration", 1000 } })).ToList();
            var results = await base.PatchBulkAsync(ops, cancellationToken: cancellationToken).ConfigureAwait(false);
            return results?.Any() ?? false;
        }
    }

}
