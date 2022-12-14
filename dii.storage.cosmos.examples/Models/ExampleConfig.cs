using dii.storage.cosmos.Models;
using dii.storage.Models;

namespace dii.storage.cosmos.examples.Models
{
    public class ExampleConfig : CosmosDatabaseConfig
    {
        public ExampleConfig()
        {
            Uri = "https://localhost:8081";
            DatabaseConfig = new DatabaseConfig
            {
                Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                DatabaseIds = new string[1]
                {
                    "dii-storage-cosmos-example-local"
                },
                AutoCreate = true,
                MaxRUPerSecond = 4000,
                // Max RU/sec management should really be left to Azure Portal or automation.
                // Use this setting at your own risk.
                AutoAdjustMaxRUPerSecond = true,
                // This should never be changed (via dii.storage) after the database is created.
                // Use Azure Portal if you must change this property.
                // After it has been successfully changed in Azure Portal, change this value to 
                // reflect the current setting.
                AutoScaling = true,
            };
        }
    }
}