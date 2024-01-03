using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using System;

namespace dii.storage.tests.Models
{
    public abstract class FakeDiiEntity : IDiiEntity
	{
        /// <inheritdoc/>
        //[Searchable(Constants.ReservedSchemaVersionKey)]
        //[IgnoreMember]
        //public Version SchemaVersion => new(1, 0);

        private Version _schemaVersion = new Version(1, 0);

        [IgnoreMember]
        public Version SchemaVersion
        {
            get { return _schemaVersion; }
            set { _schemaVersion = value; }
        }


        /// <inheritdoc/>
        [Searchable("_etag")]
		public string DataVersion { get; set; }
	}
}