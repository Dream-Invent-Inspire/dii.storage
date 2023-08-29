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
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
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
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "111",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            PersonSession session2 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "112",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session2).ConfigureAwait(false);

            PersonSession session3 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "113",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session3).ConfigureAwait(false);

            PersonSession session4 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "211",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session4).ConfigureAwait(false);

            PersonSession session5 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "212",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session5).ConfigureAwait(false);

            PersonSession session6 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "213",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session6).ConfigureAwait(false);

            PersonSession session7 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "214",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session7).ConfigureAwait(false);

            PersonSession session8 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "3",
                SessionId = "311",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session8).ConfigureAwait(false);

            List<Tuple<string, Dictionary<string, string>>> lst = new List<Tuple<string, Dictionary<string, string>>>();

            lst.Add(Tuple.Create("111",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "1" }
                }));

            lst.Add(Tuple.Create("112",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "1" }
                }));
            lst.Add(Tuple.Create("113",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "1" }
                }));

            lst.Add(Tuple.Create("211",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "2" }
                }));
            lst.Add(Tuple.Create("212",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "2" }
                }));
            lst.Add(Tuple.Create("213",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "2" }
                }));
            lst.Add(Tuple.Create("214",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "2" }
                }));

            lst.Add(Tuple.Create("311",
                new Dictionary<string, string>() {
                    { "ClientId", "SomeEnterprise" },
                    { "PersonId", "3" }
                }));


            var storedsessions = await _fixture.PersonSessionAdapter.GetManyBySessionIdsAsync(lst.AsReadOnly()).ConfigureAwait(false);

            Assert.NotNull(storedsessions);
            Assert.Equal(storedsessions.Count(), 8);
        }

        [Fact, TestPriorityOrder(30)]
        public async Task RunHPKExample3()
        {
            #region set up
            // Create new items
            try
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

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "1",
                    SessionId = "112",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "1",
                    SessionId = "113",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "2",
                    SessionId = "211",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "2",
                    SessionId = "212",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "2",
                    SessionId = "213",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "2",
                    SessionId = "214",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }

            try
            {
                PersonSession session = new PersonSession()
                {
                    ClientId = "SomeEnterprise",
                    PersonId = "3",
                    SessionId = "311",
                    SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                    Catalog = "Pets",
                    SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
                };

                // Create the entity with our adapter.
                var createdPersonSession = await _fixture.PersonSessionAdapter.CreateAsync(session).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
            #endregion

            var storedsessions = await _fixture.PersonSessionAdapter.GetManyByClientIdAsync("SomeEnterprise", "1").ConfigureAwait(false);

            Assert.NotNull(storedsessions);
            Assert.Equal(storedsessions.Count(), 3);

            storedsessions = await _fixture.PersonSessionAdapter.SearchByRunDurationAsync("SomeEnterprise", "2", 3600000).ConfigureAwait(false);

            Assert.NotNull(storedsessions);
            //Assert.Equal(storedsessions.Count(), 4);
        }

        [Fact, TestPriorityOrder(40)]
        public async Task RunHPKExample4()
        {
            // Create new item
            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                SessionId = "e7da01b0-090b-41d2-8416-dacae09fbb6b",
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

            var bok = await _fixture.PersonSessionAdapter.DeleteEntityAsync(session);

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
            List<PersonSession> lst = new List<PersonSession>();

            PersonSession session = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "111",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session);

            // Create the entity with our adapter.
            var createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session).ConfigureAwait(false);

            PersonSession session2 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "112",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session2);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session2).ConfigureAwait(false);

            PersonSession session3 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "1",
                SessionId = "113",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session3);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session3).ConfigureAwait(false);

            PersonSession session4 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "211",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session4);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session4).ConfigureAwait(false);

            PersonSession session5 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "212",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session5);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session5).ConfigureAwait(false);

            PersonSession session6 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "213",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session6);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session6).ConfigureAwait(false);

            PersonSession session7 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "2",
                SessionId = "214",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session7);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session7).ConfigureAwait(false);

            PersonSession session8 = new PersonSession()
            {
                ClientId = "SomeEnterprise",
                PersonId = "3",
                SessionId = "311",
                SessionStartDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                Catalog = "Pets",
                SessionEndDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z").AddHours(1)
            };
            lst.Add(session8);

            // Create the entity with our adapter.
            createdPersonSession = await _fixture.PersonSessionAdapter.UpsertAsync(session8).ConfigureAwait(false);


            //verify bulk delete
            var bok = await _fixture.PersonSessionAdapter.DeleteBulkAsync(lst.AsReadOnly()).ConfigureAwait(false);

            Assert.NotNull(bok);
            Assert.Equal(bok, true);
        }
    }
}
