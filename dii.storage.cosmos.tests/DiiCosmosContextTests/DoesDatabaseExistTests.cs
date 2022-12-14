using dii.storage.cosmos.Exceptions;
using dii.storage.cosmos.Models;
using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosContextTests.Data;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using dii.storage.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{
    [Collection(nameof(DoesDatabaseExistTests))]
    [TestCollectionPriorityOrder(102)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class DoesDatabaseExistTests
    {
        [Theory, TestPriorityOrder(100), ClassData(typeof(EmptyStringArrayData))]
        public async Task DoesDatabaseExistAsync_NullDatabaseIdConfig(string[] databaseIds)
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

            var context = DiiCosmosContext.Init(cosmosDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.Dbs);
            Assert.Empty(context.DbProperties);
            Assert.Empty(context.DbThroughputs);
            Assert.Empty(context.DatabasesCreatedThisContext);

            var exception = await Assert.ThrowsAsync<DiiDatabaseIdNotInConfigurationException>( () => { return context.DoesDatabaseExistAsync(); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal("DatabaseIds are required for configuration.", exception.Message);

            TestHelpers.ResetContextInstance();
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(EmptyStringArrayData))]
        public async Task DoesDatabaseExistAsync_NullDatabaseIdConfigReadOnly(string[] databaseIds)
        {
            var cosmosDatabaseConfig = new CosmosDatabaseConfig
            {
                Uri = "https://localhost:8081",
                ReadOnlyDatabaseConfig = new ReadOnlyDatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = databaseIds
                }
            };

            var context = DiiCosmosContext.Init(cosmosDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.Dbs);
            Assert.Empty(context.DbProperties);
            Assert.Empty(context.DbThroughputs);
            Assert.Empty(context.DatabasesCreatedThisContext);

            var exception = await Assert.ThrowsAsync<DiiDatabaseIdNotInConfigurationException>(() => { return context.DoesDatabaseExistAsync(); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal("DatabaseIds are required for configuration.", exception.Message);

            TestHelpers.ResetContextInstance();
        }

        [Fact, TestPriorityOrder(102)]
        public async Task DoesDatabaseExistAsync_DuplicateDatabaseId()
        {
            var cosmosDatabaseConfig = new CosmosDatabaseConfig
            {
                Uri = "https://localhost:8081",
                DatabaseConfig = new DatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        "databaseId"
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
                        "databaseId"
                    }
                }
            };

            var context = DiiCosmosContext.Init(cosmosDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.Dbs);
            Assert.Empty(context.DbProperties);
            Assert.Empty(context.DbThroughputs);
            Assert.Empty(context.DatabasesCreatedThisContext);

            var exception = await Assert.ThrowsAsync<DiiDuplicateDatabaseIdInConfigurationException>(() => { return context.DoesDatabaseExistAsync(); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal("One or more databaseIds are specified in the configuration more than once.", exception.Message);

            TestHelpers.ResetContextInstance();
        }

        [Fact, TestPriorityOrder(103)]
        public async Task DoesDatabaseExistAsync_Success()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.Dbs);
            Assert.Empty(context.DbProperties);
            Assert.Empty(context.DbThroughputs);
            Assert.Empty(context.DatabasesCreatedThisContext);

            var databaseExists = await context.DoesDatabaseExistAsync().ConfigureAwait(false);

            Assert.True(databaseExists);

            var databaseId = context.DatabasesCreatedThisContext.FirstOrDefault();

            Assert.NotEmpty(context.DatabasesCreatedThisContext);
            Assert.NotEmpty(context.Dbs);
            Assert.NotEmpty(context.DbProperties);
            Assert.NotEmpty(context.DbThroughputs);
            Assert.Equal(context.Config.DatabaseConfig.MaxRUPerSecond, context.DbThroughputs[databaseId]);
            Assert.Equal(context.Config.Uri, context.ReadWriteClient.Endpoint.OriginalString);
            Assert.Equal(context.Config.DatabaseConfig.DatabaseIds[0], context.Dbs[databaseId].Id);
            Assert.Equal(context.Config.DatabaseConfig.MaxRUPerSecond, context.DbThroughputs[databaseId]);

            await TestHelpers.TeardownCosmosDbAsync().ConfigureAwait(false);

            TestHelpers.ResetContextInstance();
        }

        [Fact, TestPriorityOrder(104)]
        public async Task DoesDatabaseExistAsync_SuccessMultipleDatabases()
        {
            var fakeCosmosMultipleDatabaseConfig = new FakeCosmosMultipleDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosMultipleDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.Dbs);
            Assert.Empty(context.DbProperties);
            Assert.Empty(context.DbThroughputs);
            Assert.Empty(context.DatabasesCreatedThisContext);

            var databaseExists = await context.DoesDatabaseExistAsync().ConfigureAwait(false);

            Assert.True(databaseExists);

            Assert.NotEmpty(context.DatabasesCreatedThisContext);
            Assert.NotEmpty(context.Dbs);
            Assert.NotEmpty(context.DbProperties);
            Assert.NotEmpty(context.DbThroughputs);
            Assert.Equal(context.Config.Uri, context.ReadWriteClient.Endpoint.OriginalString);

            var i = 0;

            foreach (var databaseId in context.DatabasesCreatedThisContext)
            {
                Assert.Equal(context.Config.DatabaseConfig.MaxRUPerSecond, context.DbThroughputs[databaseId]);
                Assert.Equal(context.Config.DatabaseConfig.DatabaseIds[i], context.Dbs[databaseId].Id);
                Assert.Equal(context.Config.DatabaseConfig.MaxRUPerSecond, context.DbThroughputs[databaseId]);

                i++;
            }
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            await TestHelpers.TeardownCosmosDbAsync().ConfigureAwait(false);

            TestHelpers.ResetContextInstance();
        }
        #endregion
    }
}