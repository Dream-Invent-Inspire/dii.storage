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
            var fakeCosmosDatabaseConfig = new FakeCosmosContextConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);

            var fetchedContext = DiiCosmosContext.Get();

            Assert.NotNull(fetchedContext);
            Assert.Equal(fakeCosmosDatabaseConfig.Uri, fetchedContext.Config.Uri);
            Assert.Equal(fakeCosmosDatabaseConfig.Uri, fetchedContext.Client.Endpoint.OriginalString);
            Assert.Equal(fakeCosmosDatabaseConfig.Key, fetchedContext.Config.Key);
            Assert.Equal(fakeCosmosDatabaseConfig.CosmosStorageDBs.First().DatabaseId, fetchedContext.Config.CosmosStorageDBs.First().DatabaseId);
            Assert.Equal(fakeCosmosDatabaseConfig.CosmosStorageDBs.First().AutoCreate, fetchedContext.Config.CosmosStorageDBs.First().AutoCreate);
            Assert.Equal(fakeCosmosDatabaseConfig.CosmosStorageDBs.First().MaxRUPerSecond, fetchedContext.Config.CosmosStorageDBs.First().MaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.CosmosStorageDBs.First().AutoAdjustMaxRUPerSecond, fetchedContext.Config.CosmosStorageDBs.First().AutoAdjustMaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.CosmosStorageDBs.First().AutoScaling, fetchedContext.Config.CosmosStorageDBs.First().AutoScaling);
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