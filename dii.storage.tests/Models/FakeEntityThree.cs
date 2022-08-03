using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class FakeEntityThree : FakeDiiEntity
	{
		[PartitionKey(order: 0, separator: '_')]
		public string PK1 { get; set; }

		[PartitionKey(order: 1, separator: '_')]
		public string PK2 { get; set; }

		[PartitionKey(order: 2, separator: '_')]
		public string PK3 { get; set; }

		[Id(0, '#')]
		public string Id1 { get; set; }

		[Id(1, '#')]
		public string Id2 { get; set; }

		[Id(2, '#')]
		public string Id3 { get; set; }

		[Searchable("string")]
		public string SearchableStringValue { get; set; }
	}
}