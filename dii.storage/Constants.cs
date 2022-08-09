using dii.storage.Attributes;

namespace dii.storage
{
    public static class Constants
    {
        /// <summary>
        /// A reserved searchable key. Cannot be used as the <see cref="SearchableAttribute.Abbreviation"/>.
        /// </summary>
        /// <value>
        /// p
        /// </value>
        public const string ReservedCompressedKey = "p";

        /// <summary>
        /// A reserved searchable key. Cannot be used as the <see cref="SearchableAttribute.Abbreviation"/>.
        /// </summary>
        /// <value>
        /// PK
        /// </value>
        public const string ReservedPartitionKeyKey = "PK";

        /// <summary>
        /// A reserved searchable key. Cannot be used as the <see cref="SearchableAttribute.Abbreviation"/>.
        /// </summary>
        /// <value>
        /// id
        /// </value>
        public const string ReservedIdKey = "id";

        /// <summary>
        /// A reserved searchable key. Cannot be used as the <see cref="SearchableAttribute.Abbreviation"/>.
        /// </summary>
        /// <value>
        /// _sv
        /// </value>
        public const string ReservedSchemaVersionKey = "_sv";

        /// <summary>
        /// The default delimiter used as the <see cref="PartitionKeyAttribute.Separator"/>.
        /// </summary>
        /// <value>
        /// |
        /// </value>
        public const char DefaultPartitionKeyDelimitor = '|';

        /// <summary>
        /// The default delimiter used as the <see cref="IdAttribute.Separator"/>.
        /// </summary>
        /// <value>
        /// |
        /// </value>
        public const char DefaultIdDelimitor = '|';
    }
}