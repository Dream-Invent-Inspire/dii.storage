using dii.storage.Models.Interfaces;
using System;

namespace dii.storage.Exceptions
{
    /// <summary>
    /// Represents errors that occur when a property of a <see cref="IDiiEntity"/> object also
    /// implements the <see cref="IDiiEntity"/> interface.
    /// </summary>
    public class DiiInvalidNestingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiiInvalidNestingException"/> class with the
        /// name of the Dii object that causes this exception.
        /// </summary>
        /// <param name="objectName">The name of the object that caused the exception.</param>
        public DiiInvalidNestingException(string objectName)
            : base($"Using an object that implements the {nameof(IDiiEntity)} interface as a property is not currently supported. Failed to initialize {objectName}.")
        {
        }
    }
}