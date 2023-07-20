using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class MultipleHierarchicalPartitionKeyEntity : FakeDiiEntity
	{
		[HierarchicalPartitionKey(order: 0, separator: '_')]
		public string PK1 { get; set; }

		[HierarchicalPartitionKey(order: 1, separator: '_')]
		public string PK2 { get; set; }

		[HierarchicalPartitionKey(order: 2, separator: '_')]
		public string PK3 { get; set; }

		[Id]
		public string Id { get; set; }

		[Searchable("string")]
		public string SearchableStringValue { get; set; }
	}
}