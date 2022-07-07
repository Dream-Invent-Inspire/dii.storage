using dii.cosmos.Attributes;
using dii.cosmos.Models;
using Microsoft.Azure.Cosmos;

namespace dii.cosmos.tests.Models
{
    public class FakeEntityTwo : DiiCosmosEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityTwo"/>.
		/// </summary>
		[PartitionKey(typeof(PartitionKey))]
		public string FakeEntityTwoId { get; set; }

		[Id()]
		public string Id { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched.
		/// </summary>
		[Searchable("string")]
		public string SearchableStringValue { get; set; }

		/// <summary>
		/// A <see cref="long"/> value to be searched.
		/// </summary>
		[Searchable("long")]
		public long SearchableLongValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(0)]
		public string CompressedStringValue { get; set; }
	}
}