using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when the compress order is negative
    /// with the name of the property it was applied to.
    /// </summary>
    public class DiiNegativeCompressOrderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiNegativeCompressOrderException"/> class.
        /// </summary>
        /// <param name="propertyName">The property name which the invalid order was applied.</param>
        public DiiNegativeCompressOrderException(string propertyName)
            : base($"The [Compress(order)] order on {propertyName} cannot be negative.")
        {
        }
    }
}