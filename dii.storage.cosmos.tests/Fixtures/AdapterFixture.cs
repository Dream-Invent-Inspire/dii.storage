using dii.storage.Models.Interfaces;
using dii.storage.cosmos.tests.Adapters;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Models.Interfaces;
using dii.storage.cosmos.tests.Utilities;
using System;
using System.Collections.Generic;

namespace dii.storage.cosmos.tests.Fixtures
{
    /// <summary>
    /// A class to allow multiple tests within this test class to share a mock database context.
    /// </summary>
    public class AdapterFixture : IDisposable
    {
        public Optimizer Optimizer;
        public INoSqlDatabaseConfig NoSqlDatabaseConfig;
        public IFakeAdapter<FakeEntity> FakeEntityAdapter;
        public List<FakeEntity> CreatedFakeEntities;

        public AdapterFixture()
        {
            NoSqlDatabaseConfig = new FakeCosmosDatabaseConfig();

            var initContextAndOptimizerTask = TestHelpers.InitContextAndOptimizerAsync(NoSqlDatabaseConfig, Optimizer, new[] { typeof(FakeEntity) });
            initContextAndOptimizerTask.Wait();

            Optimizer = initContextAndOptimizerTask.Result;

            if (FakeEntityAdapter == null)
            {
                FakeEntityAdapter = new FakeEntityAdapter();
            }

            if (CreatedFakeEntities == null)
            {
                CreatedFakeEntities = new List<FakeEntity>();
            }
        }

        protected virtual void Dispose(bool doNotCleanUpNative)
        {

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}