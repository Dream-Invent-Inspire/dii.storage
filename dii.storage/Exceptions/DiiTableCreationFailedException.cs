using System;

namespace dii.storage.Exceptions
{
    public class DiiTableCreationFailedException : Exception
    {
        public DiiTableCreationFailedException(string tableName)
            : base($"Table {tableName} could not be created or was not found.")
        {

        }
    }
}