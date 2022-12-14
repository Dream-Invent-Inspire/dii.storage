using dii.storage.cosmos.Exceptions;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
	/// <summary>
	/// A CosmosDB abstraction of the adapter pattern with support for <see cref="Optimizer"/> and <see cref="DiiCosmosContext"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> of entity the <see cref="DiiCosmosAdapter{T}"/> is to be used for.</typeparam>
	/// <remarks>
	/// <typeparamref name="T"/> must implement the <see cref="IDiiEntity"/> interface.
	/// </remarks>
	public abstract class DiiCosmosAdapter<T> : DiiCosmosReadOnlyAdapter<T> where T : IDiiEntity, new()
	{
		#region Private Fields
		private readonly Container _container;
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		private readonly DiiCosmosContext _context;
		private readonly bool _isReadOnlyAdapter;
		#endregion Private Fields

		#region Constructors
		/// <summary>
		/// Initializes an instance of the <see cref="DiiCosmosAdapter{T}"/>.
		/// </summary>
		public DiiCosmosAdapter(string databaseId)
			: base (databaseId)
		{
			_context = DiiCosmosContext.Get();

            if (_context.Config?.DatabaseConfig?.DatabaseIds == null || !_context.Config.DatabaseConfig.DatabaseIds.Contains(databaseId))
			{
				throw new DiiDatabaseIdNotInConfigurationException();
			}

            _optimizer = Optimizer.Get();
			_table = _optimizer.TableMappings[typeof(T)];

			if (_context.Config.ReadOnlyDatabaseConfig != null
                && _context.Config.ReadOnlyDatabaseConfig.DatabaseIds != null
                && _context.Config.ReadOnlyDatabaseConfig.DatabaseIds.Contains(databaseId))
            {
                _container = _context.ReadOnlyClient.GetContainer(databaseId, _table.TableName);
                _isReadOnlyAdapter = true;
            }
			else
            {
                _container = _context.ReadWriteClient.GetContainer(databaseId, _table.TableName);
            }
		}
		#endregion Constructors

		#region Public Methods

        #region Create APIs
        /// <summary>
        /// Creates an entity as an asynchronous operation.
        /// </summary>
        /// <param name="diiEntity">The <typeparamref name="T"/> to create.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The entity that was created.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        protected async Task<T> CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			ThrowReadOnlyException();

            var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey(diiEntity);

			var returnedEntity  = await _container.CreateItemAsync(packedEntity, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
            {
				return default(T);
            }

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <summary>
		/// Creates multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="diiEntities">The list of <see cref="IReadOnlyList{T}"/> to create.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entities that were created.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// </remarks>
		protected async Task<List<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var unpackedEntities = default(List<T>);

			if (diiEntities == null || !diiEntities.Any())
            {
				return unpackedEntities;
            }

			var packedEntities = diiEntities.Select(x => new
			{
				PartitionKey = _optimizer.GetPartitionKey(x),
				Entity = _optimizer.ToEntity(x)
			});

			var concurrentTasks = new List<Task<ItemResponse<object>>>();

			foreach (var packedEntity in packedEntities)
			{
				var task = _container.CreateItemAsync(packedEntity.Entity, new PartitionKey(packedEntity.PartitionKey), requestOptions, cancellationToken);
				concurrentTasks.Add(task);
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return unpackedEntities;
			}

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
        #endregion Create APIs

        #region Replace APIs
        /// <summary>
        /// Replaces an entity as an asynchronous operation.
        /// </summary>
        /// <param name="diiEntity">The <typeparamref name="T"/> to replace.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The entity that was updated.
        /// </returns>
        /// <remarks>
        /// The entity's partition key value is immutable. To change an entity's partition key
        /// value you must delete the original entity and insert a new entity.
        /// <para>
        /// This operation does not work on entities that use the same value for both the id and parition key.
        /// </para>
        /// </remarks>
        protected async Task<T> ReplaceAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey(diiEntity);
			var id = _optimizer.GetId(diiEntity);

			var returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return default(T);
			}

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <summary>
		/// Replaces multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="diiEntities">The list of <see cref="IReadOnlyList{T}"/> to replace.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entities that were updated.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// <para>
		/// The entity's partition key value is immutable. To change an entity's partition key
		/// value you must delete the original entity and insert a new entity.
		/// </para>
		/// <para>
		/// This operation does not work on entities that use the same value for both the id and parition key.
		/// </para>
		/// </remarks>
		protected async Task<List<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var unpackedEntities = default(List<T>);

			if (diiEntities == null || !diiEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiEntities.Select(x => new
			{
				Id = _optimizer.GetId(x),
				PartitionKey = _optimizer.GetPartitionKey(x),
				Entity = _optimizer.ToEntity(x),
				UnpackedEntity = x
			});

			var concurrentTasks = new List<Task<ItemResponse<object>>>();
			var generateRequestOptions = (requestOptions == null);

			foreach (var packedEntity in packedEntities)
			{
				if (generateRequestOptions && !string.IsNullOrEmpty(packedEntity.UnpackedEntity.DataVersion))
				{
					requestOptions = new ItemRequestOptions { IfMatchEtag = packedEntity.UnpackedEntity.DataVersion };
				}

				var task = _container.ReplaceItemAsync(packedEntity.Entity, packedEntity.Id, new PartitionKey(packedEntity.PartitionKey), requestOptions, cancellationToken);
				concurrentTasks.Add(task);

				if (generateRequestOptions)
				{
					requestOptions = null;
				}
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return unpackedEntities;
			}

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
        #endregion Replace APIs

        #region Upsert APIs
        /// <summary>
        /// Upserts an entity as an asynchronous operation.
        /// </summary>
        /// <param name="diiEntity">The <typeparamref name="T"/> to upsert.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The entity that was upserted.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        protected async Task<T> UpsertAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey(diiEntity);

			var returnedEntity = await _container.UpsertItemAsync(packedEntity, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return default(T);
			}

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <summary>
		/// Upserts multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="diiEntities">The list of <see cref="IReadOnlyList{T}"/> to upsert.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entities that were upserted.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// </remarks>
		protected async Task<List<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var unpackedEntities = default(List<T>);

			if (diiEntities == null || !diiEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiEntities.Select(x => new
			{
				PartitionKey = _optimizer.GetPartitionKey(x),
				Entity = _optimizer.ToEntity(x),
				UnpackedEntity = x
			});

			var concurrentTasks = new List<Task<ItemResponse<object>>>();
			var generateRequestOptions = (requestOptions == null);

			foreach (var packedEntity in packedEntities)
			{
				if (generateRequestOptions && !string.IsNullOrEmpty(packedEntity.UnpackedEntity.DataVersion))
				{
					requestOptions = new ItemRequestOptions { IfMatchEtag = packedEntity.UnpackedEntity.DataVersion };
				}

				var task = _container.UpsertItemAsync(packedEntity.Entity, new PartitionKey(packedEntity.PartitionKey), requestOptions, cancellationToken);
				concurrentTasks.Add(task);

				if (generateRequestOptions)
                {
					requestOptions = null;
				}
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return unpackedEntities;
			}

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Upsert APIs

		#region Patch APIs
		/// <summary>
		/// Patches an entity as an asynchronous operation.
		/// </summary>
		/// <param name="id">The entity id.</param>
		/// <param name="partitionKey">The partition key for the entity.</param>
		/// <param name="patchOperations">Represents a list of operations to be sequentially applied to the referred entity.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entity that was updated.
		/// </returns>
		/// <remarks>
		/// The entity's partition key value is immutable. To change an entity's partition key
		/// value you must delete the original entity and insert a new entity. The patch operations
		/// are atomic and are executed sequentially. By default, resource body will be returned
		/// as part of the response. User can request no content by setting Microsoft.Azure.Cosmos.ItemRequestOptions.EnableContentResponseOnWrite
		/// flag to false.
		/// </remarks>
		protected async Task<T> PatchAsync(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var returnedEntity = await _container.PatchItemAsync<object>(id, new PartitionKey(partitionKey), patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return default(T);
			}

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <summary>
		/// Patches multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="patchOperations">List of ids, partition keys and a list of operations to be sequentially applied to the referred entities.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entities that were updated.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// <para>
		/// The entity's partition key value is immutable. To change an entity's partition key
		/// value you must delete the original entity and insert a new entity. The patch operations
		/// are atomic and are executed sequentially. By default, resource body will be returned
		/// as part of the response. User can request no content by setting Microsoft.Azure.Cosmos.ItemRequestOptions.EnableContentResponseOnWrite
		/// flag to false.
		/// </para>
		/// </remarks>
		protected async Task<List<T>> PatchBulkAsync(IReadOnlyList<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var unpackedEntities = default(List<T>);

			if (patchOperations == null || !patchOperations.Any())
			{
				return unpackedEntities;
			}

			var concurrentTasks = new List<Task<ItemResponse<object>>>();

			foreach (var (id, partitionKey, listOfPatchOperations) in patchOperations)
			{
				var task = _container.PatchItemAsync<object>(id, new PartitionKey(partitionKey), listOfPatchOperations, requestOptions, cancellationToken);
				concurrentTasks.Add(task);
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

			if (!returnResult)
			{
				return unpackedEntities;
			}

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
        #endregion Patch APIs

        #region Delete APIs
        /// <summary>
        /// Delete an entity as an asynchronous operation.
        /// </summary>
        /// <param name="diiEntity">The <typeparamref name="T"/> to delete.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The success status of the operation.
        /// </returns>
        /// <remarks>
        /// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
        /// </remarks>
        protected Task<bool> DeleteEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var partitionKey = _optimizer.GetPartitionKey(diiEntity);
			var id = _optimizer.GetId(diiEntity);

			return DeleteAsync(id, partitionKey.ToString(), requestOptions, cancellationToken);
		}

		/// <summary>
		/// Delete an entity as an asynchronous operation.
		/// </summary>
		/// <param name="id">The entity id.</param>
		/// <param name="partitionKey">The partition key for the entity.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The success status of the operation.
		/// </returns>
		/// <remarks>
		/// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
		/// </remarks>
		protected async Task<bool> DeleteAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var response = await _container.DeleteItemStreamAsync(id, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// Delete multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="diiEntities">The list of <see cref="IReadOnlyList{T}"/> to upsert.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The success status of the operation.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
		/// </remarks>
		protected Task<bool> DeleteEntitiesBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var idAndPks = diiEntities.Select<T, (string, string)>(x => new
			(
				_optimizer.GetId(x),
				_optimizer.GetPartitionKey(x)
			)).ToList();

			return DeleteBulkAsync(idAndPks, requestOptions, cancellationToken);
		}

		/// <summary>
		/// Delete multiple entities as an asynchronous operation.
		/// </summary>
		/// <param name="idAndPks">List of ids and partition keys.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The success status of the operation.
		/// </returns>
		/// <remarks>
		/// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
		/// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
		/// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
		/// </remarks>
		protected async Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            ThrowReadOnlyException();

            var response = false;

			if (idAndPks == null || !idAndPks.Any())
			{
				return response;
			}

			var concurrentTasks = new List<Task<ResponseMessage>>();

			foreach (var (id, partitionKey) in idAndPks)
			{
				var task = _container.DeleteItemStreamAsync(id, new PartitionKey(partitionKey), requestOptions, cancellationToken);
				concurrentTasks.Add(task);
			}

			var responseMessages = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			response = responseMessages.All(x => x.IsSuccessStatusCode);

			return response;
		}
        #endregion Delete APIs

        #endregion Public Methods

		private void ThrowReadOnlyException([CallerMemberName] string memberName = null)
		{
            if (_isReadOnlyAdapter)
            {
                throw new DiiReadOnlyConnectionException(memberName);
            }
        }
    }
}