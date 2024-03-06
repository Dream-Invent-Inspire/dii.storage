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
    public class ExamplePersonOrderAdapter : DiiCosmosHierarchicalAdapter<PersonOrder> 
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

        public async Task<PersonOrder> CreateAsync(PersonOrder order, CancellationToken cancellationToken = default)
        {
            return await base.CreateAsync(order, cancellationToken: cancellationToken);
        }

        public async Task<PersonOrder> ReplaceAsync(PersonOrder order, CancellationToken cancellationToken = default)
        {
            return await base.ReplaceAsync(order, cancellationToken: cancellationToken);
        }

        public async Task<PersonOrder> UpsertAsync(PersonOrder order, CancellationToken cancellationToken = default)
        {
            return await base.UpsertAsync(order, cancellationToken: cancellationToken);
        }

        public async Task<List<PersonOrder>> BulkUpsertAsync(List<PersonOrder> orders, CancellationToken cancellationToken = default)
        {
            return await base.UpsertBulkAsync(orders, cancellationToken: cancellationToken);
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
            var results = await base.PatchBulkAsync(ops, cancellationToken: cancellationToken).ConfigureAwait(false);
            return results?.Any() ?? false;
        }

        public Task<List<PersonOrder>> ReplaceBulkAsync(IReadOnlyList<PersonOrder> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return base.ReplaceBulkAsync(diiEntities, requestOptions, cancellationToken);
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
            string polId = $"{paymentType}{dii.storage.Constants.DefaultIdDelimitor}{paymentAmount}";
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId },
                { "PersonId", personId },
                { "OrderDateString", orderDate.ToString("yyyy-MM-dd") }
            };

            //Lookup adapter stuff
            var adapter = new DiiCosmosLookupAdapter(this._table);

            var obj = await adapter.LookupAsync(polId, dic, "PId", cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var objs = await adapter.LookupByQueryAsync(query, "PId", null, null, cancellationToken).ConfigureAwait(false);
            return objs?.Cast<PersonOrder>().ToList();
        }

        public async Task<PagedList<PersonOrder>> GetManyByOrderDateAsync(string clientId, DateTime orderFromDate, DateTime orderToDate, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            QueryRequestOptions reqops = null; // new QueryRequestOptions { MaxItemCount = 4 };

            var dayCnt = orderToDate.Subtract(orderFromDate);
            List<string> days = new List<string>();
            for (int i = 0; i < dayCnt.Days; i++)
            {
                var date = orderFromDate.AddDays(i);
                var dateStr = date.ToString("yyyy-MM-dd");
                days.Add(dateStr);
            }
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.OrderDateString in (\"{string.Join("\",\"", days)}\")");
            queryDefinition.WithParameter("@clientId", clientId);

            var adapter = new DiiCosmosLookupAdapter(this._table);
            var retOrders = await adapter.LookupByQueryAsync(queryDefinition, "PId", continuationToken, reqops, cancellationToken).ConfigureAwait(false);
            return PagedList<PersonOrder>.CreateFromList(retOrders.Cast<PersonOrder>().ToList(), retOrders.ContinuationToken);
        }

        public async Task<PersonOrder> GetByReceiptAsync(string receipt, string clientId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId }
            };

            //Lookup adapter stuff
            var adapter = new DiiCosmosLookupAdapter(this._table);

            var obj = await adapter.LookupAsync(receipt, dic, "Rec", cancellationToken: cancellationToken).ConfigureAwait(false);
            return obj as PersonOrder;
        }

        public async Task<bool> DeleteByReceiptAsync(string receipt, string clientId, CancellationToken cancellationToken = default)
        {
            var dic = new Dictionary<string, string>
            {
                { "ClientId", clientId }
            };

            //Lookup adapter stuff
            var adapter = new DiiCosmosLookupAdapter(this._table);

            var fun = new Func<object, Task<bool>>(async (x) =>
            {
                return await base.DeleteEntityAsync(x as PersonOrder, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
            return await adapter.DeleteByLookupAsync(receipt, dic, fun, "Rec", cancellationToken: cancellationToken).ConfigureAwait(false);
        }


        public async Task<PagedList<PersonOrder>> GetByOrderDateAndItemAsync(string clientId, DateTime orderFromDate, DateTime orderToDate, string itemId, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var reqops = new QueryRequestOptions { MaxItemCount = 4 };

            var dayCnt = orderToDate.Subtract(orderFromDate);
            List<string> days = new List<string>();
            for (int i = 0; i < dayCnt.Days; i++)
            {
                var date = orderFromDate.AddDays(i);
                var dateStr = date.ToString("yyyy-MM-dd");
                days.Add(dateStr);
            }
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.OrderDateString in (\"{string.Join("\",\"", days)}\") AND c.MasterItemId = @itemId");
            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@itemId", itemId);

            var adapter = new DiiCosmosLookupAdapter(this._table);
            var retOrders = await adapter.LookupByQueryAsync(queryDefinition, "PId", continuationToken, reqops, cancellationToken).ConfigureAwait(false);
            return PagedList<PersonOrder>.CreateFromList(retOrders.Cast<PersonOrder>().ToList(), retOrders.ContinuationToken);
        }

        public async Task<PersonOrder> GetOrderByCheckNumber(string clientId, string checkNumber, CancellationToken cancellationToken = default)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.CheckNumber = @checkNumber");
            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@checkNumber", checkNumber);

            var adapter = new DiiCosmosLookupAdapter(this._table);
            var retOrder = await adapter.LookupByQueryAsync(queryDefinition, "Rec", null, null, cancellationToken).ConfigureAwait(false);
            return (retOrder?.Any() ?? false) ? retOrder.First() as PersonOrder : null;
        }
    }

}
