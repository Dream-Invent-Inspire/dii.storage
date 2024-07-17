using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using dii.storage.Utilities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

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
        /// <summary>
        /// Determines if the entity exists in the cosmos database.
        /// It is most efficient for this purpose because it doesn't deserialize the entity.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKeys"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<bool> ItemExistsAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return false;
            }

            // Build the full partition key path
            var partitionKey = GetPKBuilder(partitionKeys);

            //Note: we just care if the entity is present so avoid deserializing the entity
            var strEntity = await ReadStreamToStringAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(strEntity);
        }

        /// <summary>
        /// Reads an entity from the service as an asynchronous operation.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="partitionKeys">The partition key for the entity.</param>
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
            var partitionKey = GetPKBuilder(partitionKeys);

            diiEntity = await ReadStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            return diiEntity;
        }

        /// <summary>
        /// Reads an entity from the service as an asynchronous operation.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="partitionKeys">The partition key for the entity.</param>
        /// <param name="requestOptions">(Optional) The options for the entity request.</param>
        /// <param name="cancellationToken">(Optional) <see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// The entity.
        /// </returns>
        /// <remarks>
        /// <see cref="ItemRequestOptions.EnableContentResponseOnWrite"/> is ignored.
        /// </remarks>
        protected virtual async Task<T> GetAsync(string id, Dictionary<string, object> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var diiEntity = default(T);

            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, object>))
            {
                return diiEntity;
            }

            // Build the full partition key path
            var partitionKey = GetPKBuilder(partitionKeys);

            diiEntity = await ReadStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            return diiEntity;
        }

        /// <summary>
        /// Fetch an entity with the provided entity id and partition key.
        /// </summary>
        /// <param name="diiEntity"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<T> GetEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var partitionKey = GetPK(diiEntity);
            var id = GetId(diiEntity);

            diiEntity = await ReadStreamAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            return diiEntity;
        }

        /// <summary>
        /// Centralized method for fetching entities from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pkBuilder"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<T> ReadStreamAsync(string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Read the same item but as a stream.
            var strEntity = await ReadStreamToStringAsync(id, pkBuilder, requestOptions, cancellationToken).ConfigureAwait(false);
            return await HydrateEntityAsync(strEntity).ConfigureAwait(false);
        }

        /// <summary>
        /// Centralized method for fetching entities from the database as serialized strings.
        /// For use with <see cref="ReadStreamAsync(string, PartitionKeyBuilder, ItemRequestOptions, CancellationToken)"/>
        /// and <see cref="ItemExistsAsync(string, Dictionary{string, string}, ItemRequestOptions, CancellationToken)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pkBuilder"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<string> ReadStreamToStringAsync(string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Read the same item but as a stream.
            using (ResponseMessage responseMessage = await _container.ReadItemStreamAsync(
                partitionKey: pkBuilder.Build(),
                id: id,
                requestOptions: requestOptions,
                cancellationToken: cancellationToken).ConfigureAwait(false)
            )
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
        protected virtual async Task<PagedList<T>> GetManyAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks, string continuationToken = null, QueryRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var diiEntities = default(PagedList<T>);

            if (idAndPks == default)
            {
                return diiEntities;
            }
            //if (idAndPks.Count > Constants.MAX_BATCH_SIZE)
            //{
            //    throw new ArgumentException($"Cannot read more than {Constants.MAX_BATCH_SIZE} entities at a time.");
            //}

            requestOptions ??= new QueryRequestOptions
            {
                MaxItemCount = _table.DefaultPageSize ?? Constants.MAX_BATCH_SIZE
            };

            //build query
            var stringBuilder = GetByIdListPrep(idAndPks);

            //run query
            diiEntities = new PagedList<T>();

            using FeedIterator results = _container.GetItemQueryStreamIterator(
                        stringBuilder.ToString(),
                        continuationToken,
                        requestOptions: requestOptions);

            diiEntities = await GetPagedInternalAsync(results, (int)requestOptions.MaxItemCount, continuationToken, cancellationToken).ConfigureAwait(false);
            return diiEntities;
        }

        private StringBuilder GetByIdListPrep(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks)
        {
            List<string> ids = new List<string>();
            List<string> key1 = new List<string>();
            List<string> key2 = new List<string>();
            List<string> key3 = new List<string>();
            string keycol1 = (_table.HierarchicalPartitionKeys.Count > 0) ? _table.HierarchicalPartitionKeys?.ElementAt(0).Value?.Name : null;
            string keycol2 = (_table.HierarchicalPartitionKeys.Count > 1) ? _table.HierarchicalPartitionKeys?.ElementAt(1).Value?.Name : null;
            string keycol3 = (_table.HierarchicalPartitionKeys.Count > 2) ? _table.HierarchicalPartitionKeys?.ElementAt(2).Value?.Name : null;

            foreach (var set in idAndPks)
            {
                ids.Add(set.Item1.ToString());
                if (!string.IsNullOrEmpty(keycol1))
                {
                    var curkey = set.Item2.Where(x => x.Key.Equals(keycol1, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(curkey.Value)) key1.Add(curkey.Value);
                }
                if (!string.IsNullOrEmpty(keycol2))
                {
                    var curkey = set.Item2.Where(x => x.Key.Equals(keycol2, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(curkey.Value)) key2.Add(curkey.Value);
                }
                if (!string.IsNullOrEmpty(keycol3))
                {
                    var curkey = set.Item2.Where(x => x.Key.Equals(keycol3, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(curkey.Value)) key3.Add(curkey.Value);
                }
            }

            return base.GetSQLWithIdsAndHpks(ids.Cast<string>().ToList(),
                (keycol1, ((!string.IsNullOrEmpty(keycol1)) ? key1 : null)),
                (keycol2, ((!string.IsNullOrEmpty(keycol2)) ? key2 : null)),
                (keycol3, ((!string.IsNullOrEmpty(keycol3)) ? key3 : null)));
        }
        private StringBuilder GetByEntityListPrep(List<T> entities)
        {
            List<string> ids = new List<string>();
            List<string> key1 = new List<string>();
            List<string> key2 = new List<string>();
            List<string> key3 = new List<string>();
            string keycol1 = (_table.HierarchicalPartitionKeys.Count > 0) ? _table.HierarchicalPartitionKeys?.ElementAt(0).Value?.Name : null;
            string keycol2 = (_table.HierarchicalPartitionKeys.Count > 1) ? _table.HierarchicalPartitionKeys?.ElementAt(1).Value?.Name : null;
            string keycol3 = (_table.HierarchicalPartitionKeys.Count > 2) ? _table.HierarchicalPartitionKeys?.ElementAt(2).Value?.Name : null;

            foreach (var entity in entities)
            {
                ids.Add(GetId(entity));
                if (!string.IsNullOrEmpty(keycol1))
                {
                    var curkey = GetPropertyValue(entity, keycol1)?.ToString();
                    if (!string.IsNullOrEmpty(curkey)) key1.Add(curkey);
                }
                if (!string.IsNullOrEmpty(keycol2))
                {
                    var curkey = GetPropertyValue(entity, keycol2)?.ToString();
                    if (!string.IsNullOrEmpty(curkey)) key2.Add(curkey);
                }
                if (!string.IsNullOrEmpty(keycol3))
                {
                    var curkey = GetPropertyValue(entity, keycol3)?.ToString();
                    if (!string.IsNullOrEmpty(curkey)) key3.Add(curkey);
                }
            }

            return base.GetSQLWithIdsAndHpks(ids.Cast<string>().ToList(),
                (keycol1, ((!string.IsNullOrEmpty(keycol1)) ? key1 : null)),
                (keycol2, ((!string.IsNullOrEmpty(keycol2)) ? key2 : null)),
                (keycol3, ((!string.IsNullOrEmpty(keycol3)) ? key3 : null)));
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
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A PagedList of entities.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        protected virtual async Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            requestOptions ??= new QueryRequestOptions
            {
                MaxItemCount = _table.DefaultPageSize ?? Constants.MAX_BATCH_SIZE
            };

            var iterator = _container.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);

            return await GetPagedInternalAsync(iterator, (int)(requestOptions.MaxItemCount ?? _table.DefaultPageSize), continuationToken, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// This method creates a query for entities in the same partition using a SQL-like statement.
        /// </summary>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="continuationToken">(Optional) The continuation token for subsequent calls.</param>
        /// <param name="requestOptions">(Optional) The options for the entity query request.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A PagedList of entities.
        /// </returns>
        /// <remarks>
        /// Only supports single partition queries.
        /// </remarks>
        protected virtual async Task<PagedList<T>> GetPagedAsync(string queryText, string continuationToken = null, QueryRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            requestOptions ??= new QueryRequestOptions
            {
                MaxItemCount = _table.DefaultPageSize ?? Constants.MAX_BATCH_SIZE
            };

            var iterator = _container.GetItemQueryStreamIterator(queryText, continuationToken, requestOptions);

            return await GetPagedInternalAsync(iterator, (int)(requestOptions.MaxItemCount ?? _table.DefaultPageSize), continuationToken, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<PagedList<JsonElement>> QueryAsync(QueryDefinition queryDefinition, CancellationToken cancellationToken = default)
        {
            var results = new PagedList<JsonElement>();

            var iterator = _container.GetItemQueryStreamIterator(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var responseMessage = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

                if (responseMessage?.Content == null) return results;

                using (var reader = new StreamReader(responseMessage.Content))
                {
                    var json = reader.ReadToEnd();
                    var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

                    foreach (var element in wrapper.Documents)
                    {
                        if (!string.IsNullOrEmpty(element.ToString()))
                        {
                            results.Add(element);
                        }
                    }
                }
            }

            return results;
        }


        private async Task<PagedList<T>> GetPagedInternalAsync(FeedIterator iterator, int pageSize, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var results = new PagedList<T>();

            while (iterator.HasMoreResults)
            {
                var responseMessage = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

                if (responseMessage?.Content == null) return results;

                bool dobreak = false;
                using (var reader = new StreamReader(responseMessage.Content))
                {
                    var json = reader.ReadToEnd();
                    var wrapper = JsonSerializer.Deserialize<StreamIteratorContentWrapper>(json);

                    foreach (var element in wrapper.Documents)
                    {
                        var doc = element.ToString();
                        if (!string.IsNullOrEmpty(doc))
                        {
                            results.Add(await HydrateEntityAsync(doc).ConfigureAwait(false));
                            if (results.Count() == pageSize)
                            {
                                dobreak = true;
                                break;
                            }
                        }
                    }
                }
                if (dobreak) break;
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
            return await HydrateEntityAsync(returnedEntity.Resource?.ToString()).ConfigureAwait(false);
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

            if (diiEntities.Count() > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to create exceeds the maximum Create batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            var packedEntities = diiEntities.Select(x => _optimizer.ToEntity(x));

            var itemResponses = await base.ProcessConcurrentlyAsync<object, ItemResponse<object>>(packedEntities,
                ((object x) => { return _container.CreateItemAsync(x, null, requestOptions, cancellationToken); }),
                cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            var ents = itemResponses.Select(async x => await HydrateEntityAsync(x.Resource?.ToString()).ConfigureAwait(false));
            var hydrateres = await Task.WhenAll(ents).ConfigureAwait(false);
            return hydrateres?.ToList();
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

            //If this type has a lookup, then we may have to fetch current version first
            //If there are any entities that are not new (DataVersion != null) AND have NO initial state (HasInitialState == false)
            // They have not been initialized but should have been because this is a Lookup container source
            if ((_table.HasLookup()) && !diiEntity.HasInitialState)
            {
                //lookup the current version
                var currentVersion = await GetEntityAsync(diiEntity, requestOptions, cancellationToken).ConfigureAwait(false);
                if (currentVersion != null)
                {
                    diiEntity.SetInitialState(_table, currentVersion);
                }
            }

            diiEntity.SetChangeTracker(_table);
            var packedEntity = _optimizer.ToEntity(diiEntity);

            // Build the full partition key path
            var keyBuilder = GetPK(diiEntity);
            var id = GetId(diiEntity);

            var returnedEntity = await _container.ReplaceItemAsync(packedEntity, id, keyBuilder.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
            if (returnResult)
            {
                return await HydrateEntityAsync(returnedEntity.Resource?.ToString()).ConfigureAwait(false);
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
            if (diiEntities.Count() > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to replace exceeds the maximum Replace batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            //if this container has a lookup, then we may have to fetch current version first
            var prevs = await GetPrevSetAsync(diiEntities, cancellationToken).ConfigureAwait(false);

            var itemResponses = await base.ProcessConcurrentlyAsync<T, ItemResponse<object>>(diiEntities,
                ((T entity) =>
                {
                    requestOptions ??= (!string.IsNullOrEmpty(entity.DataVersion)) ? new ItemRequestOptions { IfMatchEtag = entity.DataVersion } : null;

                    //If there is a previous version, set the initial state
                    //This could happen if this container has a lookup
                    if (prevs.Count() > 0 && prevs.ContainsKey(GetId(entity)))
                    {
                        //This will ensure we save any changes to Lookup container Ids or Hpks...so we don't orphan the Lookup records
                        entity.SetInitialState(_table, prevs[GetId(entity)]);
                    }
                    entity.SetChangeTracker(_table);
                    return _container.ReplaceItemAsync(_optimizer.ToEntity(entity), GetId(entity), GetPK(entity).Build(), requestOptions, cancellationToken);
                }),
                cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            var ents = itemResponses.Select(async x => await HydrateEntityAsync(x.Resource?.ToString()).ConfigureAwait(false));
            var hydraters = await Task.WhenAll(ents).ConfigureAwait(false);
            return hydraters?.ToList();
        }

        private async Task<Dictionary<string, T>> GetPrevSetAsync(IReadOnlyList<T> diiEntities, CancellationToken cancellationToken = default)
        {
            var prevs = new Dictionary<string, T>();

            //if this container has a lookup, then we may have to fetch current version first
            if (this._table.HasLookup())
            {
                //See if there are any entities that have NO initial state (HasInitialState == false)
                //Concern: They have not been initialized but should have been because this is a Lookup container source 
                var fetchList = diiEntities.Where(x => !x.HasInitialState).ToList();
                if (fetchList?.Any() ?? false)
                {
                    //lookup the current version
                    var stringBuilder = GetByEntityListPrep(fetchList.ToList());
                    using FeedIterator results = _container.GetItemQueryStreamIterator(stringBuilder.ToString(), requestOptions: new QueryRequestOptions()
                    {
                        MaxItemCount = Constants.MAX_BATCH_SIZE
                    });

                    var currentVersions = await GetPagedInternalAsync(results, Constants.MAX_BATCH_SIZE, null, cancellationToken).ConfigureAwait(false);
                    prevs = currentVersions?.ToDictionary(x => GetId(x), x => x) ?? new Dictionary<string, T>();
                }
            }
            return prevs;
        }

        protected virtual async Task<T> ModifyHierarchicalPartitionKeyValueReplaceAsync(T diiEntity, Dictionary<string, string> newPartitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (requestOptions == null && !string.IsNullOrEmpty(diiEntity.DataVersion))
            {
                requestOptions = new ItemRequestOptions { IfMatchEtag = diiEntity.DataVersion };
            }

            var oldKeyBuilder = GetPK(diiEntity); //needed for roll back on error
            var oldId = GetId(diiEntity); //needed for initial verification

            //first verify that the new partition key(s) are valid...as in, there does not exist a record with the (new) partition key(s)
            bool itemAlreadyExists = await ItemExistsAsync(oldId, newPartitionKeys, requestOptions, cancellationToken).ConfigureAwait(false);
            if (itemAlreadyExists)
            {
                throw new Exception("The new partition key(s) already exist in the database.");
            }

            // Build the full partition key path
            var partitionKey = GetPKBuilder(newPartitionKeys);

            //delete original (old) entity
            var bok = false;
            try
            {
                await DeleteEntityAsync(diiEntity, requestOptions, cancellationToken).ConfigureAwait(false);

                //Now update original object with new key(s)
                if (newPartitionKeys.Count() > 0)
                {
                    var curkey = GetPropertyValue(diiEntity, newPartitionKeys.ElementAt(0).Key);
                    if (!curkey.Equals(newPartitionKeys.ElementAt(0).Value))
                    {
                        SetPropertyValue(diiEntity, newPartitionKeys.ElementAt(0).Key, newPartitionKeys.ElementAt(0).Value);
                    }
                }
                if (newPartitionKeys.Count() > 1)
                {
                    var curkey = GetPropertyValue(diiEntity, newPartitionKeys.ElementAt(1).Key);
                    if (!curkey.Equals(newPartitionKeys.ElementAt(1).Value))
                    {
                        SetPropertyValue(diiEntity, newPartitionKeys.ElementAt(1).Key, newPartitionKeys.ElementAt(1).Value);
                    }
                }
                if (newPartitionKeys.Count() > 2)
                {
                    var curkey = GetPropertyValue(diiEntity, newPartitionKeys.ElementAt(2).Key);
                    if (!curkey.Equals(newPartitionKeys.ElementAt(2).Value))
                    {
                        SetPropertyValue(diiEntity, newPartitionKeys.ElementAt(2).Key, newPartitionKeys.ElementAt(2).Value);
                    }
                }

                //insert new entity
                diiEntity.SetChangeTracker(_table);
                var packedEntity = _optimizer.ToEntity(diiEntity);
                var returnedEntity = await _container.CreateItemAsync(packedEntity, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);
                bok = true;

                var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
                if (!returnResult)
                {
                    return default(T);
                }
                return await HydrateEntityAsync(returnedEntity.Resource?.ToString()).ConfigureAwait(false);
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
                    if (returnedEntity.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        //ToDo: log this
                    }
                }
            }
        }

        #endregion Replace APIs

        #region Upsert APIs
        /// <summary>
        /// Upserts an entity as an asynchronous operation.
        /// NOTE: This method performs a GetAsync() call to get the current version of the entity, if this container has a lookup.
        /// If entity updates are infrequent, consider using CreateAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        ///  and handle exceptions for duplicate entities (ie. perform update)
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

            //If this type has a lookup, then we may have to fetch current version first
            //If there are any entities that are not new (DataVersion != null) AND have NO initial state (HasInitialState == false)
            // They have not been initialized but should have been because this is a Lookup container source
            if ((_table.HasLookup()) && !diiEntity.HasInitialState)
            {
                //lookup the current version
                var currentVersion = await GetEntityAsync(diiEntity, requestOptions, cancellationToken).ConfigureAwait(false);
                if (currentVersion != null)
                {
                    diiEntity.SetInitialState(_table, currentVersion);
                }
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

            return await HydrateEntityAsync(returnedEntity.Resource?.ToString()).ConfigureAwait(false);
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

            if (diiEntities == null || diiEntities.Count() == 0)
            {
                return unpackedEntities;
            }
            if (diiEntities.Count() > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to upsert exceeds the maximum batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            //if this container has a lookup, then we may have to fetch current version first
            var prevs = await GetPrevSetAsync(diiEntities, cancellationToken).ConfigureAwait(false);

            var itemResponses = await base.ProcessConcurrentlyAsync<T, ItemResponse<object>>(diiEntities,
                ((T entity) =>
                {
                    var requestOptionsLocal = requestOptions ?? ((!string.IsNullOrEmpty(entity.DataVersion)) ? new ItemRequestOptions { IfMatchEtag = entity.DataVersion } : null);

                    //If there is a previous version, set the initial state
                    //This could happen if this container has a lookup
                    if (prevs.Count() > 0 && prevs.ContainsKey(GetId(entity)))
                    {
                        //This will ensure we save any changes to Lookup container Ids or Hpks...so we don't orphan the Lookup records
                        entity.SetInitialState(_table, prevs[GetId(entity)]);
                    }
                    entity.SetChangeTracker(_table);
                    var packedEntity = _optimizer.ToEntity(entity);

                    // Build the full partition key path
                    var keyBuilder = GetPK(entity);

                    return _container.UpsertItemAsync(packedEntity, keyBuilder.Build(), requestOptionsLocal, cancellationToken);
                }),
                cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            var ents = itemResponses.Select(async x => await HydrateEntityAsync(x.Resource?.ToString()).ConfigureAwait(false));
            var hydrateres = await Task.WhenAll(ents).ConfigureAwait(false);
            return hydrateres?.ToList();
        }
        #endregion Upsert APIs

        #region Patch APIs
        /// <summary>
        /// Patches an entity as an asynchronous operation.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="partitionKeys">The partition keys for the entity.</param>
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
            var partitionKey = GetPKBuilder(partitionKeys);

            return await PatchInternalAsync(id, partitionKey, patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Patches an entity as an asynchronous operation.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="partitionKeys">The partition keys for the entity.</param>
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
        protected virtual async Task<T> PatchAsync(string id, Dictionary<string, object> partitionKeys, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Build the full partition key path
            var partitionKey = GetPKBuilder(partitionKeys);

            return await PatchInternalAsync(id, partitionKey, patchOperations, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> PatchInternalAsync(string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            //Here we need to check the patch operations and determine if we need to flag an Id of Hpk change
            var hlpr = new ChangeTrackerHelper();
            var changes = hlpr.GetAnyKeyChangesSerialized(_table, patchOperations);

            var ops = patchOperations.Select(x => PatchOperation.Set(x.Key, x.Value)).ToList(); //convert from Dictionary<string, object> to List<PatchOperation>
            if (!string.IsNullOrEmpty(changes))
            {
                ops.Add(PatchOperation.Set($"/{dii.storage.Constants.ReservedChangeTrackerKey}", changes));
            }

            var returnedEntity = await _container.PatchItemAsync<object>(id, partitionKey.Build(), (IReadOnlyList<PatchOperation>)ops, requestOptions, cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
            if (!returnResult)
            {
                return default(T);
            }
            return await HydrateEntityAsync(returnedEntity.Resource?.ToString()).ConfigureAwait(false);
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
        protected virtual async Task<List<T>> PatchBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (patchOperations == null || patchOperations.Count() == 0)
            {
                return default(List<T>);
            }
            if (patchOperations.Count() > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to patch exceeds the maximum batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            var ops = patchOperations.Select<(string id, Dictionary<string, string> partitionKeys, Dictionary<string, object> listOfPatchOperations),
                                            (string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations)>(x => new(x.id, GetPKBuilder(x.partitionKeys), x.listOfPatchOperations)).ToList();

            return await PatchBulkInternalAsync(ops, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        private async Task<List<T>> PatchBulkInternalAsync(IReadOnlyList<(string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<T>);

            var itemResponses = await base.ProcessConcurrentlyAsync<(string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations), T>(patchOperations,
                (((string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations) x) =>
                { return PatchInternalAsync(x.id, x.partitionKey, x.listOfPatchOperations, requestOptions, cancellationToken); }),
                cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            return itemResponses;
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
        protected virtual async Task<bool> DeleteEntityAsync(T diiEntity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var partitionKey = GetPK(diiEntity);
            var id = GetId(diiEntity);

            if (this._table.HasLookup())
            {
                //We have to mark this entities ttl so that it passes through change feed to ensure deletion of Lookup item(s)
                var patchItemRequestOptions = new PatchItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };
                _ = await PatchInternalAsync(id, partitionKey, GetDeletePatch(Constants.DELETE_TTL), patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
                return true;
            }
            else
            {
                return await DeleteInternalAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            }
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
        protected virtual async Task<bool> DeleteEntityByIdAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (this._table.HasLookup())
            {
                //We have to mark this entities ttl so that it passes through change feed to ensure deletion of Lookup item(s)
                var patchItemRequestOptions = new PatchItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };
                _ = await PatchAsync(id, partitionKeys, GetDeletePatch(Constants.DELETE_TTL), patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
                return true;
            }
            else
            {
                // Build the full partition key path
                var partitionKey = GetPKBuilder(partitionKeys);
                return await DeleteInternalAsync(id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            }
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
        private async Task<bool> DeleteInternalAsync(string id, PartitionKeyBuilder partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
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
            if (diiEntities == null || diiEntities.Count == 0)
            {
                return true;
            }
            if (diiEntities.Count > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to delete exceeds the maximum batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            if (this._table.HasLookup())
            {
                //We have to mark this entities ttl so that it passes through change feed to ensure deletion of Lookup item(s)
                var patchItemRequestOptions = new PatchItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                var ops = diiEntities.Select<T, (string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations)>
                    ((x) => new(GetId(x), GetPK(x), GetDeletePatch(Constants.DELETE_TTL)))
                    .ToList();

                _ = await PatchBulkInternalAsync(ops, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
                return true;
            }
            else
            {
                var idAndPks = diiEntities.Select<T, (string, PartitionKeyBuilder)>(x => new
                (
                    GetId(x),
                    GetPK(x)
                )).ToList();

                return await DeleteBulkInternalAsync(idAndPks, requestOptions, cancellationToken);
            }
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
        protected virtual async Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, Dictionary<string, string> partitionKeys)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (idAndPks == null || idAndPks.Count == 0)
            {
                return true;
            }
            if (idAndPks.Count > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to delete exceeds the maximum batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            if (this._table.HasLookup())
            {
                //We have to mark this entities ttl so that it passes through change feed to ensure deletion of Lookup item(s)
                var patchItemRequestOptions = new PatchItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                var ops = idAndPks.Select<(string id, Dictionary<string, string> partitionKeys), (string id, PartitionKeyBuilder partitionKey, Dictionary<string, object> listOfPatchOperations)>
                    ((x) => new(x.id, GetPKBuilder(x.partitionKeys), GetDeletePatch(Constants.DELETE_TTL)))
                    .ToList();

                _ = await PatchBulkInternalAsync(ops, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
                return true;
            }
            else
            {
                var internalIdAndPks = idAndPks.Select<(string id, Dictionary<string, string> partitionKeys), (string, PartitionKeyBuilder)>(x => new
                (
                    x.id,
                    GetPKBuilder(x.partitionKeys)
                )).ToList();

                return await DeleteBulkInternalAsync(internalIdAndPks, requestOptions, cancellationToken).ConfigureAwait(false);
            }
        }
        private async Task<bool> DeleteBulkInternalAsync(IReadOnlyList<(string id, PartitionKeyBuilder partitionKey)> idAndPks, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var response = false;

            if (idAndPks == null || idAndPks.Count == 0)
            {
                return true;
            }
            if (idAndPks.Count > Constants.MAX_BATCH_SIZE)
            {
                throw new ArgumentException($"The number of entities to delete exceeds the maximum batch size of {Constants.MAX_BATCH_SIZE}.");
            }

            var itemResponses = await base.ProcessConcurrentlyAsync<(string id, PartitionKeyBuilder partitionKey), ResponseMessage>(idAndPks,
                (((string id, PartitionKeyBuilder partitionKey) x) => { return _container.DeleteItemStreamAsync(x.id, x.partitionKey.Build(), requestOptions, cancellationToken); }),
                cancellationToken).ConfigureAwait(false);

            response = itemResponses.All(x => x.IsSuccessStatusCode);

            return response;
        }
        #endregion Delete APIs

        #endregion Public Methods


        private PartitionKeyBuilder GetPKBuilder(Dictionary<string, string> partitionKeys)
        {
            var partitionKey = new PartitionKeyBuilder();

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
            partitionKey.Add(curkey.Value);

            if (key2 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key2.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) partitionKey.Add(curkey.Value);
            }

            if (key3 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key3.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) partitionKey.Add(curkey.Value);
            }

            return partitionKey;
        }

        private PartitionKeyBuilder GetPKBuilder(Dictionary<string, object> partitionKeys)
        {
            var partitionKey = new PartitionKeyBuilder();

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
            AddPartitionKey(partitionKey, curkey.Value);

            if (key2 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key2.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) AddPartitionKey(partitionKey, curkey.Value);
            }

            if (key3 != null)
            {
                curkey = partitionKeys.Where(x => x.Key.Equals(key3.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (curkey.Value != null) AddPartitionKey(partitionKey, curkey.Value);
            }

            return partitionKey;
        }

        private static void AddPartitionKey(PartitionKeyBuilder pkb, object value)
        {
            if (value is int intVal)
            {
                pkb.Add(intVal);
            }
            else if (value is bool boolVal)
            {
                pkb.Add(boolVal);
            }
            else if (value is string stringVal)
            {
                pkb.Add(stringVal);
            }
            else
            {
                throw new Exception($"Type {value.GetType()} not supported as a Partition Key");

            }


        }

        private PartitionKeyBuilder GetPK(T diiEntity)
        {
            object ret = new PartitionKeyBuilder();
            _table.HierarchicalPartitionKeys.TransferProperties(diiEntity, ref ret);
            return (PartitionKeyBuilder)ret;
        }

        private string GetId(T diiEntity)
        {
            object retId = string.Empty;

            _table.IdProperties.TransferProperties(diiEntity, ref retId, _table.IdSeparator);

            return retId.ToString();
        }

        private async Task<T> HydrateEntityAsync(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                T returnObj = _optimizer.UnpackageFromJson<T>(json);
                returnObj.SetInitialState(_table);
                return returnObj;
            }
            return null;
        }

        private Dictionary<string, object> GetDeletePatch(long duration)
        {
            return new Dictionary<string, object>
            {
                { $"/{dii.storage.Constants.ReservedTTLKey}", duration },
                { $"/{dii.storage.Constants.ReservedDeletedKey}", true }
            };
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

                Task<List<T>> completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
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
                FeedResponse<string> currentResultSet = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
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
