using System;

namespace dii.cosmos.Exceptions
{
    public class DiiReservedSearchableKeyException : Exception
    {
        public DiiReservedSearchableKeyException(string key, string propertyName, string objectName)
            : base($"'{key}' is a reserved [Searchable(key)] key and cannot be used for property '{propertyName}' in object '{objectName}'.")
        {

        }
    }
}