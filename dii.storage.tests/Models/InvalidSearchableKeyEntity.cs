using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class InvalidSearchableKeyEntity : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="InvalidSearchableKeyEntity"/>.
		/// </summary>
		[PartitionKey()]
		public string InvalidSearchableKeyEntityId { get; set; }

		[Id()]
		public string Id { get { return InvalidSearchableKeyEntityId; } set { InvalidSearchableKeyEntityId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'p'.
		/// </summary>
		[Searchable(Constants.ReservedCompressedKey)]
		public string InvalidSearchableKeyStringPValue { get; set; }
	}
}