using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using System;

namespace dii.storage.cosmos.Models
{
    /// <inheritdoc/>
    public abstract class DiiCosmosEntity : IDiiEntity
	{
		/// <inheritdoc/>
		//[Searchable(Constants.ReservedSchemaVersionKey)]
		[IgnoreMember]
		public Version SchemaVersion => new(1, 0);

		/// <inheritdoc/>
		[Searchable("_etag")]
		public string DataVersion { get; set; }
    }
}