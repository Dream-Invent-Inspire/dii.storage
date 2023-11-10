using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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
        //protected readonly DiiCosmosContext _context;

        public DiiChangeFeedProcessor(Type concreteType, TableMetaData tableMetaData)
        {
            //_context = DiiCosmosContext.Get();
            _concreteType = concreteType;
            _tableMetaData = tableMetaData; //This is the Lookup TableMetaData (which was dynamically constructed from the source table)
            _optimizer = Optimizer.Get();
            _sourceTblMetaData = _optimizer.TableMappings[concreteType];
        }

        /// <summary>
        /// Handles lookup table synchronization
        /// </summary>
        /// <param name="changes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleCosmosChangesAsync(IReadOnlyCollection<JObject> changes, CancellationToken cancellationToken)
        {
            var retries = new List<JObject>();
            foreach (var job in changes)
            {
                try
                {
                    //Process the job
                    if (!await ProcessJobAsync(job))
                    {
                        //retry
                        retries.Add(job);
                    }
                }
                catch (CosmosException cre)
                {
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
                    await ProcessJobAsync(retry);
                }
                catch (Exception ex)
                { 
                }
            }

            return;
        }

        protected async Task<bool> ProcessJobAsync(JObject job)
        {
            //Hydrate the source and lookup objects
            var method = _optimizer.GetType().GetMethod("UnpackageFromJson").MakeGenericMethod(_concreteType); //This passed in concrete type is for the source table
            var sourceObj = method.Invoke(_optimizer, new object[] { job.ToString() });
            
            //Upsert the Lookup object
            var adapter = new DiiCosmosLookupAdapter(_sourceTblMetaData); //This is the source table TableMetaData
            
            List<Task<object>> tasks = new List<Task<object>>();
            List<object> results = new List<object>();

            foreach (var group in _sourceTblMetaData.LookupIds)
            {
                var dynamicObject = Activator.CreateInstance(_sourceTblMetaData.LookupTables[group.Key].LookupType); //this is the Lookup table type...the dynamically created (from the source) type

                //Transfer data from source object to the target Dynamic object
                var sourceProperties = sourceObj.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                foreach (var targetProp in dynamicObject.GetType().GetProperties())
                {
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

                tasks.Add(adapter.UpsertIfMoreRecentAsync(
                                dynamicObject,
                                ((DiiCosmosEntity)sourceObj).ChangeTracker,
                                _sourceTblMetaData.LookupIds[group.Key],
                                _sourceTblMetaData.GetHPKs(group.Key),
                                group.Key,
                                new ItemRequestOptions { EnableContentResponseOnWrite = false }));

            }
            var completedTasks = await Task.WhenAll(tasks);
            return completedTasks.All(x => x != null) && completedTasks.Count() == _sourceTblMetaData.LookupIds.Count();
        }
    }
}
