using dii.cosmos.Attributes;
using dii.cosmos.Models;

namespace dii.cosmos.tests.Models
{
    public class FakeInvalidEntityTwo : DiiCosmosEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntityTwo"/>.
		/// </summary>
		[PartitionKey()]
		public string FakeInvalidEntityTwoId { get; set; }

		[Id()]
		public string Id { get { return FakeInvalidEntityTwoId; } set { FakeInvalidEntityTwoId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'p'.
		/// </summary>
		[Searchable(Constants.ReservedPartitionKeyKey)]
		public string InvalidSearchableKeyStringPKValue { get; set; }
	}
}