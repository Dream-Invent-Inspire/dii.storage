using dii.storage.cosmos.Exceptions;
using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
	/// <summary>
	/// A CosmosDB abstraction of the adapter pattern with support for <see cref="Optimizer"/> and <see cref="DiiCosmosContext"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> of entity the <see cref="DiiCosmosReadOnlyAdapter{T}"/> is to be used for.</typeparam>
	/// <remarks>
	/// <typeparamref name="T"/> must implement the <see cref="IDiiEntity"/> interface.
	/// </remarks>
	public abstract class DiiCosmosReadOnlyAdapter<T> where T : IDiiEntity, new()
	{
		#region Private Fields
		private readonly Container _container;
		private readonly Optimizer _optimizer;
		private readonly TableMetaData _table;
		private readonly DiiCosmosContext _context;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Initializes an instance of the <see cref="DiiCosmosReadOnlyAdapter{T}"/>.
        /// </summary>
        public DiiCosmosReadOnlyAdapter(string databaseId)
		{
			if (string.IsNullOrWhiteSpace(databaseId))
			{
				throw new ArgumentNullException(nameof(databaseId), "databaseId is required to initialize the adapter.");
            }

            _context = DiiCosmosContext.Get();

            if ((_context.Config?.DatabaseConfig?.DatabaseIds == null || !_context.Config.DatabaseConfig.DatabaseIds.Contains(databaseId))
                && (_context.Config?.ReadOnlyDatabaseConfig?.DatabaseIds == null || !_context.Config.ReadOnlyDatabaseConfig.DatabaseIds.Contains(databaseId)))
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
            }
            else
            {
                _container = _context.ReadWriteClient.GetContainer(databaseId, _table.TableName);
            }
        }
		#endregion Constructors

		#region Public Methods

		#region Fetch APIs
		/// <summary>
		/// Reads an entity from the service as an asynchronous operation.
		/// </summary>
		/// <param name="id">The entity id.</param>
		/// <param name="partitionKey">The partition key for the entity.</param>
		/// <param name="requestOptions">(Optional) The options for the entity request.</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// The entity.
		/// </returns>
		/// <remarks>
		/// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
		/// </remarks>
		protected async Task<T> GetAsync(string id, string partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
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

		/// <summary>
		/// Reads multiple entities from a container using Id and PartitionKey values.
		/// </summary>
		/// <param name="idAndPks">List of ids and partition keys.</param>
		/// <param name="readManyRequestOptions">Request Options for ReadMany Operation</param>
		/// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
		/// <returns>
		/// A collection of entities.
		/// </returns>
		/// <remarks>
		/// This is meant to perform better latency-wise than a query with IN statements to fetch
		/// a large number of independent entities.
		/// </remarks>
		protected async Task<List<T>> GetManyAsync(IReadOnlyList<(string id, string partitionKey)> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default)
		{
			var diiEntities = default(List<T>);

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

		/// <summary>
		/// This method creates a query for entities under a container in an Azure Cosmos database
		/// using a SQL statement with parameterized values. For more information on preparing
		/// SQL statements with parameterized values, please see
		/// Microsoft.Azure.Cosmos.QueryDefinition.
		/// </summary>
		/// <param name="queryDefinition">The Cosmos SQL query definition.</param>
		/// <param name="continuationToken">(Optional) The continuation token for subsequent calls.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <returns>
		/// A PagedList of entities.
		/// </returns>
		/// <remarks>
		/// 
		/// </remarks>
		protected async Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
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

		/// <summary>
		/// This method creates a query for entities in the same partition using a SQL-like statement.
		/// </summary>
		/// <param name="queryText">The SQL query text.</param>
		/// <param name="continuationToken">(Optional) The continuation token for subsequent calls.</param>
		/// <param name="requestOptions">(Optional) The options for the entity query request.</param>
		/// <returns>
		/// A PagedList of entities.
		/// </returns>
		/// <remarks>
		/// Only supports single partition queries.
		/// </remarks>
		protected async Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
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

        #endregion Public Methods
    }
}