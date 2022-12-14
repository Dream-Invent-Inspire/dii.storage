using System;

namespace dii.storage.cosmos.Exceptions
{
    /// <summary>
    /// Represents errors that occur when a method call is made requiring a read/write connection
    /// but exists on a read-only connection.
    /// </summary>
    public class DiiReadOnlyConnectionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiReadOnlyConnectionException"/> class with the
        /// name of the method that causes this exception.
        /// </summary>
        /// <param name="methodName">The name of the method that caused the exception.</param>
        public DiiReadOnlyConnectionException(string methodName)
            : base($"{methodName} is not available on a Read-Only connection.")
        {
        }
    }
}