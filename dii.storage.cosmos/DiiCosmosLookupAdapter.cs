using Azure;
using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
    /// <summary>
    /// Utility adapter for supporting the (DII) Lookup pattern in Cosmos
    /// Given an alternate access pattern Hpk and Id set, or query,
    ///  this adapter will return the corresponding source item(s)
    /// </summary>
    public class DiiCosmosLookupAdapter : DiiCosmosBaseAdapter
    {
        protected readonly Container _sourceContainer;
        //protected readonly Container _lookupContainer;
        protected readonly Optimizer _optimizer;
        protected readonly TableMetaData _tableMetaData;
        protected readonly DiiCosmosContext _context;

        public DiiCosmosLookupAdapter(TableMetaData tableMetaData)
        {
            _context = DiiCosmosContext.Get();
            _optimizer = Optimizer.Get();
            _tableMetaData = tableMetaData; // _optimizer.TableMappings[typeof(T)];
            _sourceContainer = _context.Client.GetContainer(_tableMetaData.DbId, _tableMetaData.TableName);
            //_lookupContainer = tableMetaData.LookupContainer ?? _context.Client.GetContainer(_tableMetaData.DbId, _tableMetaData.LookupType.ToString());
        }

        /// <summary>
        /// This method will return the source object(s) for the provided id and partition keys 
        ///  that correspond to the alternate access pattern (aka Lookup) for the source object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKeys"></param>
        /// <param name="group">Group (of source properties) this access pattern needs</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<object> LookupAsync(string id, Dictionary<string, string> partitionKeys, string group = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return null;
            }

            if (_tableMetaData.LookupTables == null)
            {
                throw new Exception("No Lookup dynamic type has been associated with the provided TableMetaData configuration.");
            }

            var ltmd = GetLookupMetaData(group);
            if (ltmd == null)
            {
                throw new Exception("Unable to determine Lookup table type and container.");
            }

            // Build the full partition key path
            var lhpks = _tableMetaData.GetHPKs(group);
            var partitionKey = GetPK(lhpks, partitionKeys);
            
            //Read from the lookup table
            var lookupResponse = await ReadStreamAsync(ltmd.LookupContainer, ltmd.LookupType, id, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            if (lookupResponse != null)
            {
                //Build the source object partition key
                object pkBuilder = new PartitionKeyBuilder();
                _tableMetaData.HierarchicalPartitionKeys.TransferProperties(lookupResponse, ref pkBuilder);

                //Build the source object id
                object ids = string.Empty;
                _tableMetaData.IdProperties.TransferProperties(lookupResponse, ref ids);

                var sourceId = ids.ToString();
                if (string.IsNullOrWhiteSpace(sourceId))
                {
                    throw new Exception("No source Id was found in the lookup response.");
                }

                //Read/return the source object
                return await ReadStreamAsync(this._sourceContainer, _tableMetaData.ConcreteType, sourceId, (PartitionKeyBuilder)pkBuilder, requestOptions, cancellationToken);
            }
            return null;
        }

        /// <summary>
        /// This method will return the source object(s) for the provided cosmos query
        ///  that correspond to the alternate access pattern (aka Lookup) for the source object.
        /// </summary>
        /// <param name="queryDefinition"></param>
        /// <param name="continuationToken"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<PagedList<object>> LookupByQueryAsync(QueryDefinition queryDefinition, string group = null, string continuationToken = null, QueryRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (_tableMetaData.LookupTables == null)
            {
                throw new Exception("No Lookup dynamic type has been associated with the provided TableMetaData configuration.");
            }

            //For the source object lookup
            List<object> ids = new List<object>();
            object dicpk = new Dictionary<int, List<string>>();
            string keycol1 = (_tableMetaData.HierarchicalPartitionKeys.Count > 0) ? _tableMetaData.HierarchicalPartitionKeys?.ElementAt(0).Value?.Name : null;
            string keycol2 = (_tableMetaData.HierarchicalPartitionKeys.Count > 1) ? _tableMetaData.HierarchicalPartitionKeys?.ElementAt(1).Value?.Name : null;
            string keycol3 = (_tableMetaData.HierarchicalPartitionKeys.Count > 2) ? _tableMetaData.HierarchicalPartitionKeys?.ElementAt(2).Value?.Name : null;

            var ltmd = GetLookupMetaData(group);
            if (ltmd == null)
            {
                throw new Exception("Unable to determine Lookup table type and container.");
            }

            // Retrieve an iterator for the result set
            PagedList<object> objs = new PagedList<object>();
            using FeedIterator<object> results = ltmd.LookupContainer.GetItemQueryIterator<object>(queryDefinition, continuationToken, requestOptions);
            while (results.HasMoreResults)
            {
                FeedResponse<object> resultsPage = await results.ReadNextAsync().ConfigureAwait(false);

                //Transfer the lookup results (data) to the query id and HPK buckets
                foreach (var result in resultsPage)
                {
                    var lookupObj = this._optimizer.HydrateEntityByType(ltmd.LookupType, result.ToString());

                    _tableMetaData.HierarchicalPartitionKeys.TransferProperties(lookupObj, ref dicpk);

                    object eids = string.Empty;
                    _tableMetaData.IdProperties.TransferProperties(lookupObj, ref eids);
                    ids.Add(eids);
                }
            }
            if (ids.Count() == 0)
            {
                return objs;
            }

            var stringBuilder = base.GetSQLWithIdsAndHpks(ids.Cast<string>().ToList(),
                (keycol1, ((!string.IsNullOrEmpty(keycol1)) ? ((Dictionary<int, List<string>>)dicpk).ElementAt(0).Value : null)),
                (keycol2, ((!string.IsNullOrEmpty(keycol2)) ? ((Dictionary<int, List<string>>)dicpk).ElementAt(1).Value : null)),
                (keycol3, ((!string.IsNullOrEmpty(keycol3)) ? ((Dictionary<int, List<string>>)dicpk).ElementAt(2).Value : null)));

            //run query
            using FeedIterator<object> sourceResults = this._sourceContainer.GetItemQueryIterator<object>(stringBuilder.ToString(), null, requestOptions);
            while (sourceResults.HasMoreResults)
            {
                FeedResponse<object> sourceResultsPage = await sourceResults.ReadNextAsync().ConfigureAwait(false);
                foreach (object srcRes in sourceResultsPage)
                {
                    // Process result
                    objs.Add(HydrateEntityAsync(this._tableMetaData.ConcreteType, srcRes.ToString()));
                }
            }
            return objs;
        }

        /// <summary>
        /// Reads an item from the container as a stream.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="returnType"></param>
        /// <param name="id"></param>
        /// <param name="pkBuilder"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<object> ReadStreamAsync(Container container, Type returnType, string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Read the same item but as a stream.
            using (ResponseMessage responseMessage = await container.ReadItemStreamAsync(
                partitionKey: pkBuilder.Build(),
                id: id).ConfigureAwait(false)
            )
            {
                // Item stream operations do not throw exceptions for better performance
                if (responseMessage.IsSuccessStatusCode && responseMessage?.Content != null)
                {
                    using (var reader = new StreamReader(responseMessage.Content))
                    {
                        return HydrateEntityAsync(returnType, reader.ReadToEnd());
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// This method will upsert the Lookup item(s) if the provided source object is more recent.
        /// </summary>
        /// <param name="lookupType"></param>
        /// <param name="sourceChanges"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<object> UpsertIfMoreRecentAsync(object lookupType, 
            Dictionary<string, string> sourceChanges, 
            Dictionary<int, PropertyInfo> lookupIds, 
            Dictionary<int, PropertyInfo> lookupHpks, 
            string group = null, 
            ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            //Validation
            if (!lookupIds?.Any() ?? true)
            {
                throw new InvalidOperationException("At least 1 Id designation is required.");
            }

            var partitionKey = new PartitionKeyBuilder();
            var sourceProperties = lookupType.GetType().GetProperties().ToDictionary(p => p.Name, p => p);

            //Verify the idempotant key
            var _ts = ((sourceProperties.ContainsKey(Constants.ReservedTimestampKey)) ? sourceProperties[Constants.ReservedTimestampKey] : null);
            if (_ts == null)
            {
                throw new InvalidOperationException($"The source object must contain the idempotant key: ({Constants.ReservedTimestampKey})");
            }
            var sourceTs = (long)_ts.GetValue(lookupType);

            //Verify the optimisitc concurrency key
            var etag = (sourceProperties.ContainsKey("DataVersion")) ? sourceProperties["DataVersion"] : null;
            etag = etag ?? ((sourceProperties.ContainsKey(Constants.ReservedDataVersionKey)) ? sourceProperties[Constants.ReservedDataVersionKey] : null);
            if (etag == null)
            {
                throw new InvalidOperationException("The source object must contain the optimistic concurrency key: 'DataVersion' (_etag)");
            }
            var sourceVersion = etag.GetValue(lookupType)?.ToString() ?? null;
            
            var ltmd = GetLookupMetaData(group);
            if (ltmd == null)
            {
                throw new Exception("Unable to determine Lookup table type and container.");
            }

            // At this point we know the (dynamic) lookupType object has the required properties
            // Now we have to Get the actual CosmosDB enity (if it exists)
            // 1. Build the id
            // 2. Build the partition key
            // 3. Get the object
            // 4. Compare the idempotant key
            // 5. If the source object is more recent, then update the CosmosDB entity


            //But first, we need to check if the source object has changed the idempotant key
            //If it has, then we need to get the current CosmosDB entity using the old idempotant key

            //Detect orphaned Lookup object
            bool skipGetOperation = false;
            if (sourceChanges?.Any() ?? false)
            {
                //now, because it could be that the source object has changed one of the Lookup containers' hierarchical keys,
                //We should attempt to get the current look up object using a "Previous key" (if it exists)
                //structure needs to have any/all changes to either the Lookup container's HPK or its Ids

                //check ids
                bool hasIdChanges = lookupIds.Values.Select(x => x.Name).Any(key => sourceChanges.ContainsKey(key));
                bool hasHpkChanges = lookupHpks.Values.Select(x => x.Name).Any(key => sourceChanges.ContainsKey(key));
                bool hasHasBeenDeleted = sourceChanges.ContainsKey(Constants.ReservedDeletedKey);
                bool bDeleteFailed = false;
                
                if (hasIdChanges || hasHpkChanges || hasHasBeenDeleted) //any of these changes will cause the lookup object to be orphaned
                {
                    //build the old id
                    string oldStrId = null;
                    foreach (var id in lookupIds)
                    {
                        string val = (sourceProperties.ContainsKey(id.Value.Name) && sourceChanges.ContainsKey(id.Value.Name)) ? sourceChanges[id.Value.Name] : //grab the old id value from changes
                                        (sourceProperties.ContainsKey(id.Value.Name) ? sourceProperties[id.Value.Name].GetValue(lookupType).ToString() : null); //otherwise, grab the current value
                        if (!string.IsNullOrEmpty(val))
                        {
                            if (oldStrId != null) oldStrId += $"{Constants.DefaultIdDelimitor}";

                            oldStrId += $"{val}";
                        }
                    }
                    //build the old hpk
                    foreach (var hpk in lookupHpks.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value))
                    {
                        string val = (sourceProperties.ContainsKey(hpk.Value.Name) && sourceChanges.ContainsKey(hpk.Value.Name)) ? sourceChanges[hpk.Value.Name] : //grab the old hpk value from changes
                                        (sourceProperties.ContainsKey(hpk.Value.Name) ? sourceProperties[hpk.Value.Name].GetValue(lookupType)?.ToString() : null); //otherwise, grab the current value
                        if (!string.IsNullOrEmpty(val))
                        {
                            partitionKey.Add(val);
                        }
                    }
                    if (string.IsNullOrEmpty(oldStrId))
                    {
                        throw new InvalidOperationException("The source object has changes to the LookUp entity's ID (or HPK), but failed to generate the old ID for deletion.");
                    }

                    //delete the old object as it has been orphaned by the changes to the source entity
                    var response = await ltmd.LookupContainer.DeleteItemStreamAsync(oldStrId, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        //ToDo: Maybe just log this....?
                        //throw new InvalidOperationException($"Failed to delete the orphaned Lookup object.  Id: {oldStrId}");
                        bDeleteFailed = true;
                    }
                    if (hasHasBeenDeleted) return lookupType; //if the source object has been deleted, then we don't need to do anything else

                    //reset the partition key
                    partitionKey = new PartitionKeyBuilder();

                    //set the skipGetOperation flag to true so we don't attempt to get the old object based on the passed in id and hpk values,
                    //we know we won't find it because the source object has changed eiher the id or hpk values
                    skipGetOperation = true || bDeleteFailed; //re-read on delete failure

                    //remove/null the sourceVersion as this is not relevant to the new object
                    sourceVersion = null;
                }
            }

            //construct the id
            object sid = string.Empty;
            lookupIds.TransferProperties(lookupType, ref sid);

            string strId = sid.ToString();
            if (string.IsNullOrEmpty(strId))
            {
                throw new InvalidOperationException("Invalid Id value.");
            }

            object pkBuilder = new PartitionKeyBuilder();
            lookupHpks.TransferProperties(lookupType, ref pkBuilder);

            //Look up the entity
            object lookupResponse = null;
            if (!skipGetOperation) //We would skip this lookup if we had to delete an orphaned Lookup object...as it would not exist because the ID or HPK have changed
            {
                try
                {
                    // Perform a point read
                    lookupResponse = await ReadStreamAsync(ltmd.LookupContainer, ltmd.LookupType, strId.ToString(), (PartitionKeyBuilder)pkBuilder, requestOptions, cancellationToken).ConfigureAwait(false);
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                        throw ex;
                }
            }

            //Idempotency check
            if (lookupResponse != null) // && lookupResponse.Resource != null)
            {
                var fetchedObjProperties = lookupResponse.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                if (sourceProperties?.Any() ?? false)
                {
                    var ts = ((sourceProperties.ContainsKey(Constants.ReservedTimestampKey)) ? sourceProperties[Constants.ReservedTimestampKey] : null);
                    if (ts != null)
                    {
                        //compare fetched timestamp to provided timestamp
                        //Upsert if provided is more recent
                        var curts = (long)ts.GetValue(lookupResponse);
                        if (curts > 0 && sourceTs > curts)
                        {
                            //Nothing to do, the provided object's timestamp is older than currently stored value
                            return lookupResponse;
                        }
                    }
                }
            }

            //Upsert
            if (requestOptions == null && !string.IsNullOrEmpty(sourceVersion))
            {
                requestOptions = new ItemRequestOptions { IfMatchEtag = sourceVersion };
            }

            var packmethod = _optimizer.GetType().GetMethod("ToEntity").MakeGenericMethod(ltmd.LookupType);
            var packedEntity = packmethod.Invoke(_optimizer, new object[] { lookupType });

            var returnedEntity = await ltmd.LookupContainer.UpsertItemAsync(packedEntity, ((PartitionKeyBuilder)pkBuilder).Build(), requestOptions, cancellationToken).ConfigureAwait(false);

            //return the entity
            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
            if (returnResult && returnedEntity?.Resource != null)
            {
                return HydrateEntityAsync(ltmd.LookupType, returnedEntity.Resource.ToString());
            }
            return lookupResponse ?? lookupType;
        }


        #region privates
        private object HydrateEntityAsync(Type type, string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                var toReturn = this._optimizer.HydrateEntityByType(type, json);
                if (toReturn != null && toReturn is DiiCosmosEntity)
                {
                    ((DiiCosmosEntity)toReturn).SetInitialState(this._tableMetaData);
                }
                return toReturn;
            }
            return null;
        }

        private PartitionKeyBuilder GetPK(Dictionary<int, PropertyInfo> hpks, Dictionary<string, string> partitionKeys)
        {
            var partitionKey = new PartitionKeyBuilder();
            if (!_tableMetaData.LookupHpks?.Any() ?? true) return partitionKey;

            var key1 = (hpks.ContainsKey(0)) ? hpks[0] : null;
            var key2 = (hpks.ContainsKey(1)) ? hpks[1] : null;
            var key3 = (hpks.ContainsKey(2)) ? hpks[2] : null;

            var curkey = partitionKeys.Where(x => x.Key.Equals(key1.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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

        private LookupTableMetaData GetLookupMetaData(string group = null)
        {
            var ltmd = (!string.IsNullOrEmpty(group) && _tableMetaData.LookupTables.ContainsKey(group)) ? _tableMetaData.LookupTables[group] : null;
            if (ltmd == null)
            {
                if (_tableMetaData.LookupTables.Count == 1) ltmd = _tableMetaData.LookupTables.ElementAt(0).Value; //default to the one
            }
            return ltmd;
        }
        #endregion
    }
}
