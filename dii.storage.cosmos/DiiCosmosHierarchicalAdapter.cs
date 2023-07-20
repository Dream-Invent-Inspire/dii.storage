using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using dii.storage.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel;
using Container = Microsoft.Azure.Cosmos.Container;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Azure;
using System.Collections;

namespace dii.storage.cosmos
{
    /// <summary>
    /// A CosmosDB abstraction of the adapter pattern with support for <see cref="Optimizer"/> and <see cref="DiiCosmosContext"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of entity the <see cref="DiiCosmosAdapter{T}"/> is to be used for.</typeparam>
    /// <remarks>
    /// <typeparamref name="T"/> must implement the <see cref="IDiiEntity"/> interface.
    /// </remarks>
    public abstract class DiiCosmosHierarchicalAdapter<T> where T : IDiiEntity, new()
    {
        #region Private Fields
        private readonly Container _container;
        private readonly Optimizer _optimizer;
        private readonly TableMetaData _table;
        private readonly DiiCosmosContext _context;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Initializes an instance of the <see cref="DiiCosmosAdapter{T}"/>.
        /// </summary>
        public DiiCosmosHierarchicalAdapter()
        {
            _context = DiiCosmosContext.Get();
            _optimizer = Optimizer.Get();
            _table = _optimizer.TableMappings[typeof(T)];
            _container = _context.Client.GetContainer(_context.Config.DatabaseId, _table.TableName);
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
        protected async Task<T> GetAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var diiEntity = default(T);

            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return diiEntity;
            }

            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();

            var dic = GetPK(partitionKeys);
            foreach (var key in dic.Keys)
            {
                partitionKeys.Add(key, dic[key]);
            }

            // Perform a point read
            ItemResponse<object> response = await _container.ReadItemAsync<object>(
                id,
                partitionKey.Build()
            );

            if (response.Resource != null)
            {
                return _optimizer.UnpackageFromJson<T>(response.Resource.ToString());
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
        protected async Task<List<T>> GetManyAsync(IReadOnlyList<Tuple<string, Dictionary<string, string>>> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default)
        {
            var diiEntities = default(List<T>);

            if (idAndPks == default)
            {
                return diiEntities;
            }

            //DJ: As of july 2023, CosmosDB support only up to 3 hierarchical partition keys.

            List<object> ids = new List<object>();
            List<object> key1 = new List<object>();
            List<object> key2 = new List<object>();
            List<object> key3 = new List<object>();
            string keycol1 = string.Empty, keycol2 = string.Empty, keycol3 = string.Empty;

            foreach (var set in idAndPks)
            {
                ids.Add(set.Item1.ToString());
                var innerdic = GetPK(set.Item2);
                for (int i=0; i < innerdic.Values.Count(); i++)
                {
                    if (i == 0)
                    {
                        key1.Add(innerdic.ElementAt(0).Value);
                        if (string.IsNullOrEmpty(keycol1)) keycol1 = innerdic.ElementAt(0).Key;
                    }
                    if (i == 1 && innerdic.Count() > 1)
                    {
                        key2.Add(innerdic.ElementAt(1).Value);
                        if (string.IsNullOrEmpty(keycol2)) keycol2 = innerdic.ElementAt(1).Key;
                    }
                    if (i == 2 && innerdic.Count() > 2)
                    {
                        key3.Add(innerdic.ElementAt(2).Value);
                        if (string.IsNullOrEmpty(keycol3)) keycol3 = innerdic.ElementAt(2).Key;
                    }
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT * FROM c WHERE ");

            if (!string.IsNullOrEmpty(keycol1))
            {
                stringBuilder.Append($"c.{keycol1} IN ({string.Join(",", key1.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }
            if (!string.IsNullOrEmpty(keycol2))
            {
                stringBuilder.Append($"c.{keycol2} IN ({string.Join(",", key2.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }
            if (!string.IsNullOrEmpty(keycol3))
            {
                stringBuilder.Append($"c.{keycol3} IN ({string.Join(",", key3.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }

            stringBuilder.Append($"c.id IN ({string.Join(",", ids.Distinct().Select(x => $"\"{x}\"").ToList())})");

            //run query
            diiEntities = new List<T>();

            // Retrieve an iterator for the result set
            using FeedIterator<object> results = _container.GetItemQueryIterator<object>(stringBuilder.ToString());

            while (results.HasMoreResults)
            {
                FeedResponse<object> resultsPage = await results.ReadNextAsync();
                foreach (object result in resultsPage)
                {
                    // Process result
                    diiEntities.Add(_optimizer.UnpackageFromJson<T>(result.ToString()));
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
            var packedEntity = _optimizer.ToEntity(diiEntity);
            //var partitionKey = _optimizer.GetPartitionKey(diiEntity);

            //var returnedEntity = await _container.CreateItemAsync(packedEntity, new PartitionKey(partitionKey), requestOptions, cancellationToken).ConfigureAwait(false);
            var returnedEntity = await _container.CreateItemAsync(packedEntity).ConfigureAwait(false);

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


        private Dictionary<string, string> GetPK(Dictionary<string, string> partitionKeys)
        {
            var ret = new Dictionary<string, string>();

            var key1 = (_table.HierarchicalPartitionKeys.ContainsKey(0)) ? this._table.HierarchicalPartitionKeys[0] : null;
            var key2 = (_table.HierarchicalPartitionKeys.ContainsKey(1)) ? this._table.HierarchicalPartitionKeys[1] : null;
            var key3 = (_table.HierarchicalPartitionKeys.ContainsKey(2)) ? this._table.HierarchicalPartitionKeys[2] : null;

            if (key1 == null)
            {
                throw new Exception("No partition key defined for this table");
            }

            var curkey = partitionKeys.Where(x => x.Key.Equals(key1, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (curkey.Value == null)
            {
                throw new Exception("No valid partition key provided for this query");
            }
            ret.Add(key1, curkey.Value);

            if (key2 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key2, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) ret.Add(key2, curkey.Value);
            }

            if (key3 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key3, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) ret.Add(key3, curkey.Value);
            }
            return ret;
        }

        public async Task<List<T>> RunConcurrentQueries(List<string> queries)
        {
            List<T> combinedResults = new List<T>();
            Queue<string> queriesQueue = new Queue<string>(queries);
            List<Task<List<T>>> tasks = new List<Task<List<T>>>();

            while (queriesQueue.Any() || tasks.Any())
            {
                while (tasks.Count < 5 && queriesQueue.Any())
                {
                    string query = queriesQueue.Dequeue();
                    tasks.Add(RunQuery(query));
                }

                Task<List<T>> completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                combinedResults.AddRange(await completedTask);
            }

            return combinedResults;
        }

        private async Task<List<T>> RunQuery(string query)
        {
            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<string> queryResultSetIterator = _container.GetItemQueryIterator<string>(queryDefinition);

            List<T> results = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<string> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                T item = _optimizer.UnpackageFromJson<T>(currentResultSet.First().ToString());
                //results.AddRange(item);
            }

            return results;
        }



    }

}
