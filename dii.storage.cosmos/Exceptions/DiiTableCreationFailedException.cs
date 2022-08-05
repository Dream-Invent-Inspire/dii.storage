using System;

namespace dii.storage.cosmos.Exceptions
{
    /// <summary>
    /// Represents errors that occur when a Dii table is not created.
    /// </summary>
    public class DiiTableCreationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiTableCreationFailedException"/> class with the
        /// name of the table that causes this exception.
        /// </summary>
        /// <param name="tableName">The name of the table that caused the exception.</param>
        public DiiTableCreationFailedException(string tableName)
            : base($"Table {tableName} could not be created or was not found.")
        {
        }
    }
}