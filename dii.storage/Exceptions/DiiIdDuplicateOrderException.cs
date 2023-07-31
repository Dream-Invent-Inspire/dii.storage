using dii.storage.Attributes;
using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when more than one <see cref="IdAttribute"/> is used
    /// on the same object and not all <see cref="IdAttribute.Order"/> are unique.
    /// </summary>
    public class DiiIdDuplicateOrderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiIdDuplicateOrderException"/> class.
        /// </summary>
        /// <param name="propertyName">The property name which the first instance of the duplicate value was found.</param>
        /// <param name="duplicatePropertyName">The property name which the second instance of the duplicate value was found.</param>
        /// <param name="order">The duplicate value.</param>
        public DiiIdDuplicateOrderException(string propertyName, string duplicatePropertyName, int order)
            : base($"The [Id(order)] order [{order}] on {propertyName} is the same as the order [{order}] on {duplicatePropertyName}.")
        {
        }
    }
}