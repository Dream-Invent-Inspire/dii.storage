using System;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes the id that the entity belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class IdAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IdAttribute"/> class with the
		/// order and separator values with which the instance should be initalized.
		/// </summary>
		/// <param name="order">The immutable order which the field or property should be used to form the composite id.</param>
		/// <param name="separator">The immutable separator to be used when forming the composite id.</param>
		public IdAttribute(int order = 0, char separator = Constants.DefaultIdDelimitor)
		{
			Order = order;
			Separator = char.IsWhiteSpace(separator) ? Constants.DefaultIdDelimitor : separator;
		}

		/// <summary>
		/// Required when more than one field or property is designated as
		/// part of the id. The immutable order which the field or property should be used to form
		/// the composite id.
		/// <para>Defaults to 0</para>
		/// </summary>
		/// <remarks>
		/// Order is optional if only one <see cref="IdAttribute"/> is used within the entity.
		/// </remarks>
		public int Order { get; init; }

		/// <summary>
		/// Optional parameter used when more than one field or property is designated as
		/// part of the id. The immutable separator to be used when forming the composite id.
		/// <para>
		/// Defaults to <inheritdoc cref="Constants.DefaultIdDelimitor" path="//value"/>
		/// </para>
		/// </summary>
		public char Separator { get; init; }
	}
}