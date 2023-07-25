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
		#endregion Private Fields

		#region Public Fields
		/// <inheritdoc cref="CosmosClient" />
		public readonly CosmosClient Client;
		#endregion Public Fields

		#region Public Properties

		/// <inheritdoc cref="Database" />
		public List<Database> Dbs { get; private set; }

		/// <inheritdoc cref="DatabaseProperties" />
		public Dictionary<string, DatabaseProperties> DbProperties { get; private set; }

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
			Client = new CosmosClient(Config.Uri, Config.Key, cosmosClientOptions);
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
		/// Checks if the database exists. When <see cref="INoSqlDatabaseConfig.AutoCreate"/> is <see langword="true"/>,
		/// creates the database when it does not exist.
		/// </summary>
		/// <returns>
		/// Should always return true or throw an exception.
		/// </returns>
		public override async Task<bool> DoesDatabaseExistAsync()
		{
			if (Config.AutoCreate)
			{
				var throughputProperties = Config.AutoScaling ?
							ThroughputProperties.CreateAutoscaleThroughput(Config.MaxRUPerSecond)
							: ThroughputProperties.CreateManualThroughput(Config.MaxRUPerSecond);

				foreach (var dbid in Config.DatabaseIds)
				{
					var response = await Client.CreateDatabaseIfNotExistsAsync(dbid, throughputProperties).ConfigureAwait(false);
					if (Dbs == null)
					{
                        Dbs = new List<Database>();
                    }
					Dbs.Add(response.Database);
					if (DbProperties == null)
					{
						DbProperties = new Dictionary<string, DatabaseProperties>();
					}
					DbProperties.Add(dbid, response.Resource);

					// Skip throughput check if DB was just created.
					DatabaseCreatedThisContext = response.StatusCode == HttpStatusCode.Created;

					if (DatabaseCreatedThisContext)
					{
						DbThroughput = Config.MaxRUPerSecond;
					}
					else
					{
						if (Config.AutoAdjustMaxRUPerSecond)
						{
							var checkCurrentThroughputProperties = await response.Database.ReadThroughputAsync().ConfigureAwait(false);

							if (checkCurrentThroughputProperties.HasValue && checkCurrentThroughputProperties.Value != Config.MaxRUPerSecond)
							{
								// Attempt to modify the Max RU/sec.
								_ = await response.Database.ReplaceThroughputAsync(throughputProperties).ConfigureAwait(false);

								// Future: Potentially allow changing of the AutoScaling configuration.
								// https://stackoverflow.com/a/63679119
							}
						}
					}
				}
			}

			if (Dbs == null)
			{
				Dbs = new List<Database>();
				foreach (var dbid in Config.DatabaseIds)
				{
					var db = Client.GetDatabase(dbid);
					Dbs.Add(db);

					var dbTask = db.ReadAsync();
					var throughputTask = db.ReadThroughputAsync();

					await Task.WhenAll(dbTask, throughputTask).ConfigureAwait(false);

					var dbResponse = dbTask.Result;
					DbThroughput = throughputTask.Result ?? -1;
					DbProperties.Add(dbid, dbResponse.Resource);

					// Database already existed.
					if (!DatabaseCreatedThisContext && !DbThroughput.HasValue)
					{
						DbThroughput = await db.ReadThroughputAsync().ConfigureAwait(false) ?? -1;
					}
                }
            }

            return true;
		}

		/// <summary>
		/// Initializes the CosmosDB containers using the provided <see cref="TableMetaData"/>.
		/// </summary>
		/// <param name="tableMetaDatas">The <see cref="TableMetaData"/> generated by the <see cref="Optimizer"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// An invalid list of <see cref="TableMetaData"/> was provided.
		/// </exception>
		/// <exception cref="DiiNotInitializedException">
		/// The <see cref="Db"/> has not been initialized.
		/// </exception>
		/// <exception cref="DiiTableCreationFailedException">
		/// One or more tables failed to create.
		/// </exception>
		public override async Task InitTablesAsync(string dbid, ICollection<TableMetaData> tableMetaDatas)
		{
			if (tableMetaDatas == null || !tableMetaDatas.Where(x => x != null).Any())
            {
				throw new ArgumentNullException(nameof(tableMetaDatas));
			}

			if (Dbs == null)
			{
				throw new DiiNotInitializedException(nameof(Dbs));
			}

			if (Config.AutoCreate)
			{
				var tasks = new List<Task<ContainerResponse>>();

				foreach (var tableMetaData in tableMetaDatas)
				{
                    var containerProperties = new ContainerProperties(tableMetaData.TableName, tableMetaData.PartitionKeyPath)
                    {
                        DefaultTimeToLive = tableMetaData.TimeToLiveInSeconds
                    };

					//if hierarchical partition keys are defined, use them instead
                    if (tableMetaData.HierarchicalPartitionKeys?.Any() ?? false)
					{
						// List of partition keys, in hierarchical order. You can have up to three levels of keys.
						containerProperties = new ContainerProperties(
							id: tableMetaData.TableName,
							partitionKeyPaths: tableMetaData.HierarchicalPartitionKeys.OrderBy(x => x.Key).Select(x => $"/{x.Value}").ToList()
                        )
                        {
                            DefaultTimeToLive = tableMetaData.TimeToLiveInSeconds
                        };					
					}
					var db = Dbs.FirstOrDefault(x => x.Id == dbid);
					if (db == null)
					{
                        throw new DiiNotInitializedException(nameof(Dbs));
                    }
					tasks.Add(db.CreateContainerIfNotExistsAsync(containerProperties));
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

			var curmaps = tableMetaDatas.ToDictionary(x => x.ConcreteType, x => x);
			if (TableMappings == null)
			{
                TableMappings = new Dictionary<Type, TableMetaData>();
            }
			foreach(var tbl in curmaps)
			{
				TableMappings.Add(tbl.Key, tbl.Value);
			}
        }
		#endregion Public Methods
	}
}