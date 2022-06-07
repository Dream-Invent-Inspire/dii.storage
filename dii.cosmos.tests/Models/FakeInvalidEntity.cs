using dii.cosmos.Attributes;

namespace dii.cosmos.tests.Models
{
    public class FakeInvalidEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntity"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeInvalidEntityId { get; set; }

		[Searchable("id")]
		public string Id { get { return FakeInvalidEntityId; } set { FakeInvalidEntityId = value; } }

		/// <summary>
		/// A <see cref="string"/> Cosmos primitive to be searched.
		/// </summary>
		[Searchable("_etag")]
		public string Version { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'p'.
		/// </summary>
		[Searchable("p")]
		public string InvalidSearchableKeyStringValue { get; set; }
	}
}