using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when a Dii object is not initialized.
    /// </summary>
    public class DiiNotInitializedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiNotInitializedException"/> class with the
        /// name of the Dii object that causes this exception.
        /// </summary>
        /// <param name="objectName">The name of the object that caused the exception.</param>
        public DiiNotInitializedException(string objectName)
            : base($"{objectName} not initialized.")
        {
        }
    }
}