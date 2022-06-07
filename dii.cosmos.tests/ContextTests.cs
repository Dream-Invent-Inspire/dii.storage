using dii.cosmos.Exceptions;
using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests
{
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ContextTests
    {
        [Fact, TestPriorityOrder(1)]
        [Trait("Cosmos", "Context Tests")]
        public void Init_NotInitialized()
        {
            Context context = null;

            Assert.Null(context);

            var exception = Assert.Throws<DiiNotInitializedException>(() => { Context.Get(); });

            Assert.NotNull(exception);
            Assert.Equal("Context not initialized.", exception.Message);
        }

        [Fact, TestPriorityOrder(2)]
        [Trait("Cosmos", "Context Tests")]
        public void Init_Success()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig
            {
                DatabaseId = "dii-tests-local-context-tests"
            };

            var context = Context.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);
        }

        [Fact, TestPriorityOrder(3)]
        [Trait("Cosmos", "Context Tests")]
        public void Init_Get()
        {
            var context = Context.Get();

            Assert.NotNull(context);
        }

        [Fact, TestPriorityOrder(4)]
        [Trait("Cosmos", "Context Tests")]
        public async Task DoesDatabaseExistAsync_Success()
        {
            var context = Context.Get();

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
        [Trait("Cosmos", "Context Tests")]
        public async Task InitTables_Success()
        {
            var context = Context.Get();

            Assert.NotNull(context);
            Assert.Null(context.TableMappings);

            var optimizer = Optimizer.Init(typeof(FakeEntity));

            Assert.NotNull(optimizer);

            await context.InitTables(optimizer.Tables.ToArray()).ConfigureAwait(false);

            Assert.NotNull(context.TableMappings);
            Assert.Equal(optimizer.TableMappings[typeof(FakeEntity)], context.TableMappings[typeof(FakeEntity)]);
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        [Trait("Cosmos", "Context Tests")]
        public async Task Teardown()
        {
            var context = Context.Get();

            if (context.Db != null)
            {
                _ = await context.Db.DeleteAsync().ConfigureAwait(false);
            }
        }
        #endregion
    }
}