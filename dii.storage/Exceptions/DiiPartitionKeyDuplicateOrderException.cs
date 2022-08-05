using dii.storage.Attributes;
using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when more than one <see cref="PartitionKeyAttribute"/> is used
    /// on the same object and not all <see cref="PartitionKeyAttribute.Order"/> are unique.
    /// </summary>
    public class DiiPartitionKeyDuplicateOrderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiPartitionKeyDuplicateOrderException"/> class.
        /// </summary>
        /// <param name="propertyName">The property name which the first instance of the duplicate value was found.</param>
        /// <param name="duplicatePropertyName">The property name which the second instance of the duplicate value was found.</param>
        /// <param name="order">The duplicate value.</param>
        public DiiPartitionKeyDuplicateOrderException(string propertyName, string duplicatePropertyName, int order)
            : base($"The [PartitionKey(order)] order [{order}] on {propertyName} is the same as the order [{order}] on {duplicatePropertyName}.")
        {
        }
    }
}