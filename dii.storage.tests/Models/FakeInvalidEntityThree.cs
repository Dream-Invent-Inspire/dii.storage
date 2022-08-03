using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class FakeInvalidEntityThree : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntityThree"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeInvalidEntityThreeId { get; set; }

		[Id()]
		public string Id { get { return FakeInvalidEntityThreeId; } set { FakeInvalidEntityThreeId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'id'.
		/// </summary>
		[Searchable(Constants.ReservedIdKey)]
		public string InvalidSearchableKeyStringIdValue { get; set; }
	}
}