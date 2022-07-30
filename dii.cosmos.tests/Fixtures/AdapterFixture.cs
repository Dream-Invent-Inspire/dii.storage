using dii.cosmos.Models.Interfaces;
using dii.cosmos.tests.Adapters;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace dii.cosmos.tests.Fixtures
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

            var context = DiiCosmosContext.Init(NoSqlDatabaseConfig);

            var dbExistsTask = context.DoesDatabaseExistAsync();
            dbExistsTask.Wait();

            if (!dbExistsTask.Result)
            {
                throw new ApplicationException("AdapterFixture test database does not exist and failed to be created.");
            }

            if (Optimizer == null)
            {
                Optimizer = Optimizer.Init(typeof(FakeEntity), typeof(FakeEntityTwo));
            }

            context.InitTables(Optimizer.Tables.ToArray()).Wait();

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