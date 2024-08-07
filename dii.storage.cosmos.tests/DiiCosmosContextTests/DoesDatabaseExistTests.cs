﻿using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
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
        public DoesDatabaseExistTests()
        {
            var fakeCosmosContextConfig = new FakeCosmosContextConfig();

            var context = DiiCosmosContext.Init(fakeCosmosContextConfig);

            Assert.NotNull(context);
        }

        [Fact, TestPriorityOrder(100)]
        public async Task DoesDatabaseExistAsync_Success()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.Null(context.Dbs);
            Assert.Null(context.DbProperties);
            Assert.Null(context.DbThroughput);
            Assert.False(context.DatabaseCreatedThisContext);

            var databaseExists = await context.DoesDatabaseExistAsync().ConfigureAwait(false);

            Assert.True(databaseExists);

            Assert.True(context.DatabaseCreatedThisContext);
            Assert.NotNull(context.Dbs);
            Assert.NotNull(context.DbProperties);
            Assert.NotNull(context.DbThroughput);
            Assert.Equal(context.Config.CosmosStorageDBs.First().MaxRUPerSecond, context.DbThroughput);
            Assert.Equal(context.Config.Uri, context.Client.Endpoint.OriginalString);
            Assert.Equal(context.Config.CosmosStorageDBs.Select(x => x.DatabaseId).ToList(), context.Dbs.Select(x => x.Id).ToList());
            Assert.Equal(context.Config.CosmosStorageDBs.First().MaxRUPerSecond, context.DbThroughput);
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