using System;

namespace dii.cosmos.Attributes
{
    /// <summary>
    /// Denotes the partition key that the entity belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PartitionKeyAttribute : Attribute
	{
		public PartitionKeyAttribute(Type parititonKeyType = null, int order = 0, char separator = Constants.DefaultPartitionDelimitor)
		{
			Order = order;
			Separator = separator;
			PartitionKeyType = parititonKeyType ?? typeof(string);
		}

		public int Order { get; init; }
		public char Separator { get; init; }
		public Type PartitionKeyType { get; init; }
	}
}