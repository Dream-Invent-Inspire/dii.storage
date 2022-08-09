using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class InvalidSearchableKeyEntityThree : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="InvalidSearchableKeyEntityThree"/>.
		/// </summary>
		[PartitionKey()]
		public string InvalidSearchableKeyEntityThreeId { get; set; }

		[Id()]
		public string Id { get { return InvalidSearchableKeyEntityThreeId; } set { InvalidSearchableKeyEntityThreeId = value; } }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'id'.
		/// </summary>
		[Searchable(Constants.ReservedIdKey)]
		public string InvalidSearchableKeyStringIdValue { get; set; }
	}
}