using dii.storage.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.tests.Models.Enums;

namespace dii.storage.tests.Models.Interfaces
{
    public interface IExampleAdapter
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
}