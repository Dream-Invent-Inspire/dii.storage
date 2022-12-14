using System;

namespace dii.storage.cosmos.Exceptions
{
    /// <summary>
    /// Represents errors that occur when any database ids are duplicated in the configuration.
    /// </summary>
    public class DiiDuplicateDatabaseIdInConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiDuplicateDatabaseIdInConfigurationException"/> class.
        /// </summary>
        public DiiDuplicateDatabaseIdInConfigurationException()
            : base("One or more databaseIds are specified in the configuration more than once.")
        {
        }
    }
}