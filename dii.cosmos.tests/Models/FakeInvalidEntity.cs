using dii.cosmos.Attributes;
using dii.cosmos.Models;

namespace dii.cosmos.tests.Models
{
    public class FakeInvalidEntity : DiiCosmosEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntity"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeInvalidEntityId { get; set; }

		[Id()]
		public string Id { get { return FakeInvalidEntityId; } set { FakeInvalidEntityId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'p'.
		/// </summary>
		[Searchable(Constants.ReservedCompressedKey)]
		public string InvalidSearchableKeyStringPValue { get; set; }
	}
}