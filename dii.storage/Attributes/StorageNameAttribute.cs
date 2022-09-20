using System;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes that a class or struct should be stored in the data store with
	/// the name provided.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class StorageNameAttribute : Attribute
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageNameAttribute"/> class with the
        /// storageName value with which the instance should be initalized.
        /// </summary>
        /// <param name="storageName">Used to provide the storage name for like entities. Commonly referred to as a table or container name.</param>
        /// <remarks>
        /// Once the table or container is created, this value should not change.
        /// </remarks>
        public StorageNameAttribute(string storageName)
		{
			Name = storageName;
		}

        /// <summary>
        /// Used to provide the storage name for like entities. Commonly referred to as a table or container name.
        /// </summary>
        /// <remarks>
        /// The provided name must conform to the naming conventions of the storage engine.
        /// </remarks>
        public string Name { get; init; }
	}
}