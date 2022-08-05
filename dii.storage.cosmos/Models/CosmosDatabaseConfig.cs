using dii.storage.Models.Interfaces;

namespace dii.storage.cosmos.Models
{
    /// <summary>
    /// The configuration necessary to create and connect to a CosmosDB database.
    /// </summary>
    public class CosmosDatabaseConfig : INoSqlDatabaseConfig
	{
		/// <inheritdoc/>
		public string Uri { get; set; }

		/// <inheritdoc/>
		public string Key { get; set; }

		/// <inheritdoc/>
		public string DatabaseId { get; set; }

		/// <inheritdoc/>
		public bool AutoCreate { get; set; }

		/// <inheritdoc/>
		public int MaxRUPerSecond { get; set; }

		/// <inheritdoc/>
		public bool AutoAdjustMaxRUPerSecond { get; set; }

		/// <inheritdoc/>
		/// <remarks>
		/// This must be modified via Azure Portal once the database is created. Attempting to 
		/// toggle autoscale once the database exists via dii.storage will cause
		/// an exception to be thrown.
		/// </remarks>
		public bool AutoScaling { get; set; }
	}
}