using dii.storage.Models.Interfaces;

namespace dii.storage.cosmos.Models
{
    /// <summary>
    /// The configuration necessary to create and connect to a CosmosDB.
    /// </summary>
    public class CosmosDatabaseConfig : INoSqlDatabaseConfig
	{
		public string Uri { get; set; }

		public string Key { get; set; }

		public string DatabaseId { get; set; }

		public bool AutoCreate { get; set; }

		public int MaxRUPerSecond { get; set; }

		public bool AutoAdjustMaxRUPerSecond { get; set; }

		public bool AutoScaling { get; set; }

		public bool? AllowBulkExecution { get; set; }
	}
}