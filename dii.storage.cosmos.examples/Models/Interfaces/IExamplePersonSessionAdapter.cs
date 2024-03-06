using dii.storage.cosmos.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.examples.Models.Interfaces
{
    public interface IExamplePersonSessionAdapter
    {
        Task<PersonSession> FetchAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default);
        Task<List<PersonSession>> GetManyBySessionIdsAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks);

        Task<PagedList<PersonSession>> GetManyByClientIdAsync(string clientId, string personId);

        Task<PagedList<PersonSession>> SearchByRunDurationAsync(string clientId, string personId, long duration);

        Task<PersonSession> CreateAsync(PersonSession person, CancellationToken cancellationToken = default);

        Task<PersonSession> ReplaceAsync(PersonSession person, CancellationToken cancellationToken = default);

        Task<PersonSession> ChangePersonIdAsync(PersonSession diiOldEntity, string newPersonId, CancellationToken cancellationToken = default);

        Task<PersonSession> UpsertAsync(PersonSession session, CancellationToken cancellationToken = default);

        Task<PersonSession> AddEndTimeAsync(string personId, string clientId, string sessionId, DateTime started, DateTime ended, CancellationToken cancellationToken = default);

        Task<bool> DeleteEntityAsync(PersonSession session, CancellationToken cancellationToken = default);

        Task<bool> DeleteEntityByIdAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default);

        Task<bool> DeleteBulkAsync(IReadOnlyList<PersonSession> sessions, CancellationToken cancellationToken = default);

    }
}
