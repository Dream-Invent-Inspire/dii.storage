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
        public List<PersonSession> sessions = new List<PersonSession>();

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
            LoadSessions();
        }

        private void LoadSessions()
        {
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "111",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            this.sessions.Add(session);

            PersonSession session2 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "112",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session2);

            PersonSession session3 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "113",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session3);

            PersonSession session4 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "211",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session4);

            PersonSession session5 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "212",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session5);

            PersonSession session6 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "213",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session6);

            PersonSession session7 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "214",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session7);

            PersonSession session8 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "3",
                SessionId = "311",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            sessions.Add(session8);
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
