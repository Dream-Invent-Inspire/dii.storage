using dii.storage.Attributes;
using dii.storage.cosmos.Models;

namespace dii.storage.cosmos.tests.Models
{
    public class FakeInvalidEntityThree : DiiCosmosEntity
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