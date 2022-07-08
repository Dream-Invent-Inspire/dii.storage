using dii.cosmos.Attributes;
using dii.cosmos.Models.Interfaces;
using MessagePack;
using System;

namespace dii.cosmos.Models
{
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