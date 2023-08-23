using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Models
{
    public class DiiBasicEntity : IDiiEntity
    {
        public DiiBasicEntity()
        {
        }

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
