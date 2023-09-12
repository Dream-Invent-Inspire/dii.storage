using dii.storage.cosmos.examples.Models.Interfaces;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dii.storage.Models;

namespace dii.storage.cosmos.examples.Adapters
{
    public class ExamplePersonOrderAdapter : DiiCosmosHierarchicalAdapter<PersonOrder> //, IExamplePersonSessionAdapter
    {
        /// <summary>
        /// Fetch a PersonOrder where the entity (PersonOrder) id = paymentId, and clientId and orderId are the HPK values
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="clientId"></param>
        /// <param name="orderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PersonOrder> FetchAsync(string paymentId, string clientId, string orderId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "OrderId", orderId }
            };
            return await base.GetAsync(paymentId, dic, cancellationToken: cancellationToken);
        }

        public async Task<List<PersonOrder>> GetManyByOrderIdsAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks)
        {
            var results = await base.GetManyAsync(idAndPks).ConfigureAwait(false);
            return results.ToList();
        }

        public async Task<PagedList<PersonOrder>> GetManyByClientAndPersonIdAsync(string clientId, string personId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);

            return await base.GetPagedAsync(queryDefinition);
        }

        public async Task<PagedList<PersonOrder>> GetManyByClientIdAsync(string clientId, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var reqops = new QueryRequestOptions { MaxItemCount = 4 };

            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId");

            queryDefinition.WithParameter("@clientId", clientId);

            return await base.GetPagedAsync(queryDefinition, continuationToken, reqops, cancellationToken);
        }

        public async Task<PagedList<PersonOrder>> GetManyByOrderDateAsync(string clientId, DateTime orderFromDate, DateTime orderToDate, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var reqops = new QueryRequestOptions { MaxItemCount = 4 };

            var dayCnt = orderToDate.Subtract(orderFromDate);
            List<string> days = new List<string>();
            for(int i=0; i<dayCnt.Days; i++)
            {
                var date = orderFromDate.AddDays(i);
                var dateStr = date.ToString("yyyy-MM-dd");
                days.Add(dateStr);
            }
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.OrderDateString in (\"{string.Join("\",\"", days)}\")");
            queryDefinition.WithParameter("@clientId", clientId);

            var adapter = new DiiCosmosLookupAdapter(this._table);
            var retOrders = await adapter.LookupByQueryAsync(queryDefinition, continuationToken, reqops, cancellationToken);
            return PagedList<PersonOrder>.CreateFromList(retOrders.Cast<PersonOrder>().ToList(), retOrders.ContinuationToken);
        }

        public async Task<PersonOrder> CreateAsync(PersonOrder session, CancellationToken cancellationToken = default)
        {
            return await base.CreateAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonOrder> ReplaceAsync(PersonOrder session, CancellationToken cancellationToken = default)
        {
            return await base.ReplaceAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonOrder> UpsertAsync(PersonOrder session, CancellationToken cancellationToken = default)
        {
            return await base.UpsertAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<PersonOrder> AddEndTimeAsync(string personId, string clientId, string sessionId, DateTime started, DateTime ended, CancellationToken cancellationToken = default)
        {
            var patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = true
            };

            var options = new Dictionary<string, object>
            {
                { "/ended", ended },
                { "/duration", (long)(ended - started).TotalMilliseconds }
            };

            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };

            var session = await base.PatchAsync(sessionId, dic, options, patchItemRequestOptions, cancellationToken).ConfigureAwait(false);
            return session;
        }

        public async Task<bool> DeleteEntityAsync(PersonOrder session, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntityAsync(session, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteEntityByIdAsync(string personId, string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId }
            };

            return await base.DeleteEntityByIdAsync(sessionId, dic, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteBulkAsync(IReadOnlyList<PersonOrder> orders, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntitiesBulkAsync(orders, cancellationToken: cancellationToken);
        }

        public async Task<bool> PatchBulkAsync(IReadOnlyList<(string, Dictionary<string, string>)> idAndPks, CancellationToken cancellationToken = default)
        {
            var ops = idAndPks.Select(x => (x.Item1, x.Item2, new Dictionary<string, object> { { "/Catalog", "BrandX" } })).ToList();
            var results = await base.PatchBulkAsync(ops, cancellationToken: cancellationToken);
            return results?.Any() ?? false;
        }

        //Alternate access patterns for Lookup tables

        /// <summary>
        /// Get a PersonOrder where the entity (in this case it the (dynamic) PersonOrderLookup) id =  PaymentId,  and ClientId and personId are the HPK values
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="clientId"></param>
        /// <param name="personId"></param>
        /// <param name="orderDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PersonOrder> GetByPersonIdAsync(string paymentType, string paymentAmount, string clientId, string personId, DateTimeOffset orderDate, CancellationToken cancellationToken = default)
        {
            string polId = $"{paymentType}{Constants.DefaultIdDelimitor}{paymentAmount}";
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId },
                { "OrderDateString", orderDate.ToString("yyyy-MM-dd") }
            };

            //Lookup adapter stuff
            var adapter = new DiiCosmosLookupAdapter(this._table);

            var obj = await adapter.LookupAsync(polId, dic, cancellationToken: cancellationToken);
            return obj as PersonOrder;
        }

        public async Task<List<PersonOrder>> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId }
            };
            QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.ClientId = @clientId")
                    .WithParameter("@clientId", clientId);

            //Lookup adapter stuff
            var adapter = new DiiCosmosLookupAdapter(this._table);

            var objs = await adapter.LookupByQueryAsync(query, null, null, cancellationToken);
            return objs?.Cast<PersonOrder>().ToList();
        }
    }

}
