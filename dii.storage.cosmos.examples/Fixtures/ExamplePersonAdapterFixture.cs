using dii.storage.cosmos.examples.Adapters;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.examples.Models.Interfaces;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dii.storage.cosmos.examples.Fixtures
{
    /// <summary>
    /// A class to allow multiple tests within this test class to share a mock database context.
    /// </summary>
    public class ExamplePersonAdapterFixture : IDisposable
    {
        public Optimizer Optimizer;
        public INoSqlContextConfig NoSqlDatabaseConfig;
        public IExamplePersonAdapter PersonAdapter;
        public List<Person> People;

        public ExamplePersonAdapterFixture()
        {
            NoSqlDatabaseConfig = new ExampleConfig();

            var context = DiiCosmosContext.Init(NoSqlDatabaseConfig);

            var dbExistsTask = context.DoesDatabaseExistAsync();
            dbExistsTask.Wait();

            if (!dbExistsTask.Result)
            {
                throw new ApplicationException("ExamplePersonAdapterFixture test database does not exist and failed to be created.");
            }

            if (Optimizer == null)
            {
                //Optimizer = Optimizer.Init(NoSqlDatabaseConfig.CosmosStorageDBs.First().DatabaseId, typeof(Person), typeof(PersonSession));
                Optimizer = Optimizer.Init(NoSqlDatabaseConfig.CosmosStorageDBs.First().DatabaseId, typeof(Person));
            }

            context.InitTablesAsync(ExampleConfig.DbName, Optimizer.Tables, true, Optimizer).Wait();

            if (PersonAdapter == null)
            {
                PersonAdapter = new ExamplePersonAdapter();
            }

            if (People == null)
            {
                People = new List<Person>();
            }
        }

        protected virtual void Dispose(bool doNotCleanUpNative)
        {
            Optimizer.Clear();
            DiiCosmosContext.Reset();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}