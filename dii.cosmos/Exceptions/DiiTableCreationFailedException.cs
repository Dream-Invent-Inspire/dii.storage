using System;

namespace dii.cosmos.Exceptions
{
    public class DiiTableCreationFailedException : Exception
    {
        public DiiTableCreationFailedException(string tableName)
            : base($"Table {tableName} could not be created or was not found.")
        {

        }
    }
}