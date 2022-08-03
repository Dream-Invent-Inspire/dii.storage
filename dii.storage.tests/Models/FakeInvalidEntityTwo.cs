using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class FakeInvalidEntityTwo : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntityTwo"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeInvalidEntityTwoId { get; set; }

		[Id()]
		public string Id { get { return FakeInvalidEntityTwoId; } set { FakeInvalidEntityTwoId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'PK'.
		/// </summary>
		[Searchable(Constants.ReservedPartitionKeyKey)]
		public string InvalidSearchableKeyStringPKValue { get; set; }
	}
}