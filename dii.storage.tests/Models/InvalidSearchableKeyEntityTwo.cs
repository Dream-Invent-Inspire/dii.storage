using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class InvalidSearchableKeyEntityTwo : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="InvalidSearchableKeyEntityTwo"/>.
		/// </summary>
		[PartitionKey()]
		public string InvalidSearchableKeyEntityTwoId { get; set; }

		[Id()]
		public string Id { get { return InvalidSearchableKeyEntityTwoId; } set { InvalidSearchableKeyEntityTwoId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'PK'.
		/// </summary>
		[Searchable(Constants.ReservedPartitionKeyKey)]
		public string InvalidSearchableKeyStringPKValue { get; set; }
	}
}