using dii.cosmos.Attributes;

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
		/// A <see cref="string"/> value to be searched..
		/// </summary>
		[Searchable("xsoaps")]
		public string Soaps { get; set; }

		/// <summary>
		/// A value to test complex recursive nesting.
		/// </summary>
		[Searchable("xnesting")]
		public FakeSearchableEntity Nesting { get; set; }
	}
}