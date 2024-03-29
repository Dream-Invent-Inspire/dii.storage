﻿using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using System;

namespace dii.storage.tests.Models
{
    public abstract class FakeDiiEntity : IDiiEntity
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