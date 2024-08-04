using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using System.Collections.Generic;

namespace dii.storage.cosmos.examples.Models
{
    public class ExampleConfig : CosmosContextConfig
    {
        public const string DbName = $"dii-storage-cosmos-example-dj";
        public ExampleConfig()
        {
            Uri = "https://localhost:8081";
            Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            //CosmosStorageDBs = new List<INoSqlDatabaseConfig>();
            CosmosStorageDBs.Add(new CosmosDatabaseConfig
            {
                // Max RU/sec management should really be left to Azure Portal or automation.
                // Use this setting at your own risk.
                AutoAdjustMaxRUPerSecond = true,
                AutoCreate = true,
                // This should never be changed (via dii.storage) after the database is created.
                // Use Azure Portal if you must change this property.
                // After it has been successfully changed in Azure Portal, change this value to 
                // reflect the current setting.
                AutoScaling = true,
                DatabaseId = DbName,
                MaxRUPerSecond = 4000
            });
        }
    }
}