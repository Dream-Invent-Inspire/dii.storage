using dii.storage.Attributes;

namespace dii.storage.tests.Models
{
    public class FakeSearchableEntityFour
	{
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
		/// A value to test complex nesting and of the same type in the same object.
		/// </summary>
		[Searchable("complex1")]
		public FakeSearchableEntityTwo ComplexSearchable1 { get; set; }

		/// <summary>
		/// A value to test complex nesting and of the same type in the same object.
		/// </summary>
		[Searchable("complex2")]
		public FakeSearchableEntityTwo ComplexSearchable2 { get; set; }
	}
}