using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when the searchable keyword is null, empty or whitespace
    /// with the name of the property it was applied to.
    /// </summary>
    public class DiiNullSearchableKeyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiNullSearchableKeyException"/> class.
        /// </summary>
        /// <param name="propertyName">The property name which the invalid key was applied.</param>
        public DiiNullSearchableKeyException(string propertyName)
            : base($"The [Searchable(key)] key on {propertyName} cannot be null, empty or whitespace.")
        {
        }
    }
}