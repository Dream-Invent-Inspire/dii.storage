using dii.storage.cosmos.Exceptions;
using dii.storage.Exceptions;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
	/// <summary>
	/// The CosmosDB implementation of <see cref="DiiContext"/>.
	/// </summary>
	public class DiiCosmosContext : DiiContext
	{
		#region Private Fields
		private static DiiCosmosContext _instance;
		private static readonly object _instanceLock = new object();
		private static bool _hasReadWriteClient;
        private static bool _hasReadOnlyClient;
        #endregion Private Fields

        #region Public Fields
        /// <inheritdoc cref="CosmosClient" />
        public readonly CosmosClient ReadWriteClient;

        /// <inheritdoc cref="CosmosClient" />
        public readonly CosmosClient ReadOnlyClient;
		#endregion Public Fields

		#region Public Properties
		/// <inheritdoc cref="Database" />
		public Dictionary<string, Database> Dbs { get; private set; } = new Dictionary<string, Database>();

		/// <inheritdoc cref="DatabaseProperties" />
		public Dictionary<string, DatabaseProperties> DbProperties { get; private set; } = new Dictionary<string, DatabaseProperties>();
        #endregion Public Properties

        #region Constructors
        /// <summary>
        /// Internally used to create an instance of the CosmosDB implementation of <see cref="DiiContext"/>.
        /// </summary>
        /// <param name="config">The <see cref="INoSqlDatabaseConfig"/> to be used when this context is initialized.</param>
        /// <param name="cosmosClientOptions">Optional settings to be used when this context is initialized.</param>
        private DiiCosmosContext(INoSqlDatabaseConfig config, CosmosClientOptions cosmosClientOptions = null)
        {
            Config = config;

			if (Config.DatabaseConfig != null)
            {
                ReadWriteClient = new CosmosClient(Config.Uri, Config.DatabaseConfig.Key, cosmosClientOptions);
				_hasReadWriteClient = true;
            }

            if (Config.ReadOnlyDatabaseConfig != null)
            {
                ReadOnlyClient = new CosmosClient(Config.Uri, Config.ReadOnlyDatabaseConfig.Key, cosmosClientOptions);
				_hasReadOnlyClient = true;
            }
        }
		#endregion Constructors

		#region Public Methods
		/// <summary>
		/// Creates a singleton of the CosmosDB implementation of <see cref="DiiContext"/>.
		/// </summary>
		/// <param name="config">The <see cref="INoSqlDatabaseConfig"/> to be used when this context is initialized.</param>
		/// <param name="cosmosClientOptions">Optional settings to be used when this context is initialized.</param>
		/// <returns>
		/// The instance of the <see cref="DiiCosmosContext"/>.
		/// </returns>
		public static DiiCosmosContext Init(INoSqlDatabaseConfig config, CosmosClientOptions cosmosClientOptions = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"An {nameof(INoSqlDatabaseConfig)} is required to initialize an instance of {nameof(DiiCosmosContext)}.");
            }

            if (_instance == null)
			{
				lock (_instanceLock)
				{
					if (_instance == null)
					{
						_instance = new DiiCosmosContext(config, cosmosClientOptions);
					}
				}
			}

			return _instance;
		}

		/// <summary>
		/// Returns the singleton of the CosmosDB implementation of <see cref="DiiContext"/>.
		/// </summary>
		/// <exception cref="DiiNotInitializedException">
		/// The <see cref="DiiCosmosContext"/> has not been initialized yet.
		/// </exception>
		/// <returns>
		/// The instance of <see cref="DiiCosmosContext"/> or throws an exception if it does not exist.
		/// </returns>
		public static DiiCosmosContext Get()
		{
			if (_instance == null)
			{
				throw new DiiNotInitializedException(nameof(DiiCosmosContext));
			}

			return _instance;
		}

		/// <summary>
		/// Checks if the database exists. When <see cref="DatabaseConfig.AutoCreate"/> is <see langword="true" />,
		/// creates the database when it does not exist.
		/// </summary>
		/// <returns>
		/// Should always return true or throw an exception.
		/// </returns>
		public override async Task<bool> DoesDatabaseExistAsync()
		{
			var allDatabaseIds = new List<string>();

            if (_hasReadOnlyClient)
            {
				if (Config.ReadOnlyDatabaseConfig.DatabaseIds == null || !Config.ReadOnlyDatabaseConfig.DatabaseIds.Any())
				{
					throw new DiiDatabaseIdNotInConfigurationException();
				}

                allDatabaseIds.AddRange(Config.ReadOnlyDatabaseConfig.DatabaseIds);
            }

            if (_hasReadWriteClient && Config.DatabaseConfig.AutoCreate)
            {
                if (Config.DatabaseConfig.DatabaseIds == null || !Config.DatabaseConfig.DatabaseIds.Any())
                {
                    throw new DiiDatabaseIdNotInConfigurationException();
                }

                allDatabaseIds.AddRange(Config.DatabaseConfig.DatabaseIds);

				if (allDatabaseIds.GroupBy(x => x).Any(x => x.Count() > 1))
				{
					throw new DiiDuplicateDatabaseIdInConfigurationException();
				}

                var throughputProperties = Config.DatabaseConfig.AutoScaling ?
							ThroughputProperties.CreateAutoscaleThroughput(Config.DatabaseConfig.MaxRUPerSecond)
							: ThroughputProperties.CreateManualThroughput(Config.DatabaseConfig.MaxRUPerSecond);

				foreach (var databaseId in Config.DatabaseConfig.DatabaseIds)
				{
					var response = await ReadWriteClient.CreateDatabaseIfNotExistsAsync(databaseId, throughputProperties).ConfigureAwait(false);

					_ = Dbs.TryAdd(databaseId, response.Database);
                    _ = DbProperties.TryAdd(databaseId, response.Resource);

					if (response.StatusCode == HttpStatusCode.Created)
					{
						DatabasesCreatedThisContext.Add(databaseId);
                    }
                }

                // Skip throughput check if all DB's were just created.
                if (DatabasesCreatedThisContext.Any())
				{
					foreach (var databaseId in Config.DatabaseConfig.DatabaseIds)
                    {
                        _ = DbThroughputs.TryAdd(databaseId, Config.DatabaseConfig.MaxRUPerSecond);
                    }
				}
				else
                {
					if (Config.DatabaseConfig.AutoAdjustMaxRUPerSecond)
                    {
						foreach (var db in Dbs)
                        {
                            var checkCurrentThroughputProperties = await db.Value.ReadThroughputAsync().ConfigureAwait(false);

                            if (checkCurrentThroughputProperties.HasValue && checkCurrentThroughputProperties.Value != Config.DatabaseConfig.MaxRUPerSecond)
                            {
                                // Attempt to modify the Max RU/sec.
                                _ = await db.Value.ReplaceThroughputAsync(throughputProperties).ConfigureAwait(false);

                                // Future: Potentially allow changing of the AutoScaling configuration.
                                // https://stackoverflow.com/a/63679119
                            }
                        }
					}
                }
			}

			if (_hasReadWriteClient && (Dbs == null || !Dbs.Any()))
			{
				foreach (var databaseId in allDatabaseIds)
                {
					var db = ReadWriteClient.GetDatabase(databaseId);
                    _ = Dbs.TryAdd(databaseId, db);

                    var dbTask = db.ReadAsync();
                    var throughputTask = db.ReadThroughputAsync();

                    await Task.WhenAll(dbTask, throughputTask).ConfigureAwait(false);

                    var dbResponse = dbTask.Result;

                    _ = DbThroughputs.TryAdd(databaseId, throughputTask.Result ?? -1);
                    _ = DbProperties.TryAdd(databaseId, dbResponse.Resource);
                }
			}

			// Database already existed.
			if (!DatabasesCreatedThisContext.Any() && !DbThroughputs.Any())
            {
                foreach (var db in Dbs)
                {
					var dbThroughput = await db.Value.ReadThroughputAsync().ConfigureAwait(false);

                    _ = DbThroughputs.TryAdd(db.Key, dbThroughput ?? -1);
                }
			}

			return true;
		}

        /// <summary>
        /// Initializes the CosmosDB containers using the provided <see cref="TableMetaData"/> and database id.
        /// </summary>
        /// <param name="databaseId">The databaseId associated with the tables to initialize.</param>
        /// <param name="tableMetaDatas">The <see cref="TableMetaData"/> generated by the <see cref="Optimizer"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// An invalid list of <see cref="TableMetaData"/> was provided.
        /// </exception>
        /// <exception cref="DiiNotInitializedException">
        /// The <see cref="Dbs"/> has not been initialized.
        /// </exception>
        /// <exception cref="DiiTableCreationFailedException">
        /// One or more tables failed to create.
        /// </exception>
        public override async Task InitTablesAsync(string databaseId, ICollection<TableMetaData> tableMetaDatas)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentNullException(nameof(databaseId));
            }

            if (tableMetaDatas == null || !tableMetaDatas.Where(x => x != null).Any())
            {
				throw new ArgumentNullException(nameof(tableMetaDatas));
			}

			if (Dbs == null || !Dbs.ContainsKey(databaseId))
			{
				throw new DiiNotInitializedException(nameof(Dbs));
			}

			if (_hasReadWriteClient && Config.DatabaseConfig.AutoCreate)
			{
				var tasks = new List<Task<ContainerResponse>>();

				foreach (var tableMetaData in tableMetaDatas)
				{
					tasks.Add(Dbs[databaseId].CreateContainerIfNotExistsAsync(new ContainerProperties(tableMetaData.TableName, tableMetaData.PartitionKeyPath)));
				}

				await Task.WhenAll(tasks).ConfigureAwait(false);

				var containers = tasks.Select(x => x.Result.Container).ToDictionary(x => x.Id, x => x);

				foreach (var tableMetaData in tableMetaDatas)
				{
					if (!containers.ContainsKey(tableMetaData.TableName))
					{
						throw new DiiTableCreationFailedException(tableMetaData.TableName);
					}
				}
			}

			_ = TableMappings.TryAdd(databaseId, tableMetaDatas.ToDictionary(x => x.ConcreteType, x => x));
		}
		#endregion Public Methods
	}
}