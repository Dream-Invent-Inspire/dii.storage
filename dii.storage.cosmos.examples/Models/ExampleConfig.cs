using dii.storage.cosmos.Models;
using System.Collections.Generic;

namespace dii.storage.cosmos.examples.Models
{
    public class ExampleConfig : CosmosDatabaseConfig
    {
        public const string DbName = $"dii-storage-cosmos-example-local";
        public ExampleConfig()
        {
            //Uri = "https://localhost:8081";
            //Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            Uri = "https://cosmos-emporos-low.documents.azure.com:443/";
            Key = "brAonLaInaJ8UKOACnidFnckHQv0EvNy42U3812P0tn6mGoOMYO0wSG4zIZsBPUGz32ihJa3kT1VACDbWAAHOw==";

            DatabaseIds = new List<string>() { DbName };
            AutoCreate = true;
            MaxRUPerSecond = 4000;

            // Max RU/sec management should really be left to Azure Portal or automation.
            // Use this setting at your own risk.
            AutoAdjustMaxRUPerSecond = true;

            // This should never be changed (via dii.storage) after the database is created.
            // Use Azure Portal if you must change this property.
            // After it has been successfully changed in Azure Portal, change this value to 
            // reflect the current setting.
            AutoScaling = true;
        }
    }
}