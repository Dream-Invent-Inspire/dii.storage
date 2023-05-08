using dii.storage.Attributes;
using MessagePack;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace dii.storage
{
    /// <summary>
    /// A dictionary of constant values used by dii.storage.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="MessagePackObjectAttribute"/> constructor.
        /// </summary>
        public static ConstructorInfo MessagePackAttributeConstructor { get; } = typeof(MessagePackObjectAttribute).GetConstructor(new Type[] { typeof(bool) });

        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="JsonPropertyNameAttribute"/> constructor.
        /// </summary>
        public static ConstructorInfo JsonPropertyNameAttributeConstructor { get; } = typeof(JsonPropertyNameAttribute).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="KeyAttribute"/> constructor.
        /// </summary>
        public static ConstructorInfo CompressKeyAttributeConstructor { get; } = typeof(KeyAttribute).GetConstructor(new Type[] { typeof(int) });

        public static Type StringType { get; } = typeof(string);

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