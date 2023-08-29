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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
    public class DiiCosmosLookupAdapter : DiiCosmosBaseAdapter //, IDiiCosmosLookupAdapter
    {
        protected readonly Container _sourceContainer;
        protected readonly Container _lookupContainer;
        protected readonly Optimizer _optimizer;
        protected readonly TableMetaData _tableMetaData;
        protected readonly DiiCosmosContext _context;

        public DiiCosmosLookupAdapter(TableMetaData tableMetaData)
        {
            _context = DiiCosmosContext.Get();
            _optimizer = Optimizer.Get();
            _tableMetaData = tableMetaData; // _optimizer.TableMappings[typeof(T)];
            _sourceContainer = _context.Client.GetContainer(_tableMetaData.DbId, _tableMetaData.TableName);
            _lookupContainer = tableMetaData.LookupContainer ?? _context.Client.GetContainer(_tableMetaData.DbId, _tableMetaData.LookupType.ToString());
        }

        public async Task<object> LookupAsync(string id, Dictionary<string, string> partitionKeys, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id) || partitionKeys == default(Dictionary<string, string>))
            {
                return null;
            }

            if (_tableMetaData.LookupType == null)
            {
                throw new Exception("No Lookup dynamic type has been associated with the provided TableMetaData configuration.");
            }

            // Build the full partition key path
            var partitionKey = GetPK(_tableMetaData.LookupHpks, partitionKeys);

            var lookupResponse = await ReadStreamAsync(this._lookupContainer, _tableMetaData.LookupType, id, partitionKey, requestOptions, cancellationToken);
            if (lookupResponse != null)
            {
                //pull the values from the lookup response to read the source
                var lookupProperties = lookupResponse.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                var sourceKeys = new Dictionary<string, string>();
                var sourceId = string.Empty;
                foreach (var key in _tableMetaData.HierarchicalPartitionKeys)
                {
                    if (lookupProperties.ContainsKey(key.Value.Name))
                    {
                        var res = lookupProperties[key.Value.Name].GetValue(lookupResponse);
                        if (res != null)
                            sourceKeys.Add(key.Value.Name, res.ToString());
                        else
                        {
                            //wtf.... this key property value is null in the dynamic lookup object
                        }
                    }
                }
                foreach (var idProp in _tableMetaData.IdProperties)
                {
                    if (!string.IsNullOrWhiteSpace(sourceId)) sourceId += $"{_tableMetaData.IdSeparator}";
                    if (lookupProperties.ContainsKey(idProp.Value.Name))
                    {
                        var res = lookupProperties[idProp.Value.Name].GetValue(lookupResponse);
                        if (res != null)
                            sourceId += lookupProperties[idProp.Value.Name].GetValue(lookupResponse).ToString();
                        else
                        {
                            //wtf....this id property value is null in the dynamic lookup object
                        }
                    }
                }
                partitionKey = GetPK(_tableMetaData.HierarchicalPartitionKeys, sourceKeys);
                if (string.IsNullOrWhiteSpace(sourceId))
                {
                    throw new Exception("No source Id was found in the lookup response.");
                }
                var sourceObject = await ReadStreamAsync(this._sourceContainer, _tableMetaData.ConcreteType, sourceId, partitionKey, requestOptions, cancellationToken);
                ((DiiCosmosEntity)sourceObject).SetInitialState(_tableMetaData);
                return sourceObject;
            }
            return null;
        }

        public async Task<List<object>> LookupByQueryAsync(QueryDefinition queryDefinition, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (_tableMetaData.LookupType == null)
            {
                throw new Exception("No Lookup dynamic type has been associated with the provided TableMetaData configuration.");
            }

            //For the source object lookup
            List<object> ids = new List<object>();
            List<object> key1 = new List<object>();
            List<object> key2 = new List<object>();
            List<object> key3 = new List<object>();
            string keycol1 = string.Empty, keycol2 = string.Empty, keycol3 = string.Empty;

            // Retrieve an iterator for the result set
            List<object> objs = new List<object>();
            using FeedIterator<object> results = this._lookupContainer.GetItemQueryIterator<object>(queryDefinition);
            while (results.HasMoreResults)
            {
                //List<Tuple<string, Dictionary<string, string>>> idAndPks = new List<Tuple<string, Dictionary<string, string>>>();
                FeedResponse<object> resultsPage = await results.ReadNextAsync();

                foreach (var result in resultsPage)
                {
                    var lookupObj = this._optimizer.HydrateEntityByType(this._tableMetaData.LookupType, result.ToString());

                    //pull the values from the lookup response to read the source
                    var lookupProperties = lookupObj.GetType().GetProperties().ToDictionary(p => p.Name, p => p);

                    var innerdic = _tableMetaData.HierarchicalPartitionKeys;
                    if (innerdic.Count() > 0 && lookupProperties.ContainsKey(innerdic.ElementAt(0).Value.Name))
                    {
                        var res = lookupProperties[innerdic.ElementAt(0).Value.Name].GetValue(lookupObj);
                        if (res != null)
                        {
                            key1.Add(res.ToString());
                            if (string.IsNullOrEmpty(keycol1)) keycol1 = innerdic.ElementAt(0).Value.Name;
                        }
                        else
                        {
                            //wtf.... this key property value is null in the dynamic lookup object
                        }
                    }
                    if (innerdic.Count() > 1 && lookupProperties.ContainsKey(innerdic.ElementAt(1).Value.Name))
                    {
                        var res = lookupProperties[innerdic.ElementAt(1).Value.Name].GetValue(lookupObj);
                        if (res != null)
                        {
                            key2.Add(res.ToString());
                            if (string.IsNullOrEmpty(keycol2)) keycol2 = innerdic.ElementAt(1).Value.Name;
                        }
                        else
                        {
                            //wtf.... this key property value is null in the dynamic lookup object
                        }
                    }
                    if (innerdic.Count() > 2 && lookupProperties.ContainsKey(innerdic.ElementAt(2).Value.Name))
                    {
                        var res = lookupProperties[innerdic.ElementAt(2).Value.Name].GetValue(lookupObj);
                        if (res != null)
                        {
                            key3.Add(res.ToString());
                            if (string.IsNullOrEmpty(keycol3)) keycol3 = innerdic.ElementAt(2).Value.Name;
                        }
                        else
                        {
                            //wtf.... this key property value is null in the dynamic lookup object
                        }
                    }

                    string sourceId = string.Empty;
                    foreach (var idProp in _tableMetaData.IdProperties)
                    {
                        if (!string.IsNullOrWhiteSpace(sourceId)) sourceId += $"{_tableMetaData.IdSeparator}";
                        if (lookupProperties.ContainsKey(idProp.Value.Name))
                        {
                            var res = lookupProperties[idProp.Value.Name].GetValue(lookupObj);
                            if (res != null)
                                sourceId += lookupProperties[idProp.Value.Name].GetValue(lookupObj).ToString();
                            else
                            {
                                //wtf....this id property value is null in the dynamic lookup object
                            }
                        }
                    }
                    ids.Add(sourceId);
                }
            }
            if (ids.Count() == 0)
            {
                return objs;
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
            using FeedIterator<object> sourceResults = this._sourceContainer.GetItemQueryIterator<object>(stringBuilder.ToString());
            while (sourceResults.HasMoreResults)
            {
                FeedResponse<object> sourceResultsPage = await sourceResults.ReadNextAsync();
                foreach (object srcRes in sourceResultsPage)
                {
                    // Process result
                    var returnSrcObj = this._optimizer.HydrateEntityByType(this._tableMetaData.ConcreteType, srcRes.ToString());
                    ((DiiCosmosEntity)returnSrcObj).SetInitialState(this._tableMetaData);
                    objs.Add(returnSrcObj);
                }
            }
            return objs;
        }

        protected async Task<object> ReadStreamAsync(Container container, Type returnType, string id, PartitionKeyBuilder pkBuilder, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            // Read the same item but as a stream.
            using (ResponseMessage responseMessage = await container.ReadItemStreamAsync(
                partitionKey: pkBuilder.Build(),
                id: id))
            {
                // Item stream operations do not throw exceptions for better performance
                if (responseMessage.IsSuccessStatusCode && responseMessage?.Content != null)
                {
                    using (var reader = new StreamReader(responseMessage.Content))
                    {
                        var json = reader.ReadToEnd();
                        var returnObj = this._optimizer.HydrateEntityByType(returnType, json);
                        return returnObj;
                    }
                }
                return null;
            }
        }

        public async Task<object> UpsertIfMoreRecentAsync(object lookupType, Dictionary<string, string> sourceChanges, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            //Validation
            if (!_tableMetaData.LookupIds?.Any() ?? true)
            {
                throw new InvalidOperationException("At least 1 Id designation is required.");
            }

            var partitionKey = new PartitionKeyBuilder();
            var sourceProperties = lookupType.GetType().GetProperties().ToDictionary(p => p.Name, p => p);

            //Verify the idempotant key
            var _ts = ((sourceProperties.ContainsKey(Constants.ReservedTimestampKey)) ? sourceProperties[Constants.ReservedTimestampKey] : null);
            if (_ts == null)
            {
                throw new InvalidOperationException("The source object must contain the idempotant key: 'DiiTimestampUTC' (_ts)");
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

            // At this point we know the (dynamic) lookupType object has the required properties
            // Now we have to Get the actual CosmosDB enity (if it exists)
            // 1. Build the id
            // 2. Build the partition key
            // 3. Get the object
            // 4. Compare the idempotant key
            // 5. Compare the optimistic concurrency key
            // 6. If the source object is more recent, then update the CosmosDB entity


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
                string oldStrId = null;
                bool hasIdChanges = _tableMetaData.LookupIds.Values.Select(x => x.Name).Any(key => sourceChanges.ContainsKey(key));
                bool hasHpkChanges = _tableMetaData.LookupHpks.Values.Select(x => x.Name).Any(key => sourceChanges.ContainsKey(key));
                bool bDeleteFailed = false;
                if (hasIdChanges || hasHpkChanges) //either changes will cause the lookup object to be orphaned
                {
                    //build the old id
                    foreach (var id in _tableMetaData.LookupIds)
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
                    foreach (var hpk in _tableMetaData.LookupHpks)
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
                    var response = await this._lookupContainer.DeleteItemStreamAsync(oldStrId, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        //ToDo: Maybe just log this....?
                        //throw new InvalidOperationException($"Failed to delete the orphaned Lookup object.  Id: {oldStrId}");
                        bDeleteFailed = true;
                    }
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
            string strId = null;
            for (int i = 0; i < _tableMetaData.LookupIds.Count(); i++)
            {
                if (sourceProperties.ContainsKey(_tableMetaData.LookupIds[i].Name))
                {
                    var sourceProp = sourceProperties[_tableMetaData.LookupIds[i].Name].GetValue(lookupType)?.ToString();
                    if (string.IsNullOrEmpty(sourceProp))
                    {
                        throw new InvalidOperationException("Invalid Id value.");
                    }

                    if (strId != null) strId += $"|";
                    strId += $"{sourceProp}";
                }
            }

            if (string.IsNullOrEmpty(strId))
            {
                throw new InvalidOperationException("Invalid Id value.");
            }

            //Construct the partition key
            if (_tableMetaData.LookupHpks.Count() > 0 && sourceProperties.ContainsKey(_tableMetaData.LookupHpks.ElementAt(0).Value.Name))
            {
                var sourceProp = sourceProperties[_tableMetaData.LookupHpks.ElementAt(0).Value.Name];
                var sourceValue = sourceProp.GetValue(lookupType)?.ToString();
                if (string.IsNullOrEmpty(sourceValue)) throw new InvalidOperationException("Invalid Partition Key value.");
                partitionKey.Add(sourceValue);
            }
            if (_tableMetaData.LookupHpks.Count() > 1 && sourceProperties.ContainsKey(_tableMetaData.LookupHpks.ElementAt(1).Value.Name))
            {
                var sourceProp = sourceProperties[_tableMetaData.LookupHpks.ElementAt(1).Value.Name];
                var sourceValue = sourceProp.GetValue(lookupType)?.ToString();
                if (string.IsNullOrEmpty(sourceValue)) throw new InvalidOperationException("Invalid Partition Key value.");
                partitionKey.Add(sourceValue);
            }
            if (_tableMetaData.LookupHpks.Count() > 2 && sourceProperties.ContainsKey(_tableMetaData.LookupHpks.ElementAt(2).Value.Name))
            {
                var sourceProp = sourceProperties[_tableMetaData.LookupHpks.ElementAt(2).Value.Name];
                var sourceValue = sourceProp.GetValue(lookupType)?.ToString();
                if (string.IsNullOrEmpty(sourceValue)) throw new InvalidOperationException("Invalid Partition Key value.");
                partitionKey.Add(sourceValue);
            }

            //Look up the entity
            object lookupResponse = null;
            if (!skipGetOperation) //We would skip this lookup if we had to delete an orphaned Lookup object...as it would not exist because the ID or HPK have changed
            {
                try
                {
                    // Perform a point read
                    lookupResponse = await ReadStreamAsync(this._lookupContainer, _tableMetaData.LookupType, strId, partitionKey, requestOptions, cancellationToken);
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                        throw ex;
                }
            }

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

            //var packedEntity = _optimizer.ToEntity(diiEntity);
            var packmethod = _optimizer.GetType().GetMethod("ToEntity").MakeGenericMethod(_tableMetaData.LookupType);
            var packedEntity = packmethod.Invoke(_optimizer, new object[] { lookupType });

            var returnedEntity = await this._lookupContainer.UpsertItemAsync(packedEntity, partitionKey.Build(), requestOptions, cancellationToken).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;
            if (returnResult && returnedEntity?.Resource != null)
            {
                var toReturn = this._optimizer.HydrateEntityByType(_tableMetaData.LookupType, returnedEntity.Resource.ToString());
                return toReturn;
            }
            return lookupResponse ?? lookupType;
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
    }
}
