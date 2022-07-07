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
    public class Adapter<T> : IDiiCosmosAdapter<T> where T : IDiiCosmosEntity, new()
	{
		#region Private Fields
		private readonly Container _container;
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		private readonly Context _context;
		#endregion Private Fields

		#region Public Fields
		public TableMetaData Table => _table;
		#endregion Public Fields

		#region Constructors
		public Adapter()
		{
			_context = Context.Get();
			_optimizer = Optimizer.Get();
			_table = _context.TableMappings[typeof(T)];
			_container = _context.Client.GetContainer(_context.Config.DatabaseId, _table.TableName);
		}
		#endregion Constructors

		#region Public Methods

		#region Fetch APIs
		/// <inheritdoc/>
		public async Task<T> GetAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var diiCosmosEntity = default(T);

			if (string.IsNullOrWhiteSpace(id) || partitionKey == default)
			{
				return diiCosmosEntity;
			}

			var response = await _container.ReadItemStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
			if (response.Content == null)
			{
				return diiCosmosEntity;
			}

			using (var reader = new StreamReader(response.Content))
			{
				var json = reader.ReadToEnd();
				diiCosmosEntity = _optimizer.UnpackageFromJson<T>(json);
			}

			return diiCosmosEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default)
		{
			var diiCosmosEntities = default(ICollection<T>);

			if (items == default)
			{
				return diiCosmosEntities;
			}

			var response = await _container.ReadManyItemsStreamAsync(items, readManyRequestOptions, cancellationToken).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				return diiCosmosEntities;
			}

			diiCosmosEntities = new List<T>();

			using (var reader = new StreamReader(response.Content))
			{
				var json = reader.ReadToEnd();
				var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

				foreach (var element in wrapper.Documents)
				{
					var doc = element.ToString();
					diiCosmosEntities.Add(_optimizer.UnpackageFromJson<T>(doc));
				}
			}

			return diiCosmosEntities;
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
		public async Task<T> CreateAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);

			var returnedEntity  = await _container.CreateItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<(PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiCosmosEntities == null || !diiCosmosEntities.Any())
            {
				return unpackedEntities;
            }

			var packedEntities = diiCosmosEntities.Select(x => new
			{
				PartitionKey = x.partitionKey,
				Entity = _optimizer.ToEntity(x.diiCosmosEntity)
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
		public async Task<T> ReplaceAsync(T diiCosmosEntity, string id, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiCosmosEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiCosmosEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);

			var returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiCosmosEntities == null || !diiCosmosEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiCosmosEntities.Select(x => new
			{
				Id = x.id,
				PartitionKey = x.partitionKey,
				Entity = _optimizer.ToEntity(x.diiCosmosEntity),
				UnpackedEntity = x.diiCosmosEntity
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
		public async Task<T> UpsertAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiCosmosEntity.DataVersion))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiCosmosEntity.DataVersion };
			}

			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);

			var returnedEntity = await _container.UpsertItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<(PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (diiCosmosEntities == null || !diiCosmosEntities.Any())
			{
				return unpackedEntities;
			}

			var packedEntities = diiCosmosEntities.Select(x => new
			{
				PartitionKey = x.partitionKey,
				Entity = _optimizer.ToEntity(x.diiCosmosEntity),
				UnpackedEntity = x.diiCosmosEntity
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
		public async Task<T> PatchAsync(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var returnedEntity = await _container.PatchItemAsync<object>(id, partitionKey, patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<ICollection<T>> PatchBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var unpackedEntities = default(ICollection<T>);

			if (patchOperations == null || !patchOperations.Any())
			{
				return unpackedEntities;
			}

			var concurrentTasks = new List<Task<ItemResponse<object>>>();

			foreach (var patchOperation in patchOperations)
			{
				var task = _container.PatchItemAsync<object>(patchOperation.id, patchOperation.partitionKey, patchOperation.listOfPatchOperations, requestOptions, cancellationToken);
				concurrentTasks.Add(task);
			}

			var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

			unpackedEntities = itemResponses.Select(x => _optimizer.FromEntity<T>(x.Resource)).ToList();

			return unpackedEntities;
		}
		#endregion Patch APIs

		#region Delete APIs
		/// <inheritdoc/>
		public async Task<bool> DeleteAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var response = await _container.DeleteItemStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			return response.IsSuccessStatusCode;
		}

		/// <inheritdoc/>
		public async Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> ids, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			var response = false;

			if (ids == null || !ids.Any())
			{
				return response;
			}

			var concurrentTasks = new List<Task<ResponseMessage>>();

			foreach (var id in ids)
			{
				var task = _container.DeleteItemStreamAsync(id.id, id.partitionKey, requestOptions, cancellationToken);
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