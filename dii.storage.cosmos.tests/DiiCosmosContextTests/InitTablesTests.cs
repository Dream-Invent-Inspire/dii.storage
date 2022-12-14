using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosContextTests.Data;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using dii.storage.Exceptions;
using dii.storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{
    [Collection(nameof(InitTablesTests))]
    [TestCollectionPriorityOrder(103)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class InitTablesTests
    {
        [Fact, TestPriorityOrder(100)]
        public void InitTablesAsync_Prep()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);
            Assert.Empty(context.TableMappings);

            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(ContextEmptyDatabaseIdData))]
        public async Task InitTablesAsync_NullDatabaseId(string databaseId)
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => { return context.InitTablesAsync(databaseId, new List<TableMetaData>()); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(new ArgumentNullException(nameof(databaseId)).Message, exception.Message);
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(ContextEmptyInitData))]
        public async Task InitTablesAsync_NullTables(List<TableMetaData> tableMetaDatas)
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => { return context.InitTablesAsync("databaseId", tableMetaDatas); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(new ArgumentNullException(nameof(tableMetaDatas)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(103)]
        public async Task InitTablesAsync_DbNotInitialized()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);

            var databaseId = context.Config.DatabaseConfig.DatabaseIds.FirstOrDefault();

            var exception = await Assert.ThrowsAsync<DiiNotInitializedException>(() => { return context.InitTablesAsync(databaseId, optimizer.Tables); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException("Dbs").Message, exception.Message);
        }

        [Fact, TestPriorityOrder(104)]
        public async Task InitTablesAsync_Success()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);

            var databaseExists = await context.DoesDatabaseExistAsync().ConfigureAwait(false);

            Assert.True(databaseExists);

            var databaseId = context.DatabasesCreatedThisContext.FirstOrDefault();

            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);

            await context.InitTablesAsync(databaseId, optimizer.Tables).ConfigureAwait(false);

            Assert.NotNull(context.TableMappings);
            Assert.Equal(optimizer.TableMappings[typeof(FakeEntity)], context.TableMappings[databaseId][typeof(FakeEntity)]);
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            await TestHelpers.TeardownCosmosDbAsync().ConfigureAwait(false);

            TestHelpers.ResetContextInstance();
            TestHelpers.ResetOptimizerInstance();
        }
        #endregion
    }
}