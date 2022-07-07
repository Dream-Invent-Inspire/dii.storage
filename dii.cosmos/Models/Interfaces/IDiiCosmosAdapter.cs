using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dii.cosmos.Models.Interfaces
{
    public interface IDiiCosmosAdapter<T> where T : IDiiCosmosEntity, new()
    {
        #region Fetch APIs
        /// <summary>
        /// Reads a item from the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="id">The Cosmos item id</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <param name="requestOptions">(Optional) The options for the item request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// A System.Threading.Tasks.Task containing a Microsoft.Azure.Cosmos.ResponseMessage
        /// which wraps a System.IO.Stream containing the read resource record.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<T> GetAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads multiple items from a container using Id and PartitionKey values.
        /// </summary>
        /// <param name="items">List of item.Id and Microsoft.Azure.Cosmos.PartitionKey</param>
        /// <param name="readManyRequestOptions">Request Options for ReadMany Operation</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// A System.Threading.Tasks.Task containing a Microsoft.Azure.Cosmos.ResponseMessage
        /// which wraps a System.IO.Stream containing the response.
        /// </returns>
        /// <remarks>
        /// This is meant to perform better latency-wise than a query with IN statements to fetch
        /// a large number of independent items.
        /// </remarks>
        Task<ICollection<T>> GetManyAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method creates a query for items under a container in an Azure Cosmos database
        /// using a SQL statement with parameterized values. For more information on preparing
        /// SQL statements with parameterized values, please see
        /// Microsoft.Azure.Cosmos.QueryDefinition.
        /// </summary>
        /// <param name="queryDefinition">The Cosmos SQL query definition.</param>
        /// <param name="continuationToken">(Optional) The continuation token in the Azure Cosmos DB service.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <returns>
        /// A PagedList of items.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<PagedList<T>> GetPagedAsync(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null);

        /// <summary>
        /// This method creates a query for items under a container in an Azure Cosmos database
        /// using a SQL statement.
        /// </summary>
        /// <param name="queryText">The Cosmos SQL query text.</param>
        /// <param name="continuationToken">(Optional) The continuation token in the Azure Cosmos DB service.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <returns>
        /// A PagedList of items.
        /// </returns>
        /// <remarks>
        /// Only supports single partition queries.
        /// </remarks>
        Task<PagedList<T>> GetPagedAsync(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null);
        #endregion Fetch APIs

        #region Create APIs
        /// <summary>
        /// Creates a item as an asynchronous operation in the Azure Cosmos service.
        /// </summary>
        /// <param name="diiCosmosEntity">A JSON serializable object that must contain an id property.</param>
        /// <param name="partitionKey">The partition key for the item. If not specified will be populated by extracting from {T}.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The item that was created.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<T> CreateAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates multiple items as an asynchronous operation in the Azure Cosmos service.
        /// </summary>
        /// <param name="diiCosmosEntities">List of Microsoft.Azure.Cosmos.PartitionKey and entities.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The items that were created.
        /// </returns>
        /// <remarks>
        /// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
        /// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
        /// </remarks>
        Task<ICollection<T>> CreateBulkAsync(IReadOnlyList<(PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        #endregion Create APIs

        #region Replace APIs
        /// <summary>
        /// Replaces a item in the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="diiCosmosEntity">A JSON serializable object that must contain an id property.</param>
        /// <param name="id">The Cosmos item id of the existing item.</param>
        /// <param name="partitionKey">The partition key for the item. If not specified will be populated by extracting from {T}.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The item that was updated.
        /// </returns>
        /// <remarks>
        /// The item's partition key value is immutable. To change an item's partition key
        /// value you must delete the original item and insert a new item.
        /// <para>
        /// This operation does not work on entities that use the same value for both the id and parition key.
        /// </para>
        /// </remarks>
        Task<T> ReplaceAsync(T diiCosmosEntity, string id, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces multiple items in the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="diiCosmosEntities">List of item.Id, Microsoft.Azure.Cosmos.PartitionKey and entities.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The items that were updated.
        /// </returns>
        /// <remarks>
        /// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
        /// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
        /// <para>
        /// The item's partition key value is immutable. To change an item's partition key
        /// value you must delete the original item and insert a new item.
        /// </para>
        /// <para>
        /// This operation does not work on entities that use the same value for both the id and parition key.
        /// </para>
        /// </remarks>
        Task<ICollection<T>> ReplaceBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        #endregion Replace APIs

        #region Upsert APIs
        /// <summary>
        /// Upserts an item as an asynchronous operation in the Azure Cosmos service.
        /// </summary>
        /// <param name="diiCosmosEntity">A JSON serializable object that must contain an id property.</param>
        /// <param name="partitionKey">The partition key for the item. If not specified will be populated by extracting from {T}.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The item that was upserted.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<T> UpsertAsync(T diiCosmosEntity, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upserts multiple items as an asynchronous operation in the Azure Cosmos service.
        /// </summary>
        /// <param name="diiCosmosEntities">List of Microsoft.Azure.Cosmos.PartitionKey and entities.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The items that were upserted.
        /// </returns>
        /// <remarks>
        /// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
        /// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
        /// </remarks>
        Task<ICollection<T>> UpsertBulkAsync(IReadOnlyList<(PartitionKey partitionKey, T diiCosmosEntity)> diiCosmosEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        #endregion Upsert APIs

        #region Patch APIs
        /// <summary>
        /// Patches an item in the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="id">The Cosmos item id of the item to be patched.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <param name="patchOperations">Represents a list of operations to be sequentially applied to the referred Cosmos item.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The item that was updated.
        /// </returns>
        /// <remarks>
        /// The item's partition key value is immutable. To change an item's partition key
        /// value you must delete the original item and insert a new item. The patch operations
        /// are atomic and are executed sequentially. By default, resource body will be returned
        /// as part of the response. User can request no content by setting Microsoft.Azure.Cosmos.ItemRequestOptions.EnableContentResponseOnWrite
        /// flag to false.
        /// </remarks>
        Task<T> PatchAsync(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Patches multiple items in the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="patchOperations">List of item.Id, Microsoft.Azure.Cosmos.PartitionKey and a list of operations to be sequentially applied to the referred Cosmos item.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The items that were updated.
        /// </returns>
        /// <remarks>
        /// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
        /// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
        /// <para>
        /// The item's partition key value is immutable. To change an item's partition key
        /// value you must delete the original item and insert a new item. The patch operations
        /// are atomic and are executed sequentially. By default, resource body will be returned
        /// as part of the response. User can request no content by setting Microsoft.Azure.Cosmos.ItemRequestOptions.EnableContentResponseOnWrite
        /// flag to false.
        /// </para>
        /// </remarks>
        Task<ICollection<T>> PatchBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        #endregion Patch APIs

        #region Delete APIs
        /// <summary>
        /// Delete a item from the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="id">The Cosmos item id</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The success status of the operation.
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<bool> DeleteAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete multiple items from the Azure Cosmos service as an asynchronous operation.
        /// </summary>
        /// <param name="ids">List of item.Id and Microsoft.Azure.Cosmos.PartitionKey.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
        /// <returns>
        /// The success status of the operation.
        /// </returns>
        /// <remarks>
        /// When <see cref="CosmosClientOptions.AllowBulkExecution"/> is set to <see langword="true"/>, allows optimistic batching of requests
        /// to the service. This option is recommended for non-latency sensitive scenarios only as it trades latency for throughput.
        /// </remarks>
        Task<bool> DeleteBulkAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> ids, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        #endregion Delete APIs
    }
}