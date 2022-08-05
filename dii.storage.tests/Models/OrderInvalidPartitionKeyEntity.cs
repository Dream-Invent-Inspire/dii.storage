using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class OrderInvalidPartitionKeyEntity : FakeDiiEntity
	{
		[PartitionKey(order: 0)]
		public string PK1 { get; set; }

		[PartitionKey(order: 0)]
		public string PK2 { get; set; }

		[Id]
		public string Id { get; set; }

		[Searchable("string")]
		public string SearchableStringValue { get; set; }
	}
}