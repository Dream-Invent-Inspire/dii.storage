using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using Microsoft.Azure.Cosmos;
using System;

namespace dii.storage.cosmos.examples.Models
{
    public class Person : IDiiEntity
	{
		/// <summary>
		/// The Client Id that the <see cref="Person"/> record belongs to.
		/// </summary>
		[PartitionKey(typeof(PartitionKey))]
		public string ClientId { get; set; }

		/// <summary>
		/// The Unique Id for the <see cref="Person"/>.
		/// </summary>
		[Id]
		public string PersonId { get; set; }

		/// <summary>
		/// The name of the <see cref="Person"/>.
		/// </summary>
		[Searchable("name")]
		public string Name { get; set; }

		/// <summary>
		/// The age of the <see cref="Person"/>.
		/// </summary>
		[Searchable("age")]
		public long Age { get; set; }

		/// <summary>
		/// The address of the <see cref="Person"/>.
		/// </summary>
		[Searchable("address")]
		public Address Address { get; set; }

		/// <summary>
		/// Other data for the <see cref="Person"/> that does not need to be searchable via queries.
		/// </summary>
		[Compress(0)]
		public string OtherData { get; set; }

		/// <inheritdoc/>
		[Searchable("_etag")]
		public string DataVersion { get; set; }

		/// <inheritdoc/>
		[IgnoreMember]
		public Version SchemaVersion => new(1, 0);
	}
}