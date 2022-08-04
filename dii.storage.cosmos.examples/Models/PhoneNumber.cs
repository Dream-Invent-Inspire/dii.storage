using dii.storage.Attributes;

namespace dii.storage.cosmos.examples.Models
{
    public class PhoneNumber
	{
		/// <summary>
		/// The full phone number of the <see cref="PhoneNumber"/>.
		/// </summary>
		[Searchable("number")]
		public string FullPhoneNumber { get; set; }

		/// <summary>
		/// Other data for the <see cref="PhoneNumber"/> that does not need to be searchable via queries.
		/// </summary>
		[Compress(0)]
		public string OtherData { get; set; }
	}
}