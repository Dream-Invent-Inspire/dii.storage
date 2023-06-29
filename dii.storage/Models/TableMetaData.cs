using dii.storage.Attributes;
using System;
using System.Collections.Generic;

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
        /// Currently hardcoded as /PK.
        /// </remarks>
        public Dictionary<int, string> HierarchicalPartitionKeys { get; set; } = new Dictionary<int, string>();

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
    }
}