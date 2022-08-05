using dii.storage.Exceptions;
using System;
using System.Runtime.CompilerServices;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes that a field or proprety should be compressed and does not need to be
    /// searchable from the data store.
	/// <para>
	/// <see cref="Order"/> cannot be negative.
	/// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class CompressAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompressAttribute"/> class with the
		/// order value with which the instance should be initalized.
		/// </summary>
		/// <param name="order">The immutable order which the field or proprety should be stored and compressed.</param>
		/// <param name="propertyName">The name of the property this is applied to. Auto-captured and should be left blank.</param>
		/// <exception cref="DiiNegativeCompressOrderException">
		/// The <see cref="Order"/> is negative.
		/// </exception>
		public CompressAttribute(int order, [CallerMemberName] string propertyName = "")
		{
			if (order < 0)
            {
				throw new DiiNegativeCompressOrderException(propertyName);
			}

			Order = order;
		}

		/// <summary>
		/// The immutable order which the field or proprety should be stored and compressed.
		/// <para>
		/// This value should not change once the object has been used in storage.
		/// Changing it will cause data to be corrupt.
		/// </para>
		/// </summary>
		public int Order { get; init; }
	}
}