using dii.cosmos.Attributes;
using dii.cosmos.Models.Interfaces;

namespace dii.cosmos.Models
{
    public abstract class DiiCosmosEntity : IDiiCosmosEntity
	{
		/// <summary>
		/// The computed version of the stored object.
		/// Necessary for optimistic concurrency.
		/// </summary>
		[Searchable("_etag")]
		public string Version { get; set; }
	}
}