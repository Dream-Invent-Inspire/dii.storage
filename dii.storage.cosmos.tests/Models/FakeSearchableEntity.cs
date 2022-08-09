using dii.storage.Attributes;

namespace dii.storage.cosmos.tests.Models
{
    public class FakeSearchableEntity
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
		/// A value to test complex recursive nesting.
		/// </summary>
		[Searchable("complex")]
		public FakeSearchableEntityTwo ComplexSearchable { get; set; }
	}
}