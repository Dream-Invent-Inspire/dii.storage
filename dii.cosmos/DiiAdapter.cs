using dii.cosmos.Models;
using dii.cosmos.Models.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.cosmos
{
    public abstract class DiiAdapter<T> : IDiiAdapter<T> where T : IDiiEntity, new()
	{
		#region Private Fields
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		#endregion Private Fields

		#region Public Fields
		public TableMetaData Table => _table;
		#endregion Public Fields

		#region Constructors
		public DiiAdapter()
		{
			_optimizer = Optimizer.Get();
			_table = _optimizer.TableMappings[typeof(T)];
		}
		#endregion Constructors

		#region Public Methods

		#region Fetch APIs
		/// <inheritdoc/>
		public abstract Task<T> GetAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null);
		#endregion Fetch APIs

		#region Create APIs
		/// <inheritdoc/>
		public abstract Task<T> CreateAsync(T diiEntity, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default);
		#endregion Create APIs

		#region Replace APIs
		/// <inheritdoc/>
		public abstract Task<T> ReplaceAsync(T diiEntity, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default);
		#endregion Replace APIs

		#region Upsert APIs
		/// <inheritdoc/>
		public abstract Task<T> UpsertAsync(T diiEntity, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default);
		#endregion Upsert APIs

		#region Delete APIs
		/// <inheritdoc/>
		public abstract Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, CancellationToken cancellationToken = default);
        #endregion Delete APIs

        #endregion Public Methods
    }
}