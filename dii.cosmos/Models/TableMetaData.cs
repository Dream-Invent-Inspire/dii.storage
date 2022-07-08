using System;
using System.Collections.Generic;

namespace dii.cosmos.Models
{
    public class TableMetaData
	{
		public string TableName { get; set; }
		public List<string> UniqueProperties { get; set; }
		public string PartitionKeyPath { get; } = $"/{Constants.ReservedPartitionKeyKey}";
		public Type StorageType { get; set; }
		public Type ConcreteType { get; set; }
	}
}