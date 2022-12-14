using dii.storage.cosmos.Exceptions;
using dii.storage.cosmos.Models;
using dii.storage.cosmos.tests.Adapters;
using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosReadOnlyAdapterTests.Data;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using dii.storage.Models;
using System;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosReadOnlyAdapterTests
{
    [Collection(nameof(ReadOnlyConstructorTests))]
    [TestCollectionPriorityOrder(500)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ReadOnlyConstructorTests
    {
        [Theory, TestPriorityOrder(100), ClassData(typeof(EmptyStringData))]
        public void ConstructorTests_NullDatabaseId(string databaseId)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => { new FakeEntityReadOnlyAdapter(databaseId); });

            Assert.NotNull(exception);
            Assert.Equal(nameof(databaseId), exception.ParamName);
            Assert.Equal("databaseId is required to initialize the adapter. (Parameter 'databaseId')", exception.Message);
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(EmptyStringArrayData))]
        public void ConstructorTests_NullDatabaseIdConfig(string[] databaseIds)
        {
            var cosmosDatabaseConfig = new CosmosDatabaseConfig
            {
                Uri = "https://localhost:8081",
                DatabaseConfig = new DatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = databaseIds,
                    AutoCreate = true,
                    MaxRUPerSecond = 4000,
                    AutoAdjustMaxRUPerSecond = true,
                    AutoScaling = true
                }
            };

            _ = DiiCosmosContext.Init(cosmosDatabaseConfig);

            var exception = Assert.Throws<DiiDatabaseIdNotInConfigurationException>(() => { new FakeEntityReadOnlyAdapter("databaseId"); });

            Assert.NotNull(exception);
            Assert.Equal("DatabaseIds are required for configuration.", exception.Message);

            TestHelpers.ResetContextInstance();
        }

        [Fact, TestPriorityOrder(102)]
        public void ConstructorTests_MissingDatabaseId()
        {
            var cosmosDatabaseConfig = new CosmosDatabaseConfig
            {
                Uri = "https://localhost:8081",
                DatabaseConfig = new DatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        "notMyDatabaseId"
                    },
                    AutoCreate = true,
                    MaxRUPerSecond = 4000,
                    AutoAdjustMaxRUPerSecond = true,
                    AutoScaling = true
                }
            };

            _ = DiiCosmosContext.Init(cosmosDatabaseConfig);

            var exception = Assert.Throws<DiiDatabaseIdNotInConfigurationException>(() => { new FakeEntityReadOnlyAdapter("databaseId"); });

            Assert.NotNull(exception);
            Assert.Equal("DatabaseIds are required for configuration.", exception.Message);

            TestHelpers.ResetContextInstance();
        }

        [Fact, TestPriorityOrder(103)]
        public void ConstructorTests_MissingDatabaseIdBoth()
        {
            var cosmosDatabaseConfig = new CosmosDatabaseConfig
            {
                Uri = "https://localhost:8081",
                DatabaseConfig = new DatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        "notMyDatabaseId"
                    },
                    AutoCreate = true,
                    MaxRUPerSecond = 4000,
                    AutoAdjustMaxRUPerSecond = true,
                    AutoScaling = true
                },
                ReadOnlyDatabaseConfig = new ReadOnlyDatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        "notMyDatabaseIdEither"
                    }
                }
            };

            _ = DiiCosmosContext.Init(cosmosDatabaseConfig);

            var exception = Assert.Throws<DiiDatabaseIdNotInConfigurationException>(() => { new FakeEntityReadOnlyAdapter("databaseId"); });

            Assert.NotNull(exception);
            Assert.Equal("DatabaseIds are required for configuration.", exception.Message);

            TestHelpers.ResetContextInstance();
        }
    }
}