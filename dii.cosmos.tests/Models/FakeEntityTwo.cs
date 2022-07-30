using dii.cosmos.Attributes;
using dii.cosmos.Models;
using Microsoft.Azure.Cosmos;

namespace dii.cosmos.tests.Models
{
	public class FakeSearchableEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeInvalidEntity"/>.
		/// </summary>
		[Searchable("xtacos")]
		public string Tacos { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched. This property has an invalid search key of 'p'.
		/// </summary>
		[Searchable("xsoaps")]
		public string Soaps { get; set; }
	}
	
	public class FakeEntityTwo : DiiCosmosEntity
	{
		/// <summary>
		/// A test object for complex nesting of objects to be searchable.
		/// </summary>
		[Searchable("complex")]
		public FakeSearchableEntity ComplexSearchable { get; set; }

		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityTwo"/>.
		/// </summary>
		[PartitionKey(typeof(PartitionKey))]
		public string FakeEntityTwoId { get; set; }

		[Id()]
		public string Id { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched.
		/// </summary>
		[Searchable("string")]
		public string SearchableStringValue { get; set; }

		/// <summary>
		/// A <see cref="long"/> value to be searched.
		/// </summary>
		[Searchable("long")]
		public long SearchableLongValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(0)]
		public string CompressedStringValue { get; set; }
	}
}