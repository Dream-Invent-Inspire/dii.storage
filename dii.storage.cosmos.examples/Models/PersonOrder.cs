using dii.storage.Attributes;
using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.cosmos.examples.Models
{
    /// <inheritdoc/>
    [StorageName("Example-PersonOrder")]
    public class PersonOrder : DiiCosmosEntity//, IDiiLookupEntity
    {
        

        /// <inheritdoc/>
        /// <summary>
        /// The Client Id that the <see cref="PersonOrder"/> record belongs to.
        /// </summary>
        [HierarchicalPartitionKey(order: 0)]
        [LookupHpk(order: 0, group: "*")]
        public string ClientId { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// The Order Id that the <see cref="PersonOrder"/> record belongs to.
        /// </summary>
        [HierarchicalPartitionKey(order: 1)]
        [Searchable("OrderId")]
        public string OrderId { get; set; }

        //[Id(0)]
        //public string id
        //{
        //    get
        //    {
        //        return PaymentId;
        //    }
        //    set
        //    {
        //        PaymentId = value;
        //    }
        //}

        /// <inheritdoc/>
        /// <summary>
        /// The payment Id of the <see cref="PersonOrder"/>.
        /// </summary>
        [Id(0)]
        [Searchable("PaymentId")]
        public string PaymentId { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// The Person Id that the <see cref="PersonOrder"/> record belongs to.
        /// </summary>
        [Searchable("PersonId")]
        [LookupHpk(order: 1, group: "PId")]
        public string PersonId { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// The order date of the <see cref="PersonOrder"/>.
        /// </summary>
        [Searchable("OrderDate")]
        public DateTimeOffset OrderDate { get; set; }

        /// <summary>
        /// For it to (also) be a Hierarchical Partition Key, it must be a string.
        /// </summary>
        [Searchable("OrderDateString")]
        [LookupHpk(order: 2, group: "PId")]
        public string OrderDateString
        {
            get
            {
                return OrderDate.ToString("yyyy-MM-dd");
            }
            set
            {
                if (DateTimeOffset.TryParse(value, out DateTimeOffset dt))
                {
                    OrderDate = dt;
                }
            }
        }

        /// <inheritdoc/>
        [Searchable("PaymentType")]
        [LookupIdAttribute(order: 0, group: "PId")]
        public string PaymentType { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// The payment amount of the <see cref="PersonOrder"/>.
        /// </summary>
        [Searchable("PaymentAmount")]
        [LookupIdAttribute(order: 1, idType: typeof(double), group: "PId")]
        public double PaymentAmount { get; set; }

        [Searchable("ReceiptNumber")]
        [LookupIdAttribute(order: 0, group: "Rec")]
        public string ReceiptNumber { get; set; }

        /// <inheritdoc/>
        [Searchable("Catalog")]
        public string Catalog { get; set; }

    }

}
