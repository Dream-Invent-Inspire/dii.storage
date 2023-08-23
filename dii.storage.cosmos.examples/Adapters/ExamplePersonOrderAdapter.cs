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

        public async Task<List<PersonOrder>> GetManyByOrderIdsAsync(IReadOnlyList<Tuple<string, Dictionary<string, string>>> idAndPks)
        {
            var results = await base.GetManyAsync(idAndPks).ConfigureAwait(false);
            return results.ToList();
        }

        public async Task<PagedList<PersonOrder>> GetManyByClientIdAsync(string clientId, string personId)
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.ClientId = @clientId AND c.PersonId = @personId");

            queryDefinition.WithParameter("@clientId", clientId);
            queryDefinition.WithParameter("@personId", personId);

            return await base.GetPagedAsync(queryDefinition);
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

        public async Task<bool> DeleteBulkAsync(IReadOnlyList<PersonOrder> sessions, CancellationToken cancellationToken = default)
        {
            return await base.DeleteEntitiesBulkAsync(sessions, cancellationToken: cancellationToken);
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

    }

}
