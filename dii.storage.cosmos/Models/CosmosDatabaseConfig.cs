using dii.storage.Models;
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
        public DatabaseConfig DatabaseConfig { get; set; }

        /// <inheritdoc/>
        public ReadOnlyDatabaseConfig ReadOnlyDatabaseConfig { get; set; }
	}
}