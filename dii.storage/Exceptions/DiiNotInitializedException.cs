using System;

namespace dii.storage.Exceptions
{
    public class DiiNotInitializedException : Exception
    {
        public DiiNotInitializedException(string objectName)
            : base($"{objectName} not initialized.")
        {

        }
    }
}