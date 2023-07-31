using dii.storage.Attributes;

namespace dii.storage.cosmos.examples.Models
{
    public class Address
	{
		/// <summary>
		/// The <see cref="Address"/> zip code.
		/// </summary>
		[Searchable("zip")]
		public string ZipCode { get; set; }

		/// <summary>
		/// Other data for the <see cref="Address"/> that does not need to be searchable via queries.
		/// </summary>
		[Compress(0)]
		public string OtherData { get; set; }

		/// <summary>
		/// The <see cref="PhoneNumber"/>.
		/// </summary>
		[Searchable("phone")]
		public PhoneNumber PhoneNumber { get; set; }
	}
}