using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class FirstPartitionKeySeparatorWinsEntity : FakeDiiEntity
	{
		[PartitionKey(order: 0, separator: '#')]
		public string PK1 { get; set; }

		[PartitionKey(order: 1, separator: '^')]
		public string PK2 { get; set; }

		[PartitionKey(order: 2, separator: '*')]
		public string PK3 { get; set; }

		[Id]
		public string Id { get; set; }

		[Searchable("string")]
		public string SearchableStringValue { get; set; }
	}
}