using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class OrderInvalidIdEntity : FakeDiiEntity
	{
		[PartitionKey]
		public string PK { get; set; }

		[Id(0)]
		public string Id1 { get; set; }

		[Id(0)]
		public string Id2 { get; set; }

		[Searchable("string")]
		public string SearchableStringValue { get; set; }
	}
}