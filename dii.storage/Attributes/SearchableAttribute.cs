using dii.storage.Exceptions;
using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes that a field or proprety should be indexed and searchable from the
    /// data store.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SearchableAttribute : DiiBaseAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SearchableAttribute"/> class with the
		/// abbreviation value with which the instance should be initalized.
		/// </summary>
		/// <param name="abbreviation">Used to shorten a long display name to reduce stored document size.</param>
		/// <param name="propertyName">The name of the property this is applied to. Auto-captured and should be left blank.</param>
		/// <exception cref="DiiNullSearchableKeyException">
		/// The <see cref="Abbreviation"/> is null, empty or whitespace.
		/// </exception>
		public SearchableAttribute(string abbreviation, [CallerMemberName] string propertyName = "")
		{
			if (string.IsNullOrWhiteSpace(abbreviation))
            {
				throw new DiiNullSearchableKeyException(propertyName);
            }

			Abbreviation = abbreviation;
		}

		/// <summary>
		/// Used to shorten a long display name to reduce stored document size.
		/// <para>Cannot contain the following reserved values: 
		/// [<inheritdoc cref="Constants.ReservedCompressedKey" path="//value"/>, 
		/// <inheritdoc cref="Constants.ReservedPartitionKeyKey" path="//value"/>, 
		/// <inheritdoc cref="Constants.ReservedIdKey" path="//value"/>, 
		/// <inheritdoc cref="Constants.ReservedSchemaVersionKey" path="//value"/>]
		/// </para>
		/// </summary>
		public string Abbreviation { get; init; }

        public string Group { get; init; }

        public override CustomAttributeBuilder GetConstructorBuilder()
        {
            // Getting the right constructor
            var ctor = typeof(SearchableAttribute).GetConstructor(new[] { typeof(string), typeof(string) });

            // Creating the CustomAttributeBuilder with extracted values
            var attributeBuilder = new CustomAttributeBuilder(
                ctor,
                new object[] { this.Abbreviation, "" }
            );
            return attributeBuilder;
        }
    }
}