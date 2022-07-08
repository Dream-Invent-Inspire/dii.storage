using dii.cosmos.Models;
using dii.cosmos.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace dii.cosmos
{
    public class DiiCosmosAdapter<T> : DiiAdapter<T>, IDiiCosmosAdapter<T> where T : IDiiEntity, new()
	{
		#region Private Fields
		private readonly Container _container;
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		private readonly DiiCosmosContext _context;
		#endregion Private Fields

		#region Constructors
		public DiiCosmosAdapter()
		{
			_context = DiiCosmosContext.Get();
			_optimizer = Optimizer.Get();
			_table = _optimizer.TableMappings[typeof(T)];
			_container = _context.Client.GetContainer(_context.Config.DatabaseId, _table.TableName);
		}
		#endregion Constructors

		#region Public Methods

		#region Fetch APIs
		/// <inheritdoc/>
		public override Task<T> GetAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
		{
			return GetAsync(id, partitionKey, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<T> GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var diiEntity = default(T);

			if (string.IsNullOrWhiteSpace(id) || partitionKey == default)
			{
				return diiEntity;
			}

			var response = await _container.ReadItemStreamAsync(id, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);
			if (response.Content == null)
			{
				return diiEntity;
			}

			using (var reader = new StreamReader(response.Content))
			{
				var json = reader.ReadToEnd();
				diiEntity = _optimizer.UnpackageFromJson<T>(json);
			}

			return diiEntity;
		}

		/// <inheritdoc/>
		public override Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, CancellationToken cancellationToken = default)
		{
			return GetManyAsync(idAndPks, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default)
		{
			var diiEntities = default(ICollection<T>);

			if (idAndPks == default)
			{
				return diiEntities;
			}

			var response = await _container.ReadManyItemsStreamAsync(idAndPks.Select(x => (x.id, new PartitionKey(x.partitionKey))).ToList(), readManyRequestOptions, cancellationToken).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				return diiEntities;
			}

			diiEntities = new List<T>();

			using (var reader = new StreamReader(response.Content))
			{
				var json = reader.ReadToEnd();
				var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

				foreach (var element in wrapper.Documents)
				{
					var doc = element.ToString();
					diiEntities.Add(_optimizer.UnpackageFromJson<T>(doc));
				}
			}

			return diiEntities;
		}

		/// <inheritdoc/>
		public async Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
		{
			var iterator = _container.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);

			var results = new PagedList<T>();

			while (iterator.HasMoreResults)
			{
				var responseMessage = await iterator.ReadNextAsync().ConfigureAwait(false);

				using (var reader = new StreamReader(responseMessage.Content))
				{
					var json = reader.ReadToEnd();
					var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

					foreach (var element in wrapper.Documents)
					{
						var doc = element.ToString();
						results.Add(_optimizer.UnpackageFromJson<T>(doc));
					}
				}

				results.ContinuationToken = responseMessage.ContinuationToken;
			}

			return results;
		}

		/// <inheritdoc/>
		public override Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null)
		{
			return GetPagedAsync(queryText, continuationToken: continuationToken);
		}

		/// <inheritdoc/>
		public async Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
		{
			var iterator = _container.GetItemQueryStreamIterator(queryText, continuationToken, requestOptions);

			var results = new PagedList<T>();

			while (iterator.HasMoreResults)
			{
				var responseMessage = await iterator.ReadNextAsync().ConfigureAwait(false);

				using (var reader = new StreamReader(responseMessage.Content))
				{
					var json = reader.ReadToEnd();
					var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

					foreach (var element in wrapper.Documents)
					{
						var doc = element.ToString();
						results.Add(_optimizer.UnpackageFromJson<T>(doc));
					}
				}

				results.ContinuationToken = responseMessage.ContinuationToken;
			}

			return results;
		}
		#endregion Fetch APIs

		#region Create APIs
		/// <inheritdoc/>
		public override Task<T> CreateAsync(T diiEntity, CancellationToken cancellationToken = default)
		{
			return CreateAsync(diiEntity, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<T> CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(diiEntity);

			var returnedEntity  = await _container.CreateItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public override Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default)
		{
			return CreateBulkAsync(diiEntities, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiEntities == null || !diiEntities.Any())
            {
				return unpackedEntities;
            }

			var packedEntities = diiEntities.Select(x => new
			{
				PartitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(x),
				Entity = _optimizer.ToEntity(x)
			});

			var concurrentTasks = new List<Task<ItemResponse<object>>>();

			foreach (var packedEntity in packedEntities)
			{
				var task = _container.CreateItemAsync(packedEntity.Entity, packedEntity.PartitionKey, requestOptions, cancellationToken);
				concurrentTasks.Add(task);
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Create APIs

		#region Replace APIs
		/// <inheritdoc/>
		public override Task<T> ReplaceAsync(T diiEntity, CancellationToken cancellationToken = default)
		{
			return ReplaceAsync(diiEntity, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<T> ReplaceAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(diiEntity);
			var id = _optimizer.GetId(diiEntity);

			var returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public override Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default)
		{
			return ReplaceBulkAsync(diiEntities, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiEntities == null || !diiEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiEntities.Select(x => new
			{
				Id = _optimizer.GetId(x),
				PartitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(x),
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

				var task = _container.ReplaceItemAsync(packedEntity.Entity, packedEntity.Id, packedEntity.PartitionKey, requestOptions, cancellationToken);
				concurrentTasks.Add(task);

				if (generateRequestOptions)
				{
					requestOptions = null;
				}
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Replace APIs

		#region Upsert APIs
		/// <inheritdoc/>
		public override Task<T> UpsertAsync(T diiEntity, CancellationToken cancellationToken = default)
		{
			return UpsertAsync(diiEntity, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<T> UpsertAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiEntity);
			var partitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(diiEntity);

			var returnedEntity = await _container.UpsertItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public override Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default)
		{
			return UpsertBulkAsync(diiEntities, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiEntities == null || !diiEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiEntities.Select(x => new
			{
				PartitionKey = _optimizer.GetPartitionKey<T, PartitionKey>(x),
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

				var task = _container.UpsertItemAsync(packedEntity.Entity, packedEntity.PartitionKey, requestOptions, cancellationToken);
				concurrentTasks.Add(task);

				if (generateRequestOptions)
                {
					requestOptions = null;
				}
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Upsert APIs

		#region Patch APIs
		/// <inheritdoc/>
		public async Task<T> PatchAsync(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var returnedEntity = await _container.PatchItemAsync<object>(id, new PartitionKey(partitionKey), patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> PatchBulkAsync(IReadOnlyList<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

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

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Patch APIs

		#region Delete APIs
		/// <inheritdoc/>
		public override Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
		{
			return DeleteAsync(id, partitionKey, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<bool> DeleteAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var response = await _container.DeleteItemStreamAsync(id, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);

			return response.IsSuccessStatusCode;
		}

		/// <inheritdoc/>
		public override Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, CancellationToken cancellationToken = default)
		{
			return DeleteBulkAsync(idAndPks, cancellationToken: cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
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
    }
}