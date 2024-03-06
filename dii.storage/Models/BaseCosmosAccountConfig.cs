using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Models
{
    /// <summary>
    /// This exists so that .net core knows how to load config data from a configuration file like apsettings.json
    /// </summary>

    public class BaseCosmosAccountConfig : INoSqlAccountConfig
    {
        public List<BaseCosmosContextConfig> CosmosStorageAccounts { get; set; } = new List<BaseCosmosContextConfig>();

        List<INoSqlContextConfig> INoSqlAccountConfig.CosmosStorageAccounts
        {
            get
            {
                return this.CosmosStorageAccounts.Select(x => x as INoSqlContextConfig).ToList();
            }
            set
            {
                this.CosmosStorageAccounts = value.Select(x => x as BaseCosmosContextConfig).ToList();
            }
        }
    }

    public class BaseCosmosContextConfig : INoSqlContextConfig
    {
        /// <inheritdoc/>
        public string Uri { get; set; }
        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public List<BaseCosmosDatabaseConfig> CosmosStorageDBs { get; set; } = new List<BaseCosmosDatabaseConfig>();
        List<INoSqlDatabaseConfig> INoSqlContextConfig.CosmosStorageDBs
        {
            get
            {
                return this.CosmosStorageDBs.Select(x => x as INoSqlDatabaseConfig).ToList();
            }
            set 
            {
                this.CosmosStorageDBs = value.Select(x => x as BaseCosmosDatabaseConfig).ToList();
            }
        }
    }
    public class BaseCosmosDatabaseConfig : INoSqlDatabaseConfig
    {
        public string DatabaseId { get; set; }
        public bool AutoCreate { get; set; } = true;
        public int MaxRUPerSecond { get; set; } = 4000;
        public bool AutoAdjustMaxRUPerSecond { get; set; } = true;
        public bool AutoScaling { get; set; } = true;
        public string TypeCollectionName { get; set; }
    }
}
