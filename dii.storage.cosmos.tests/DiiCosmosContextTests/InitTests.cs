﻿using dii.storage.Exceptions;
using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using Xunit;
using System;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{
    [Collection(nameof(InitTests))]
    [TestCollectionPriorityOrder(100)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class InitTests
    {
        [Fact, TestPriorityOrder(100)]
        public void Init_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { DiiCosmosContext.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(DiiCosmosContext)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(101)]
        public void Init_NullINoSqlDatabaseConfig()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => { DiiCosmosContext.Init(null); });

            Assert.NotNull(exception);
            Assert.Equal("config", exception.ParamName);
            Assert.Equal("An INoSqlDatabaseConfig is required to initialize an instance of DiiCosmosContext. (Parameter 'config')", exception.Message);
        }

        [Fact, TestPriorityOrder(102)]
        public void Init_Success()
        {
            var fakeCosmosDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = DiiCosmosContext.Init(fakeCosmosDatabaseConfig);

            Assert.NotNull(context);
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