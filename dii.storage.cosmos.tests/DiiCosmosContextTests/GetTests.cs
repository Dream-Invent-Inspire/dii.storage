﻿using dii.storage.Exceptions;
using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using Xunit;

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
            Assert.Equal(fakeCosmosDatabaseConfig.Uri, fetchedContext.Client.Endpoint.OriginalString);
            Assert.Equal(fakeCosmosDatabaseConfig.Key, fetchedContext.Config.Key);
            Assert.Equal(fakeCosmosDatabaseConfig.DatabaseId, fetchedContext.Config.DatabaseId);
            Assert.Equal(fakeCosmosDatabaseConfig.AutoCreate, fetchedContext.Config.AutoCreate);
            Assert.Equal(fakeCosmosDatabaseConfig.MaxRUPerSecond, fetchedContext.Config.MaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.AutoAdjustMaxRUPerSecond, fetchedContext.Config.AutoAdjustMaxRUPerSecond);
            Assert.Equal(fakeCosmosDatabaseConfig.AutoScaling, fetchedContext.Config.AutoScaling);
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