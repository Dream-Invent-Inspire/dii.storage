using dii.storage.Exceptions;
using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using Xunit;
using System.Linq;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{
    [Collection(nameof(GetTests))]
    [TestCollectionPriorityOrder(101)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class GetTests
    {
        [Fact, TestPriorityOrder(100)]
        public void Get_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { DiiCosmosContext.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(DiiCosmosContext)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(101)]
        public void Get_Success()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);

            var fetchedContext = DiiCosmosContext.Get();

            Assert.NotNull(fetchedContext);
            Assert.Equal(fakeCosmosDatabaseConfig.Uri, fetchedContext.Config.Uri);
            Assert.Equal(fakeCosmosDatabaseConfig.Uri, fetchedContext.ReadWriteClient.Endpoint.OriginalString);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.Key, fetchedContext.Config.DatabaseConfig.Key);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.DatabaseIds[0], fetchedContext.Config.DatabaseConfig.DatabaseIds[0]);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.AutoCreate, fetchedContext.Config.DatabaseConfig.AutoCreate);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.MaxRUPerSecond, fetchedContext.Config.DatabaseConfig.MaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.AutoAdjustMaxRUPerSecond, fetchedContext.Config.DatabaseConfig.AutoAdjustMaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseConfig.AutoScaling, fetchedContext.Config.DatabaseConfig.AutoScaling);
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public void Teardown()
        {
            TestHelpers.ResetContextInstance();
        }
        #endregion
    }
}