using dii.storage.Attributes;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dii.storage.Models
{
	/// <summary>
	/// Represents a mapping between a concrete object and an emit object as understood by the <see cref="Optimizer"/>.
	/// This class cannot be inherited.
	/// </summary>
	public sealed class TableMetaData
    {
        /// <summary>
        /// The name of the table in storage.
        /// </summary>
		/// <remarks>
		/// This value is derived from the class or struct name unless the <see cref="StorageNameAttribute" /> is present.
		/// </remarks>
        public string TableName { get; set; }

		/// <summary>
		/// The name of the class.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// A collection of unique properties.
		/// </summary>
		/// <remarks>
		/// Currently not implemented.
		/// </remarks>
		public List<string> UniqueProperties { get; init; } = null;

		/// <summary>
		/// The searchable path of the partition key.
		/// </summary>
		/// <remarks>
		/// Currently hardcoded as /PK.
		/// </remarks>
		public string PartitionKeyPath { get; init; } = $"/{Constants.ReservedPartitionKeyKey}";

        /// <summary>
        /// The searchable path of the partition key.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public Dictionary<int, PropertyInfo> HierarchicalPartitionKeys { get; set; } = new Dictionary<int, PropertyInfo>();

        /// <summary>
		/// Properties composing the CosmosDB Id
		/// </summary>
		public Dictionary<int, PropertyInfo> IdProperties { get; set; } = new Dictionary<int, PropertyInfo>();

        /// <summary>
		/// Separator used when multiple Id properties
		/// </summary>
		public string IdSeparator { get; set; }

        /// <summary>
        /// The searchable path of the Lookup container partition key.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public Dictionary<int, PropertyInfo> LookupHpks { get; set; } = new Dictionary<int, PropertyInfo>();

        /// <summary>
        /// The searchable path of the Lookup container id key.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public Dictionary<int, PropertyInfo> LookupIds { get; set; } = new Dictionary<int, PropertyInfo>();

        /// <summary>
        /// The searchable path of the search fields.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public List<PropertyInfo> SearchableFields { get; set; } = new List<PropertyInfo>();


        /// <summary>
        /// The dynamically created type created by the <see cref="Optimizer"/>.
        /// </summary>
        public Type StorageType { get; set; }

		/// <summary>
		/// The concrete type. This is the user-defined object that is passed into the <see cref="Optimizer"/>
		/// to be registered.
		/// </summary>
		public Type ConcreteType { get; set; }

        /// <summary>
        /// The time to live in seconds value of the table in storage.
        /// </summary>
		/// <remarks>
		/// The unit of measurement is seconds. The maximum allowed value is 2147483647. A valid value must be either a nonzero positive integer, '-1' or null.
		/// By default, DefaultTimeToLive is set to null meaning the time to live is turned off for the container.
		/// <para>
		/// This value is set by using the <see cref="EnableTimeToLiveAttribute" /> to a non-null, nonzero value.
		/// </para>
		/// </remarks>
        public int? TimeToLiveInSeconds { get; set; }

		/// <summary>
		/// The database for this table.
		/// 
		public string DbId { get; set; }


		public bool Initialized { get; set; } = false;
        public bool IsLookupTable { get; internal set; }
		public string SourceTableNameForLookup { get; internal set; }
        public Type SourceTableTypeForLookup { get; internal set; }

		public Container LookupContainer { get; set; } 
		public Type LookupType { get; set;}
		public int? DefaultPageSize { get; set; } = 10;

        public bool HasLookup()
		{
			return this.LookupIds?.Count > 0;
		}
    }
}