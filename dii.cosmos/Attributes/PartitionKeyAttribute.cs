using System;

namespace dii.cosmos.Attributes
{
    /// <summary>
    /// Denotes the partition key that the entity belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PartitionKeyAttribute : Attribute
	{
		public PartitionKeyAttribute(int order = 0, char separator = Constants.DefaultPartitionDelimitor)
		{
			Order = order;
			Separator = separator;
		}

		public int Order { get; set; }
		public char Separator { get; set; }
	}
}