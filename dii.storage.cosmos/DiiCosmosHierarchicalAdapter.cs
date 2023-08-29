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
using System.Security.Principal;
using System.Reflection;
using dii.storage.Attributes;

namespace dii.storage.cosmos
{
    /// <summary>
    /// A CosmosDB abstraction of the adapter pattern with support for <see cref="Optimizer"/> and <see cref="DiiCosmosContext"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of entity the <see cref="DiiCosmosAdapter{T}"/> is to be used for.</typeparam>
    /// <remarks>
    /// <typeparamref name="T"/> must implement the <see cref="IDiiEntity"/> interface.
    /// </remarks>
    public abstract class DiiCosmosHierarchicalAdapter<T> : DiiCosmosBaseAdapter
        where T : DiiCosmosEntity, new()
    {
        #region protected Fields
        protected readonly Container _container;
        protected readonly Optimizer _optimizer;
        protected readonly TableMetaData _table;
        protected readonly DiiCosmosContext _context;
        #endregion protected Fields

        #region Constructors
        /// <summary>
        /// Initializes an instance of the <see cref="DiiCosmosAdapter{T}"/>.
        /// </summary>
        public DiiCosmosHierarchicalAdapter()
        {
            _context = DiiCosmosContext.Get();
            _optimizer = Optimizer.Get();
            _table = _optimizer.TableMappings[typeof(T)];
            _container = _context.Client.GetContainer(_table.DbId, _table.TableName);
        }
        #endregion Constructors

        #region Public Methods

        #region Fetch APIs

        protected virtual async Task<bool> ItemExistsAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return false;
            }

            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();

            var dic = GetPK(partitionKeys);

            if (dic.Count() > 0) partitionKey.Add(dic.ElementAt(0).Value);
            if (dic.Count() > 1) partitionKey.Add(dic.ElementAt(1).Value);
            if (dic.Count() > 2) partitionKey.Add(dic.ElementAt(2).Value);

            //Note: we just care if the entity is present so avoid deserializing the entity
            var strEntity = await ReadStreamToStringAsync(id, partitionKey, requestOptions, cancellationToken);
            return !string.IsNullOrWhiteSpace(strEntity);
        }

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
        protected virtual async Task<T> GetAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var diiEntity = default(T);

            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return diiEntity;
            }

            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();

            var dic = GetPK(partitionKeys);
            
            if (dic.Count() > 0) partitionKey.Add(dic.ElementAt(0).Value);
            if (dic.Count() > 1) partitionKey.Add(dic.ElementAt(1).Value);
            if (dic.Count() > 2) partitionKey.Add(dic.ElementAt(2).Value);

            diiEntity = await ReadStreamAsync(id, partitionKey, requestOptions, cancellationToken);
            return diiEntity;
        }

        protected async Task<T> ReadStreamAsync(string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Read the same item but as a stream.
            var strEntity = await ReadStreamToStringAsync(id, pkBuilder, requestOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(strEntity))
            {
                T returnObj = _optimizer.UnpackageFromJson<T>(strEntity);
                returnObj.SetInitialState(_table);
                return returnObj;
            }
            return null;
        }
        protected async Task<string> ReadStreamToStringAsync(string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            //Remove case sensitivity
            //var ids = id.Split($"{Constants.DefaultIdDelimitor}");
            //if (ids.Length > 0)
            //{
            //    var newlst = ids.Select(x => x.ToLower()).ToList();
            //    id = string.Join($"{Constants.DefaultIdDelimitor}", newlst);
            //}

            // Read the same item but as a stream.
            using (ResponseMessage responseMessage = await _container.ReadItemStreamAsync(
                               partitionKey: pkBuilder.Build(),
                                              id: id))
            {
                // Item stream operations do not throw exceptions for better performance
                if (responseMessage.IsSuccessStatusCode && responseMessage?.Content != null)
                {
                    using (var reader = new StreamReader(responseMessage.Content))
                    {
                        var json = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            return json;
                        }
                    }
                }
                return null;
            }
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
        protected virtual async Task<List<T>> GetManyAsync(IReadOnlyList<Tuple<string, Dictionary<string, string>>> idAndPks, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default)
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
                    //diiEntities.Add(_optimizer.UnpackageFromJson<T>(result.ToString()));
                    var returnObj = _optimizer.UnpackageFromJson<T>(result.ToString());
                    returnObj.SetInitialState(_table);
                    diiEntities.Add(returnObj);
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
        protected virtual async Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
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
                        //results.Add(_optimizer.UnpackageFromJson<T>(doc));
                        var returnObj = _optimizer.UnpackageFromJson<T>(doc);
                        returnObj.SetInitialState(_table);
                        results.Add(returnObj);
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
        protected virtual async Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
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
                        var returnObj = _optimizer.UnpackageFromJson<T>(doc);
                        returnObj.SetInitialState(_table);
                        results.Add(returnObj);
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
        protected virtual async Task<T> CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var packedEntity = _optimizer.ToEntity(diiEntity);
            var returnedEntity = await _container.CreateItemAsync(packedEntity).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return default(T);
            }

            var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);
            unpackedEntity.SetInitialState(_table);

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
        protected virtual async Task<List<T>> CreateBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<T>);

            if (diiEntities == null || !diiEntities.Any())
            {
                return unpackedEntities;
            }

            var packedEntities = diiEntities.Select(x => new
            {
                //PartitionKey = _optimizer.GetPartitionKey(x),
                Entity = _optimizer.ToEntity(x)
            });

            var concurrentTasks = new List<Task<ItemResponse<object>>>();

            foreach (var packedEntity in packedEntities)
            {
                var task = _container.CreateItemAsync(packedEntity.Entity, null, requestOptions, cancellationToken);
                concurrentTasks.Add(task);
            }

            var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            unpackedEntities = itemResponses.Select(x =>
            {
                var obj = _optimizer.FromEntity<T>(x.Resource);
                obj.SetInitialState(_table);
                return obj;
            }).ToList();

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
        protected virtual async Task<T> ReplaceAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
            {
                requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
            }

            var packedEntity = _optimizer.ToEntity(diiEntity);

            // Build the full partition key path
            var keyBuilder = GetPK(diiEntity);
            var id = _optimizer.GetId(diiEntity);

            try
            {
                var returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, keyBuilder.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

                var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
                if (returnResult)
                {
                    var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);
                    unpackedEntity.SetInitialState(_table);
                    return unpackedEntity;
                }
            }
            catch (CosmosException ex)
            {
                throw;
            }
            return default(T);
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
        protected virtual async Task<List<T>> ReplaceBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<T>);

            if (diiEntities == null || !diiEntities.Any())
            {
                return unpackedEntities;
            }

            var concurrentTasks = new List<Task<ItemResponse<object>>>();
            var generateRequestOptions = (requestOptions == null);

            foreach (var entity in diiEntities)
            {
                var packedEntity = _optimizer.ToEntity(entity);

                // Build the full partition key path
                var keyBuilder = GetPK(entity);

                //var partitionKey = _optimizer.GetPartitionKey(diiEntity);
                var id = _optimizer.GetId(entity);

                if (generateRequestOptions && !string.IsNullOrEmpty(entity.DataVersion))
                {
                    requestOptions = new ItemRequestOptions { IfMatchEtag = entity.DataVersion };
                }

                var task = _container.ReplaceItemAsync(packedEntity, id, keyBuilder.Build(), requestOptions, cancellationToken);
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

            unpackedEntities = itemResponses.Select(x =>
            {
                var obj = _optimizer.FromEntity<T>(x.Resource);
                obj.SetInitialState(_table);
                return obj;
            }).ToList();

            return unpackedEntities;
        }

        protected virtual async Task<T> ModifyHierarchicalPartitionKeyValueReplaceAsync(T diiEntity, Dictionary<string, string> newPartitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
            {
                requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
            }

            var oldKeyBuilder = GetPK(diiEntity); //needed for roll back on error
            var oldId = _optimizer.GetId(diiEntity); //needed for initial verification

            //first verify that the new partition key(s) are valid...as in, there does not exist a record with the (new) partition key(s)
            bool itemAlreadyExists = await ItemExistsAsync(oldId, newPartitionKeys, requestOptions, cancellationToken);
            if (itemAlreadyExists)
            {
                throw new Exception("The new partition key(s) already exist in the database.");
            }

            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();
            var dic = GetPK(newPartitionKeys);

            //delete original (old) entity
            var bok = false;
            try
            {
                await DeleteEntityAsync(diiEntity, requestOptions, cancellationToken).ConfigureAwait(false);

                //Now update original object with new key(s)
                if (dic.Count() > 0)
                {
                    partitionKey.Add(dic.ElementAt(0).Value);
                    var curkey = GetPropertyValue(diiEntity, dic.ElementAt(0).Key);
                    if (!curkey.Equals(dic.ElementAt(0).Value))
                    {
                        SetPropertyValue(diiEntity, dic.ElementAt(0).Key, dic.ElementAt(0).Value);
                    }
                }
                if (dic.Count() > 1)
                {
                    partitionKey.Add(dic.ElementAt(1).Value);
                    var curkey = GetPropertyValue(diiEntity, dic.ElementAt(1).Key);
                    if (!curkey.Equals(dic.ElementAt(1).Value))
                    {
                        SetPropertyValue(diiEntity, dic.ElementAt(1).Key, dic.ElementAt(1).Value);
                    }
                }
                if (dic.Count() > 2)
                {
                    partitionKey.Add(dic.ElementAt(2).Value);
                    var curkey = GetPropertyValue(diiEntity, dic.ElementAt(2).Key);
                    if (!curkey.Equals(dic.ElementAt(2).Value))
                    {
                        SetPropertyValue(diiEntity, dic.ElementAt(2).Key, dic.ElementAt(2).Value);
                    }
                }

                //insert new entity
                var packedEntity = _optimizer.ToEntity(diiEntity);
                var returnedEntity = await _container.CreateItemAsync(packedEntity, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

                var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
                if (!returnResult)
                {
                    return default(T);
                }
                bok = true;
                var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);
                unpackedEntity.SetInitialState(_table);
                return unpackedEntity;
            }
            catch (CosmosException ex)
            {
                throw;
            }
            finally
            {
                if (!bok)
                {
                    //re-insert original (old) entity
                    var packedEntity = _optimizer.ToEntity(diiEntity);
                    var returnedEntity = await _container.UpsertItemAsync(packedEntity, oldKeyBuilder.Build(), requestOptions, cancellationToken).ConfigureAwait(false);
                }
            }
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
        protected virtual async Task<T> UpsertAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
            {
                requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
            }
            diiEntity.SetChangeTracker(_table);
            var packedEntity = _optimizer.ToEntity(diiEntity);

            // Build the full partition key path
            var keyBuilder = GetPK(diiEntity);

            var returnedEntity = await _container.UpsertItemAsync(packedEntity, keyBuilder.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return default(T);
            }

            var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);
            unpackedEntity.SetInitialState(_table);
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
        protected virtual async Task<List<T>> UpsertBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<T>);

            if (diiEntities == null || !diiEntities.Any())
            {
                return unpackedEntities;
            }

            var concurrentTasks = new List<Task<ItemResponse<object>>>();
            var generateRequestOptions = (requestOptions == null);

            foreach (var entity in diiEntities)
            {
                if (generateRequestOptions && !string.IsNullOrEmpty(entity.DataVersion))
                {
                    requestOptions = new ItemRequestOptions { IfMatchEtag = entity.DataVersion };
                }
                entity.SetChangeTracker(_table);
                var packedEntity = _optimizer.ToEntity(entity);

                // Build the full partition key path
                var keyBuilder = GetPK(entity);

                var task = _container.UpsertItemAsync(packedEntity, keyBuilder.Build(), requestOptions, cancellationToken);
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

            unpackedEntities = itemResponses.Select(x =>
            {
                var obj = _optimizer.FromEntity<T>(x.Resource);
                obj.SetInitialState(_table);
                return obj;
            }).ToList();

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
        protected virtual async Task<T> PatchAsync(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();

            var dic = GetPK(partitionKeys);
            if (dic.Count() > 0) partitionKey.Add(dic.ElementAt(0).Value);
            if (dic.Count() > 1) partitionKey.Add(dic.ElementAt(1).Value);
            if (dic.Count() > 2) partitionKey.Add(dic.ElementAt(2).Value);

            var changes = new Dictionary<string, string>();
            foreach(var op in patchOperations)
            {
                var path = op.Key.Split('/')?.ElementAt(1) ?? op.Key;
                foreach(var lid in _table.LookupIds)
                {
                    var sat = lid.Value.GetCustomAttribute<SearchableAttribute>();
                    if (lid.Value.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        (sat?.Abbreviation?.Equals(path, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        changes.Add(sat?.Abbreviation ?? lid.Value.Name, op.Value.ToString());
                    }
                }
            }

            var ops = patchOperations.Select(x => PatchOperation.Set(x.Key, x.Value)).ToList(); //convert from Dictionary<string, object> to List<PatchOperation>
            ops.Add(PatchOperation.Set("/LastUpdated", DateTime.UtcNow));
            ops.Add(PatchOperation.Set("/ChangeTracker", changes));

            var returnedEntity = await _container.PatchItemAsync<object>(id, partitionKey.Build(), (IReadOnlyList<PatchOperation>)ops, requestOptions, cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
            if (!returnResult)
            {
                return default(T);
            }

            var unpackedEntity = _optimizer.FromEntity<T>(returnedEntity.Resource);
            unpackedEntity.SetInitialState(_table);
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
        protected virtual async Task<List<T>> PatchBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<T>);

            if (patchOperations == null || !patchOperations.Any())
            {
                return unpackedEntities;
            }

            var concurrentTasks = new List<Task<ItemResponse<object>>>();

            foreach (var (id, partitionKeys, listOfPatchOperations) in patchOperations)
            {
                // Build the full partition key path
                var partitionKey = new PartitionKeyBuilder();

                var dic = GetPK(partitionKeys);
                if (dic.Count() > 0) partitionKey.Add(dic.ElementAt(0).Value);
                if (dic.Count() > 1) partitionKey.Add(dic.ElementAt(1).Value);
                if (dic.Count() > 2) partitionKey.Add(dic.ElementAt(2).Value);

                var task = _container.PatchItemAsync<object>(id, partitionKey.Build(), listOfPatchOperations, requestOptions, cancellationToken);
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
        protected virtual Task<bool> DeleteEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var partitionKey = GetPK(diiEntity);
            var id = _optimizer.GetId(diiEntity);

            return DeleteAsync(id, partitionKey, requestOptions, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by id as an asynchronous operation.
        /// </summary>
        /// <param name="id">The id value for <typeparamref name="T"/> to delete.</param>
        /// <param name="partitionKeys">The dictionary of PK values for <typeparamref name="T"/> to delete.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The success status of the operation.
        /// </returns>
        /// <remarks>
        /// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
        /// </remarks>
        protected virtual Task<bool> DeleteEntityByIdAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {

            // Build the full partition key path
            var partitionKey = new PartitionKeyBuilder();

            var dic = GetPK(partitionKeys);

            if (dic.Count() > 0) partitionKey.Add(dic.ElementAt(0).Value);
            if (dic.Count() > 1) partitionKey.Add(dic.ElementAt(1).Value);
            if (dic.Count() > 2) partitionKey.Add(dic.ElementAt(2).Value);

            return DeleteAsync(id, partitionKey, requestOptions, cancellationToken);
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
        private async Task<bool> DeleteAsync(string id, PartitionKeyBuilder partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var response = await _container.DeleteItemStreamAsync(id, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

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
        protected virtual async Task<bool> DeleteEntitiesBulkAsync(IReadOnlyList<T> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var idAndPks = diiEntities.Select<T, (string, PartitionKeyBuilder)>(x => new
            (
                _optimizer.GetId(x),
                GetPK(x)
            )).ToList();

            return await DeleteBulkAsync(idAndPks, requestOptions, cancellationToken);
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
        protected virtual async Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, PartitionKeyBuilder partitionKey)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var response = false;

            if (idAndPks == null || !idAndPks.Any())
            {
                return response;
            }

            var concurrentTasks = new List<Task<ResponseMessage>>();

            foreach (var (id, partitionKey) in idAndPks)
            {
                var task = _container.DeleteItemStreamAsync(id, partitionKey.Build(), requestOptions, cancellationToken);
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

            var curkey = partitionKeys.Where(x => x.Key.Equals(key1.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (curkey.Value == null)
            {
                throw new Exception("No valid partition key provided for this query");
            }
            ret.Add(key1.Name, curkey.Value);

            if (key2 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key2.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) ret.Add(key2.Name, curkey.Value);
            }

            if (key3 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key3.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) ret.Add(key3.Name, curkey.Value);
            }
            return ret;
        }

        private PartitionKeyBuilder GetPK(T diiEntity)
        {
            var ret = new PartitionKeyBuilder();

            var key1 = (_table.HierarchicalPartitionKeys.ContainsKey(0)) ? this._table.HierarchicalPartitionKeys[0] : null;
            var key2 = (_table.HierarchicalPartitionKeys.ContainsKey(1)) ? this._table.HierarchicalPartitionKeys[1] : null;
            var key3 = (_table.HierarchicalPartitionKeys.ContainsKey(2)) ? this._table.HierarchicalPartitionKeys[2] : null;

            if (key1 == null)
            {
                throw new Exception("No partition key defined for this table");
            }

            var curkey = GetPropertyValue(diiEntity, key1.Name);
            if (curkey == null)
            {
                throw new Exception("No valid partition key provided for this query");
            }
            ret.Add(curkey.ToString());

            if (key2 != null)
            {
                curkey = GetPropertyValue(diiEntity, key2.Name);
                if (curkey != null) ret.Add(curkey.ToString());
            }

            if (key3 != null)
            {
                curkey = GetPropertyValue(diiEntity, key3.Name);
                if (curkey != null) ret.Add(curkey.ToString());
            }
            return ret;
        }


        public async Task<List<T>> RunConcurrentQueriesAsync(List<string> queries)
        {
            List<T> combinedResults = new List<T>();
            Queue<string> queriesQueue = new Queue<string>(queries);
            List<Task<List<T>>> tasks = new List<Task<List<T>>>();

            while (queriesQueue.Any() || tasks.Any())
            {
                while (tasks.Count < 5 && queriesQueue.Any())
                {
                    string query = queriesQueue.Dequeue();
                    tasks.Add(RunQueryAsync(query));
                }

                Task<List<T>> completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                combinedResults.AddRange(await completedTask);
            }

            return combinedResults;
        }

        private async Task<List<T>> RunQueryAsync(string query)
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


        private static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
        private static bool SetPropertyValue(object obj, string propertyName, object newValue)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
                throw new ArgumentException($"Property {propertyName} does not exist on type {obj.GetType().FullName}.");

            if (!propertyInfo.CanWrite)
                throw new ArgumentException($"Property {propertyName} does not have a set method.");

            propertyInfo.SetValue(obj, newValue);
            return true;
        }
    }

}
