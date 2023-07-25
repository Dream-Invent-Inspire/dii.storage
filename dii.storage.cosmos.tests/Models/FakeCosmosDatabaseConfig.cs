using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace dii.storage.cosmos.tests.Models
{
    
    public class FakeCosmosDatabaseConfig : INoSqlDatabaseConfig
    {
        public static string FakeDBName = $"dii-storage-cosmos-tests-local-{Guid.NewGuid()}";

        public string Uri { get; set; } = "https://localhost:8081";
        public string Key { get; set; } = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        public List<string> DatabaseIds { get; set; } = new List<string>() { FakeDBName };
        public bool AutoCreate { get; set; } = true;
        public int MaxRUPerSecond { get; set; } = 4000;
        public bool AutoAdjustMaxRUPerSecond { get; set; } = true;
        public bool AutoScaling { get; set; } = true;
    }
}