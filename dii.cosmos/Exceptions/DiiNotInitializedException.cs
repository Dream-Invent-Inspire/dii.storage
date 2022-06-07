using System;

namespace dii.cosmos.Exceptions
{
    public class DiiNotInitializedException : Exception
    {
        public DiiNotInitializedException(string objectName)
            : base($"{objectName} not initialized.")
        {

        }
    }
}