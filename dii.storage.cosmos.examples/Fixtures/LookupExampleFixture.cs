using dii.storage.cosmos.examples.Adapters;
using dii.storage.cosmos.examples.Models.Interfaces;
using dii.storage.cosmos.examples.Models;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.cosmos.examples.Fixtures
{
    /// <summary>
    /// A class to allow multiple tests within this test class to share a mock database context.
    /// </summary>
    public class LookupExampleFixture : IDisposable
    {
        public Optimizer Optimizer;
        public INoSqlContextConfig NoSqlDatabaseConfig;
        public IExamplePersonAdapter PersonAdapter;
        public IExamplePersonSessionAdapter PersonSessionAdapter;
        public ExamplePersonOrderAdapter PersonOrderAdapter;
        public List<PersonOrder> Orders;
        public DiiCosmosLookupAdapter OrderLookupAdapter;

        public LookupExampleFixture()
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
                Optimizer = Optimizer.Init(NoSqlDatabaseConfig.CosmosStorageDBs.First().DatabaseId, typeof(Person), typeof(PersonSession), typeof(PersonOrder));
                //Optimizer = Optimizer.Init(NoSqlDatabaseConfig.CosmosStorageDBs.First().DatabaseId, typeof(PersonOrder));
            }

            context.InitTablesAsync(ExampleConfig.DbName, Optimizer.Tables, true, Optimizer).Wait();

            if (PersonAdapter == null)
            {
                PersonAdapter = new ExamplePersonAdapter();
            }
            if (PersonOrderAdapter == null)
            {
                PersonOrderAdapter = new ExamplePersonOrderAdapter();
            }

            if (OrderLookupAdapter == null)
            {
                var table = Optimizer.GetLookupTableMetaData<PersonOrder>();                
                OrderLookupAdapter = new DiiCosmosLookupAdapter(table);
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
