using dii.storage.Attributes;
using System;
using System.Collections.Generic;

namespace dii.storage.tests.Models
{
    public class FakeEntityTwo : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityTwo"/>.
		/// </summary>
		[PartitionKey(typeof(Guid))]
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

		/// <summary>
		/// A test object for complex nesting of objects to be searchable.
		/// </summary>
		[Searchable("complex")]
		public FakeSearchableEntity ComplexSearchable { get; set; }

		[Searchable("collection_complex")]
		public List<FakeSearchableEntity> CollectionComplexSearchable { get; set; }

	}
}