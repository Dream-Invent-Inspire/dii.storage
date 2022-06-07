using System;

namespace dii.cosmos.Attributes
{
    /// <summary>
    /// Denotes that a Field or Proprety should be indexed and searchable from the
    /// data store.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SearchableAttribute : Attribute
	{
		public SearchableAttribute(string abbreviation)
		{
			Abbreviation = abbreviation;
		}

		/// <summary>
		/// Used to shorten a long display name to reduce stored document size.
		/// </summary>
		public string Abbreviation { get; set; }
	}
}