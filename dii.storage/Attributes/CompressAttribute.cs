using System;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes that a Field or Proprety should be compressed and does not need to be
    /// searchable from the data store.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class CompressAttribute : Attribute
	{
		public CompressAttribute(int order)
		{
			Order = order;
		}

		/// <summary>
		/// The immutable order which the property should be stored and compressed.
		/// </summary>
		public int Order { get; set; }
	}
}