namespace dii.storage.Models.Interfaces
{
	/// <summary>
	/// The configuration necessary to create and connect to NoSQL databases.
	/// </summary>
	public interface INoSqlDatabaseConfig
	{
		/// <summary>
		/// The uri of the database.
		/// </summary>
		string Uri { get; set; }

		/// <summary>
		/// The read/write access database configuration.
		/// </summary>
		DatabaseConfig DatabaseConfig { get; set; }

        /// <summary>
        /// The read-only access database configuration.
        /// </summary>
        ReadOnlyDatabaseConfig ReadOnlyDatabaseConfig { get; set; }
	}
}