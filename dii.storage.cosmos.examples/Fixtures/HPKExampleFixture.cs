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
    public class HPKExampleFixture : IDisposable
    {
        public Optimizer Optimizer;
        public INoSqlContextConfig NoSqlDatabaseConfig;
        public ExamplePersonSessionAdapter PersonSessionAdapter;

        public HPKExampleFixture()
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
                Optimizer = Optimizer.Init(NoSqlDatabaseConfig.CosmosStorageDBs.First().DatabaseId, typeof(PersonSession));
            }

            context.InitTablesAsync(ExampleConfig.DbName, Optimizer.Tables, true, Optimizer).Wait();

            if (PersonSessionAdapter == null)
            {
                PersonSessionAdapter = new ExamplePersonSessionAdapter();
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
