using System;

namespace dii.storage.Exceptions
{
	/// <summary>
	/// Represents errors that occur when a type is initialized more than once.
	/// </summary>
	public class DiiDuplicateTypeInitializedException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DiiDuplicateTypeInitializedException"/> class.
		/// </summary>
		/// <param name="type">The type which has already been initialized.</param>
		public DiiDuplicateTypeInitializedException(Type type) 
            : base($"The type {type.FullName} has already been initialized.")
        { }
    }
}