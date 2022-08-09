using dii.storage.Attributes;
using System;

namespace dii.storage.tests.Models
{
    public class FakeEntityThree : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityThree"/>.
		/// </summary>
		[PartitionKey(typeof(Guid))]
		public string FakeEntityThreeId { get; set; }

		[Id()]
		public string Id { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched.
		/// </summary>
		[Searchable("string")]
		public string SearchableStringValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(0)]
		public string CompressedStringValue { get; set; }

		/// <summary>
		/// A test object for complex nesting of objects to be searchable.
		/// </summary>
		[Searchable("recursive")]
		public FakeSearchableEntityThree RecursiveSearchable { get; set; }
	}
}