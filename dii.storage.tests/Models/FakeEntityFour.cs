using dii.storage.Attributes;
using System;

namespace dii.storage.tests.Models
{
    public class FakeEntityFour : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="FakeEntityFour"/>.
		/// </summary>
		[PartitionKey(typeof(Guid))]
		public string FakeEntityFourId { get; set; }

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
		[Searchable("complex1")]
		public FakeSearchableEntity ComplexSearchable1 { get; set; }

		/// <summary>
		/// A test object for complex nesting of objects to be searchable.
		/// </summary>
		[Searchable("complex2")]
		public FakeSearchableEntityFour ComplexSearchable2 { get; set; }

	}
}