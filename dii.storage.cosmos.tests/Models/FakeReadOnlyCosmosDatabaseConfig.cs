using dii.storage.Models;
using dii.storage.Models.Interfaces;
using System;

namespace dii.storage.cosmos.tests.Models
{
    public class FakeReadOnlyCosmosDatabaseConfig : INoSqlDatabaseConfig
    {
        private static string _databaseIdSuffix = Guid.NewGuid().ToString();

        public string Uri { get; set; } = "https://localhost:8081";

        public DatabaseConfig DatabaseConfig { get; set; }

        public ReadOnlyDatabaseConfig ReadOnlyDatabaseConfig
        {
            get
            {
                return new ReadOnlyDatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        $"dii-storage-cosmos-tests-local-{_databaseIdSuffix}"
                    }
                };
            }
            set => ReadOnlyDatabaseConfig = value;
        }
    }
}