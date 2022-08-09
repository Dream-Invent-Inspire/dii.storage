using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when a reserved searchable keyword is used.
    /// </summary>
    public class DiiReservedSearchableKeyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiReservedSearchableKeyException"/> class
        /// with the key, property name and Dii object that causes this exception.
        /// </summary>
        /// <param name="key">The key that caused the exception.</param>
        /// <param name="propertyName">The name of the property that caused the exception.</param>
        /// <param name="objectName">The name of the object that caused the exception.</param>
        public DiiReservedSearchableKeyException(string key, string propertyName, string objectName)
            : base($"'{key}' is a reserved [Searchable(key)] key and cannot be used for property '{propertyName}' in object '{objectName}'.")
        {
        }
    }
}