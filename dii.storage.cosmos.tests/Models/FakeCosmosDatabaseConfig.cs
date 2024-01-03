using dii.storage.cosmos.Models;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace dii.storage.cosmos.tests.Models
{
    /// <inheritdoc/>
    public class FakeCosmosAccountConfig : BaseCosmosAccountConfig
    {
    }

    /// <inheritdoc/>
    public class FakeCosmosContextConfig : BaseCosmosContextConfig
    {
        public FakeCosmosContextConfig()
        {
            Uri = "https://localhost:8081";
            Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            CosmosStorageDBs = new List<BaseCosmosDatabaseConfig>
            {
               new FakeCosmosDatabaseConfig()
            };
        }
    }
    public class FakeCosmosDatabaseConfig : BaseCosmosDatabaseConfig
    {
        public static string FakeDBName = $"dii-storage-cosmos-tests-local-{Guid.NewGuid()}";

       public FakeCosmosDatabaseConfig()
        {
            DatabaseId = FakeDBName;
            AutoCreate = true;
            MaxRUPerSecond = 4000;
            AutoAdjustMaxRUPerSecond = true;
            AutoScaling = true;
        }
    }
}