using dii.storage.cosmos.tests.Adapters;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Models.Interfaces;
using dii.storage.cosmos.tests.Utilities;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var databaseId = NoSqlDatabaseConfig.DatabaseConfig.DatabaseIds.FirstOrDefault();

            if (FakeEntityAdapter == null)
            {
                FakeEntityAdapter = new FakeEntityAdapter(databaseId);
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