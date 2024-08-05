using System.Collections.Generic;
using System.Linq;

namespace dii.storage.Models.Interfaces
{
	public interface INoSqlAccountConfig
	{
        List<INoSqlContextConfig> CosmosStorageAccounts { get; set; }
    }

	/// <summary>
	/// The configuration necessary to create and connect to a NoSQL database.
	/// </summary>
	/// 
	public interface INoSqlContextConfig
	{
        /// <summary>
        /// The uri of the database.
        /// </summary>
        string Uri { get; set; }

        /// <summary>
        /// The key for the Uri Connection.
        /// </summary>
        string Key { get; set; }

        List<INoSqlDatabaseConfig> CosmosStorageDBs { get; set; }
	}
	public interface INoSqlDatabaseConfig
	{
        /// <summary>
        /// The ids of the databases.
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// Auto-creates the database and/or containers if they do not exist.
        /// </summary>
        bool AutoCreate { get; set; }

		/// <summary>
		/// The max RU/sec.
		/// </summary>
		int MaxRUPerSecond { get; set; }

		/// <summary>
		/// When true, the <see cref="DiiContext"/> should attempt to change
		/// the existing database RU/sec throughput provisioning if it is different
		/// than what is set in <see cref="MaxRUPerSecond"/>.
		/// </summary>
		bool AutoAdjustMaxRUPerSecond { get; set; }

		/// <summary>
		/// When true, RU throughput provisioning will be set to autoscale. 
		/// </summary>
		bool AutoScaling { get; set; }

        public bool IsServerless { get; set; }
    }
}