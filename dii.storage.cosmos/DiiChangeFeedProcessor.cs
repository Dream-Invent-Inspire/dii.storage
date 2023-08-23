using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
    public class DiiChangeFeedProcessor : IDiiChangeFeedProcessor
    {
        protected readonly Type _concreteType;
        protected readonly TableMetaData _tableMetaData;
        protected readonly TableMetaData _sourceTblMetaData;
        protected readonly Container _container;
        protected readonly Optimizer _optimizer;
        protected readonly DiiCosmosContext _context;

        public DiiChangeFeedProcessor(Type concreteType, TableMetaData tableMetaData)
        {
            _context = DiiCosmosContext.Get();
            _concreteType = concreteType;
            _tableMetaData = tableMetaData; //This is the Lookup TableMetaData (which was dynamically constructed from the source table)
            _optimizer = Optimizer.Get();
            _sourceTblMetaData = _optimizer.TableMappings[concreteType];
        }
        public async Task HandleCosmosChangesAsync(IReadOnlyCollection<JObject> changes, CancellationToken cancellationToken)
        {
            var retries = new List<JObject>();
            foreach (var job in changes)
            {
                try
                {
                    //Process the job
                    if (!await ProcessJob(job))
                    {
                        //retry
                        retries.Add(job);
                    }
                }
                catch (CosmosException cre)
                {
                    //   now notice the failure when attempting the update 
                    //   this is because the ETag on the server no longer matches the ETag of this changed item...out of order..?
                    if (cre.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        //retry
                        retries.Add(job);
                    }

                    //TODO: log
                }
                catch (Exception ex)
                {
                    //TODO: log
                }
            }

            foreach(var retry in retries)
            {
                try
                {
                    //TODO: add exponential retry here
                    await ProcessJob(retry);
                }
                catch (Exception ex)
                { 
                }
            }

            return;
        }

        protected async Task<bool> ProcessJob(JObject job)
        {
            //Hydrate the lookup object
            //Executed command: var lookupObj = _optimizer.UnpackageFromJson<T>(job.ToString());

            var method = _optimizer.GetType().GetMethod("UnpackageFromJson").MakeGenericMethod(_concreteType); //This passed in concrete type is for the source table
            var sourceObj = method.Invoke(_optimizer, new object[] { job.ToString() });
            var dynamicObject = Activator.CreateInstance(_tableMetaData.ConcreteType); //this is the Lookup table type...the dynamically created (from the source) type

            var sourceProperties = sourceObj.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
            foreach (var targetProp in dynamicObject.GetType().GetProperties())
            {
                //Transfer data from source object to the target Dynamic object
                if (sourceProperties.ContainsKey(targetProp.Name))
                {
                    var sourceProp = sourceProperties[targetProp.Name];
                    if (sourceProp.PropertyType == targetProp.PropertyType) // Ensure the property types match
                    {
                        var valueToSet = sourceProp.GetValue(sourceObj);
                        targetProp.SetValue(dynamicObject, valueToSet);
                    }
                }
            }

            //upsert the sync'd object
            //NOTE: We don't need or want the entity back from the upsert...so don't unpack it.
            //Use EnableContentResponseOnWrite=false and cast result to bool....hack..?

            //First, set the Lookup Container on the (source) MetaTableData object
            //_tableMetaData.LookupContainer = _optimizer.TableMappings.ContainsKey(_tableMetaData.LookupType) ? _optimizer.TableMappings[_tableMetaData.LookupType]. : null;
            _tableMetaData.LookupContainer = _context.Client.GetContainer(_tableMetaData.DbId, _tableMetaData.SourceTableNameForLookup);
            if (_tableMetaData.LookupContainer == null)
            {
                throw new Exception($"Unable to find Lookup Container for {_tableMetaData.SourceTableNameForLookup}");
            }
            var adapter = new DiiCosmosLookupAdapter(_sourceTblMetaData); //This is the source table TableMetaData
            var result = await adapter.UpsertIfMoreRecentAsync(
                dynamicObject, 
                ((DiiCosmosEntity)sourceObj).ChangeTracker, 
                new ItemRequestOptions { EnableContentResponseOnWrite=false });
            
            return result != null;
        }
    }
}
