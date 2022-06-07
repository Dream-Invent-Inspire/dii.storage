using dii.cosmos.Attributes;

namespace dii.cosmos.tests.Models
{
    public class FakeEntityTwo
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityTwo"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeEntityTwoId { get; set; }

		[Searchable("id")]
		public string Id { get { return FakeEntityTwoId; } set { FakeEntityTwoId = value; } }

		/// <summary>
		/// A <see cref="string"/> Cosmos primitive to be searched.
		/// </summary>
		[Searchable("_etag")]
		public string Version { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(0)]
		public string CompressedStringValue { get; set; }
	}
}