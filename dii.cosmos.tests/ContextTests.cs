using dii.cosmos.Exceptions;
using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using dii.cosmos.tests.Utilities;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests
{
    [Collection(nameof(ContextTests))]
    [TestCollectionPriorityOrder(1)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ContextTests
    {
        [Fact, TestPriorityOrder(1)]
        public void Init_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { DiiCosmosContext.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(DiiCosmosContext)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(2)]
        public void Init_Success()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);
        }

        [Fact, TestPriorityOrder(3)]
        public void Init_Get()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
        }

        [Fact, TestPriorityOrder(4)]
        public async Task DoesDatabaseExistAsync_Success()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.Null(context.Db);
            Assert.Null(context.DbProperties);
            Assert.Null(context.DbThroughput);
            Assert.False(context.DatabaseCreatedThisContext);

            Assert.True(await context.DoesDatabaseExistAsync().ConfigureAwait(false));

            Assert.True(context.DatabaseCreatedThisContext);
            Assert.NotNull(context.Db);
            Assert.NotNull(context.DbProperties);
            Assert.NotNull(context.DbThroughput);
            Assert.Equal(context.Config.MaxRUPerSecond, context.DbThroughput);
        }

        [Fact, TestPriorityOrder(5)]
        public async Task InitTables_Success()
        {
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.Null(context.TableMappings);

            var optimizer = Optimizer.Init(typeof(FakeEntity));

            Assert.NotNull(optimizer);

            await context.InitTables(optimizer.Tables).ConfigureAwait(false);

            Assert.NotNull(context.TableMappings);
            Assert.Equal(optimizer.TableMappings[typeof(FakeEntity)], context.TableMappings[typeof(FakeEntity)]);
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