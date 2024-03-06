using dii.storage.cosmos.examples.Attributes;
using dii.storage.cosmos.examples.Fixtures;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.examples.Orderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.examples
{
    [Collection(nameof(ExampleHPKTests))]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ExampleHPKTests : IClassFixture<HPKExampleFixture>
    {
        private readonly HPKExampleFixture _fixture;

        public ExampleHPKTests(HPKExampleFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact, TestPriorityOrder(10)]
        public async Task RunHPKExample1()
        {

            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "pid1",
                SessionId = "sid1",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1),
                Grants = new Dictionary<Domain, List<string>>() { { Domain.Payments, new List<string>() { "ALL" } }, { Domain.Pharmacy, new List<string>() { "ALL" } } }
            };
            var createdPersonSession = session;

            try
            {

                // Create the entity with our adapter.
                createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);

                Assert.NotNull(createdPersonSession);
                Assert.Equal(session.ClientId, createdPersonSession.ClientId);
                Assert.Equal(session.PersonId, createdPersonSession.PersonId);
                Assert.Equal(session.SessionId, createdPersonSession.SessionId);
                Assert.Equal(session.SessionStartDate, createdPersonSession.SessionStartDate);
                Assert.Equal(session.Catalog, createdPersonSession.Catalog);
                Assert.Equal(session.SessionEndDate, createdPersonSession.SessionEndDate);
            }
            catch (Exception ex)
            {
            }

            var storedsession = await _fixture.PersonSessionAdapter.FetchAsync(session.PersonId, session.ClientId, session.SessionId).ConfigureAwait(false);

            Assert.NotNull(storedsession);
            Assert.Equal(storedsession.ClientId, createdPersonSession.ClientId);
            Assert.Equal(storedsession.PersonId, createdPersonSession.PersonId);
            Assert.Equal(storedsession.SessionId, createdPersonSession.SessionId);
            Assert.Equal(storedsession.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(storedsession.Catalog, createdPersonSession.Catalog);
            Assert.Equal(storedsession.SessionEndDate, createdPersonSession.SessionEndDate);

        }


        [Fact, TestPriorityOrder(20)]
        public async Task RunHPKExample2()
        {
            //Bulk Upsert
            var createdPersonSessions = await _fixture.PersonSessionAdapter.UpsertManyAsync(_fixture.sessions).ConfigureAwait(false);

            List<(string, Dictionary<string, string>)> lst = _fixture.sessions.Select(s => (s.SessionId, new Dictionary<string, string>()
            {
                    { "ClientId", s.ClientId },
                    { "PersonId", s.PersonId }
                })).ToList();

            var storedsessions = await _fixture.PersonSessionAdapter.GetManyBySessionIdsAsync(lst.AsReadOnly()).ConfigureAwait(false);

            Assert.NotNull(storedsessions);
            Assert.Equal(5, storedsessions.Count());
        }

        [Fact, TestPriorityOrder(30)]
        public async Task RunHPKExample3()
        {
            //Delete bulk
            var res1 = await _fixture.PersonSessionAdapter.DeleteBulkAsync(_fixture.sessions).ConfigureAwait(false);

            //Create bulk
            var res2 = await _fixture.PersonSessionAdapter.CreateBulkAsync(_fixture.sessions).ConfigureAwait(false);

            //fetch them
            var storedsessions = await _fixture.PersonSessionAdapter.GetManyByClientIdAsync("SomeEnterprise", "1").ConfigureAwait(false);

            Assert.NotNull(storedsessions);
            Assert.Equal(storedsessions.Count(), 3);

        }

        [Fact, TestPriorityOrder(40)]
        public async Task RunHPKExample4()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "111",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.ReplaceAsync(session).ConfigureAwait(false);

            Assert.NotNull(createdPersonSession);
            Assert.Equal(session.ClientId, createdPersonSession.ClientId);
            Assert.Equal(session.PersonId, createdPersonSession.PersonId);
            Assert.Equal(session.SessionId, createdPersonSession.SessionId);
            Assert.Equal(session.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(session.Catalog, createdPersonSession.Catalog);
            Assert.Equal(session.SessionEndDate, createdPersonSession.SessionEndDate);

            var storedsession = await _fixture.PersonSessionAdapter.FetchAsync(session.PersonId, session.ClientId, session.SessionId).ConfigureAwait(false);

            Assert.NotNull(storedsession);
            Assert.Equal(storedsession.ClientId, createdPersonSession.ClientId);
            Assert.Equal(storedsession.PersonId, createdPersonSession.PersonId);
            Assert.Equal(storedsession.SessionId, createdPersonSession.SessionId);
            Assert.Equal(storedsession.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(storedsession.Catalog, createdPersonSession.Catalog);
            Assert.Equal(storedsession.SessionEndDate, createdPersonSession.SessionEndDate);

        }

        [Fact, TestPriorityOrder(50)]
        public async Task RunHPKExample5()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "e7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Furniture",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            Assert.NotNull(createdPersonSession);
            Assert.Equal(session.ClientId, createdPersonSession.ClientId);
            Assert.Equal(session.PersonId, createdPersonSession.PersonId);
            Assert.Equal(session.SessionId, createdPersonSession.SessionId);
            Assert.Equal(session.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(session.Catalog, createdPersonSession.Catalog);
            Assert.Equal(session.SessionEndDate, createdPersonSession.SessionEndDate);

            var storedsession = await _fixture.PersonSessionAdapter.FetchAsync(session.PersonId, session.ClientId, session.SessionId).ConfigureAwait(false);

            Assert.NotNull(storedsession);
            Assert.Equal(storedsession.ClientId, createdPersonSession.ClientId);
            Assert.Equal(storedsession.PersonId, createdPersonSession.PersonId);
            Assert.Equal(storedsession.SessionId, createdPersonSession.SessionId);
            Assert.Equal(storedsession.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(storedsession.Catalog, createdPersonSession.Catalog);
            Assert.Equal(storedsession.SessionEndDate, createdPersonSession.SessionEndDate);

        }

        [Fact, TestPriorityOrder(60)]
        public async Task RunHPKExample6()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "f7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = null
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            var updatedPersonSession = await _fixture.PersonSessionAdapter.AddEndTimeAsync(session.PersonId, session.ClientId, session.SessionId, session.SessionStartDate, DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)).ConfigureAwait(false);

            Assert.NotNull(updatedPersonSession);
            Assert.Equal(updatedPersonSession.ClientId, createdPersonSession.ClientId);
            Assert.Equal(updatedPersonSession.PersonId, createdPersonSession.PersonId);
            Assert.Equal(updatedPersonSession.SessionId, createdPersonSession.SessionId);
            Assert.Equal(updatedPersonSession.SessionStartDate, createdPersonSession.SessionStartDate);
            Assert.Equal(updatedPersonSession.Catalog, createdPersonSession.Catalog);
            Assert.Equal(updatedPersonSession.SessionEndDate, DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1));

        }

        [Fact, TestPriorityOrder(70)]
        public async Task RunHPKExample7()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "f7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            var bok = await _fixture.PersonSessionAdapter.DeleteEntityAsync(session).ConfigureAwait(false);

            Assert.NotNull(bok);
            Assert.Equal(bok, true);
        }

        [Fact, TestPriorityOrder(80)]
        public async Task RunHPKExample8()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "9999f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "f7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            //make sure this item doesn't already exist
            PersonSession newSession = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1411f20f-be3e-416a-a3e7-dcd5a3c1f28c",
                SessionId = "f7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            var bok = await _fixture.PersonSessionAdapter.DeleteEntityAsync(newSession).ConfigureAwait(false);

            //test changee on of the HPK values
            var replacedPersonSession = await _fixture.PersonSessionAdapter.ChangePersonIdAsync(session, "1411f20f-be3e-416a-a3e7-dcd5a3c1f28c").ConfigureAwait(false);

            Assert.NotNull(replacedPersonSession);
            Assert.Equal(replacedPersonSession.ClientId, createdPersonSession.ClientId);
            Assert.Equal(replacedPersonSession.PersonId, "1411f20f-be3e-416a-a3e7-dcd5a3c1f28c");
        }

        [Fact, TestPriorityOrder(90)]
        public async Task RunHPKExample9()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "f7da01b0-090b-41d2-8416-dacae09fbb6b",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Flowers",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            var bok = await _fixture.PersonSessionAdapter.DeleteEntityByIdAsync(session.PersonId, session.ClientId, session.SessionId).ConfigureAwait(false);

            Assert.NotNull(bok);
            Assert.Equal(bok, true);
        }

        [Fact, TestPriorityOrder(95)]
        public async Task RunHPKExample95()
        {
            var tmpsessions = _fixture.sessions.Select(x => new PersonSession()
            {
                ClientId = x.ClientId,
                PersonId = x.PersonId,
                SessionId = x.SessionId,
                SessionStartDate = x.SessionStartDate,
                Catalog = "TempCatalog", // x.Catalog,
                SessionEndDate = x.SessionEndDate
            }).ToList();

            foreach (var s in tmpsessions)
            {
                await _fixture.PersonSessionAdapter.UpsertAsync(s).ConfigureAwait(false);
            }

            //verify bulk delete
            var sessions = await _fixture.PersonSessionAdapter.ReplaceBulkAsync(_fixture.sessions.AsReadOnly()).ConfigureAwait(false);

            Assert.NotNull(sessions);
            //Assert.Equal(bok, true);
        }

        [Fact, TestPriorityOrder(97)]
        public async Task RunHPKExample97()
        {
            try
            {
                var sessions = await _fixture.PersonSessionAdapter.GetLastSessionsByPersonAsync(_fixture.sessions.First()?.ClientId).ConfigureAwait(false);

                Assert.NotNull(sessions);
            }
            catch (Exception ex)
            {
            }
        }


        [Fact, TestPriorityOrder(100)]
        public async Task RunHPKExample100()
        {
            var sessions = new List<PersonSession>();
            for (int i = 0; i < 10; i++)
            {
                foreach (var s in _fixture.sessions)
                {
                    s.PersonId += i.ToString();
                    var ps = await _fixture.PersonSessionAdapter.UpsertAsync(s).ConfigureAwait(false);
                    sessions.Add(ps);
                }
            }

            var pagedsessions = new List<PersonSession>();
            var storedsessions = await _fixture.PersonSessionAdapter.GetManyAsync("SomeEnterprise").ConfigureAwait(false);
            while (storedsessions.ContinuationToken != null) 
            {
                pagedsessions.AddRange(storedsessions);
                storedsessions = await _fixture.PersonSessionAdapter.GetManyAsync("SomeEnterprise", storedsessions.ContinuationToken).ConfigureAwait(false);
            }
            Assert.NotNull(pagedsessions);
            Assert.Equal(pagedsessions.Select(x => x.PersonId).ToList().Distinct().Count(), pagedsessions.Select(x => x.PersonId).ToList().Distinct().Count());

            //verify bulk delete
            var bok = await _fixture.PersonSessionAdapter.DeleteBulkAsync(sessions.AsReadOnly()).ConfigureAwait(false);

            Assert.NotNull(bok);
            Assert.Equal(bok, true);
        }


        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            try
            {
                //Optimizer.Clear();
                //DiiCosmosContext.Reset();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
