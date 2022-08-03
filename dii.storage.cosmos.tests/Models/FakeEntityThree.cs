using dii.storage.Attributes;
using dii.storage.cosmos.Models;
using Microsoft.Azure.Cosmos;

namespace dii.storage.cosmos.tests.Models
{
	public class FakeEntityThree : DiiCosmosEntity
	{
		[PartitionKey(typeof(PartitionKey), 0, '_')]
		public string PK1 { get; set; }

		[PartitionKey(typeof(PartitionKey), 1, '_')]
		public string PK2 { get; set; }

		[PartitionKey(typeof(PartitionKey), 2, '_')]
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