using System;

namespace dii.storage.cosmos.Exceptions
{
    /// <summary>
    /// Represents errors that occur when any database ids are not present in the configuration.
    /// </summary>
    public class DiiDatabaseIdNotInConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiDatabaseIdNotInConfigurationException"/> class.
        /// </summary>
        public DiiDatabaseIdNotInConfigurationException()
            : base("DatabaseIds are required for configuration.")
        {
        }
    }
}