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
		/// The dynamically created type created by the <see cref="Optimizer"/>.
		/// </summary>
		public Type StorageType { get; set; }

		/// <summary>
		/// The concrete type. This is the user-defined object that is passed into the <see cref="Optimizer"/>
		/// to be registered.
		/// </summary>
		public Type ConcreteType { get; set; }
	}
}