using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.cosmos.examples.Models
{
    [StorageName("Example-PersonSession")]
    public class PersonSession : IDiiEntity
    {
        /// <summary>
        /// The Client Id that the <see cref="Person"/> record belongs to.
        /// </summary>
        //[PartitionKey(typeof(PartitionKey))]
        [HierarchicalPartitionKey(order: 0)]
        public string ClientId { get; set; }

        /// <summary>
        /// The Client Id that the <see cref="Person"/> record belongs to.
        /// </summary>
        [HierarchicalPartitionKey(order: 1)]
        public string PersonId { get; set; }


        /// <summary>
        /// The Unique Id for the <see cref="Person"/>.
        /// </summary>
        [Id]
        public string SessionId { get; set; }

        /// <summary>
        /// The name of the <see cref="Person"/>.
        /// </summary>
        [Searchable("cat")]
        public string Catalog { get; set; }

        /// <summary>
        /// The age of the <see cref="Person"/>.
        /// </summary>
        [Searchable("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// The address of the <see cref="Person"/>.
        /// </summary>
        [Searchable("started")]
        public DateTime SessionStartDate { get; set; }

        [Searchable("ended")]
        public DateTime? SessionEndDate { get; set; }


        /// <summary>
        /// Other data for the <see cref="Person"/> that does not need to be searchable via queries.
        /// </summary>
        [Compress(0)]
        public string SessionData { get; set; }

        /// <inheritdoc/>
        [Searchable("_etag")]
        public string DataVersion { get; set; }

        /// <inheritdoc/>
        [IgnoreMember]
        public Version SchemaVersion => new(1, 0);
    }
}
