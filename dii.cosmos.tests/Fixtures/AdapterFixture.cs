using dii.cosmos.Models.Interfaces;
using dii.cosmos.tests.Models;
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
        public IDiiAdapter<FakeEntity> FakeEntityAdapter;
        public IDiiAdapter<FakeEntityTwo> FakeEntityTwoAdapter;
        public List<FakeEntity> CreatedFakeEntities;
        public List<FakeEntityTwo> CreatedFakeEntityTwos;

        public AdapterFixture()
        {
            NoSqlDatabaseConfig = new FakeCosmosDatabaseConfig();

            var context = Context.Init(NoSqlDatabaseConfig);

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
                FakeEntityAdapter = new Adapter<FakeEntity>();
            }

            if (FakeEntityTwoAdapter == null)
            {
                FakeEntityTwoAdapter = new Adapter<FakeEntityTwo>();
            }

            if (CreatedFakeEntities == null)
            {
                CreatedFakeEntities = new List<FakeEntity>();
            }

            if (CreatedFakeEntityTwos == null)
            {
                CreatedFakeEntityTwos = new List<FakeEntityTwo>();
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