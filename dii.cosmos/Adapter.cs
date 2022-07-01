using dii.cosmos.Models;
using dii.cosmos.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace dii.cosmos
{
    public class Adapter<T> : IDiiCosmosAdapter<T> where T : IDiiCosmosEntity, new()
	{
		private readonly Container _container;
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		private readonly Context _context;
		public TableMetaData Table => _table;

		public Adapter()
		{
			_context = Context.Get();
			_optimizer = Optimizer.Get();
			_table = _context.TableMappings[typeof(T)];
			_container = _context.Client.GetContainer(_context.Config.DatabaseId, _table.TableName);
		}

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

		/// <inheritdoc/>
		public async Task<T> CreateAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiCosmosEntity.Version))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiCosmosEntity.Version };
			}

			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);
			ItemResponse<object> returnedEntity;

			returnedEntity = await _container.CreateItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<T> UpsertAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiCosmosEntity.Version))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiCosmosEntity.Version };
			}

			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);
			ItemResponse<object> returnedEntity;

			returnedEntity = await _container.UpsertItemAsync(packedEntity, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<T> PatchAsync(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			ItemResponse<object> returnedEntity;

			returnedEntity = await _container.PatchItemAsync<object>(id, partitionKey, patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<T> ReplaceAsync(T diiCosmosEntity, string id, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
		{
			if (requestOptions == null && !string.IsNullOrEmpty(diiCosmosEntity.Version))
			{
				requestOptions = new ItemRequestOptions { IfMatchEtag = diiCosmosEntity.Version };
			}

			var packedEntity = _optimizer.ToEntity(diiCosmosEntity);
			ItemResponse<object> returnedEntity;

			returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);

			return unpackedEntity;
		}

		/// <inheritdoc/>
		public async Task<bool> DeleteAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
			var response = await _container.DeleteItemStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);

			return response.IsSuccessStatusCode;
		}
    }
}