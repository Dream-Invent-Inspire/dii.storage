using dii.storage.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.cosmos.examples.Models.Enums;

namespace dii.storage.cosmos.examples.Models.Interfaces
{
    public interface IExamplePersonAdapter
    {
        Task<Person> FetchAsync(string personId, string clientId, CancellationToken cancellationToken = default);
        Task<List<Person>> GetManyByPersonIdsAsync(IReadOnlyList<string> personIds);
        Task<PagedList<Person>> GetManyByClientIdAsync(string clientId);
        Task<PagedList<Person>> GetByAgeComparisonAsync(long age, ComparisonType comparisonType);
        Task<PagedList<Person>> SearchByZipCodeAsync(string clientId, string zipCode);
        Task<PagedList<Person>> SearchByAreaCodeAsync(string clientId, string areaCode);

        Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);

        Task<Person> UpsertAsync(Person person, CancellationToken cancellationToken = default);
        Task AddYearToAgeAsync(string personId, string clientId, CancellationToken cancellationToken = default);

        Task<bool> DeleteBulkAsync(IReadOnlyList<Person> people, CancellationToken cancellationToken = default);
    }
}