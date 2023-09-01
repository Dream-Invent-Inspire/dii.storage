using dii.storage.cosmos.examples.Attributes;
using dii.storage.cosmos.examples.Fixtures;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.examples.Orderer;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Threading;

namespace dii.storage.cosmos.examples
{
    [Collection(nameof(LookupExampleTests))]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class LookupExampleTests : IClassFixture<LookupExampleFixture>
    {
        private readonly LookupExampleFixture _fixture;

        public LookupExampleTests(LookupExampleFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

            if (_fixture.Orders == null)
            {
                _fixture.Orders = FillOrders();
            }
        }

        private List<PersonOrder> FillOrders()
        {
            var orders = new List<PersonOrder>();

            #region add orders
            orders.Add(new PersonOrder
            {
                PaymentAmount = 100,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "1",
                PaymentId = "1",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                OrderDate = DateTime.Parse("2023-07-02T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "2",
                PaymentId = "2",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                OrderDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 300,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "3",
                PaymentId = "3",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28c",
                OrderDate = DateTime.Parse("2023-07-23T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 400,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "4",
                PaymentId = "4",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28d",
                OrderDate = DateTime.Parse("2023-07-24T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 500,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "5",
                PaymentId = "5",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28e",
                OrderDate = DateTime.Parse("2023-07-25T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 600,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "6",
                PaymentId = "6",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28e",
                OrderDate = DateTime.Parse("2023-07-12T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 700,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "7",
                PaymentId = "7",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28f",
                OrderDate = DateTime.Parse("2023-07-26T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 800,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "8",
                PaymentId = "8",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29a",
                OrderDate = DateTime.Parse("2023-07-27T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 900,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "9",
                PaymentId = "9",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29b",
                OrderDate = DateTime.Parse("2023-07-28T21:14:37.3066349Z")
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 1000,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "10",
                PaymentId = "10",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29c",
                OrderDate = DateTime.Parse("2023-07-29T22:14:37.3066349Z")
            });
            #endregion

            return orders;
        }

        //[Fact, TestPriorityOrder(10)]
        public async Task RunHPKExample1()
        {

            // Create new item
            var o1 = new PersonOrder
            {
                PaymentAmount = 100,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "1",
                PaymentId = Guid.NewGuid().ToString(),
                PersonId = Guid.NewGuid().ToString(),
                OrderDate = DateTimeOffset.Parse("2023-07-02T21:14:37.3066349Z")
            };

            var o2 = new PersonOrder
            {
                PaymentAmount = 200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "2",
                PaymentId = Guid.NewGuid().ToString(),
                PersonId = Guid.NewGuid().ToString(),
                OrderDate = DateTimeOffset.Parse("2023-07-20T21:14:37.3066349Z")
            };

            try
            {
                var result1 = await _fixture.PersonOrderAdapter.CreateAsync(o1).ConfigureAwait(false);
                Task.Delay(100).Wait();
                var result2 = await _fixture.PersonOrderAdapter.CreateAsync(o2).ConfigureAwait(false);
                Task.Delay(100).Wait();


                o1.Catalog = "Catalog1";
                var result3 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
                Task.Delay(100).Wait();

                o1.PaymentType = "Cash";
                var result4 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
                Task.Delay(100).Wait();


            }
            catch (Exception ex)
            {
            }

            //var storedsession = await _fixture.OrderLookupAdapter.LookupAsync(o1);

            //Assert.NotNull(storedsession);
            //Assert.Equal(storedsession.ClientId, createdPersonSession.ClientId);
            //Assert.Equal(storedsession.PersonId, createdPersonSession.PersonId);
            //Assert.Equal(storedsession.SessionId, createdPersonSession.SessionId);
            //Assert.Equal(storedsession.SessionStartDate, createdPersonSession.SessionStartDate);
            //Assert.Equal(storedsession.Catalog, createdPersonSession.Catalog);
            //Assert.Equal(storedsession.SessionEndDate, createdPersonSession.SessionEndDate);
            int x = 100;
            while (--x >= 0)
            {
                Task.Delay(1000).Wait();
            }

        }

        [Fact, TestPriorityOrder(20)]
        public async Task RunHPKExample2()
        {
            // Create new item
            var o1 = new PersonOrder
            {
                PaymentAmount = 2220,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid222",
                PaymentId = "pmtid222",
                PersonId = "person222",
                OrderDate = DateTimeOffset.Parse("2023-08-22T21:14:07.3066349Z")
            };

            try
            {
                //Create/update
                PersonOrder result1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);

                //Fetch
                var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
                int go = 50;
                while (getres1 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
                }
                Assert.NotNull(getres1);

                //Update a "normal" field
                getres1.Catalog = "Catalog2";
                var result2 = await _fixture.PersonOrderAdapter.UpsertAsync(getres1 ?? o1).ConfigureAwait(false);

                //Fetch again
                var getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
                go = 50;
                while (getres2 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
                }
                if (go > 0) Task.Delay(1000).Wait();
                Assert.NotNull(getres2);

                //Update a "id" field
                getres2.PaymentType = "Cash";
                var result3 = await _fixture.PersonOrderAdapter.UpsertAsync(getres2).ConfigureAwait(false);

                //Last fetch...will the "new" record be there? ...and the old record deleted?
                var result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
                go = 50;
                while (result4 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
                }
                if (go > 0) Task.Delay(1000).Wait();
                Assert.NotNull(result4);

            }
            catch (Exception ex)
            {
            }

            //int x = 100;
            //while (--x >= 0)
            //{
            //    Task.Delay(1000).Wait();
            //}

        }

        [Fact, TestPriorityOrder(30)]
        public async Task RunHPKExample3()
        {
            // Create new item
            var o1 = new PersonOrder
            {
                PaymentAmount = 300,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid3",
                PaymentId = "pmtid3",
                PersonId = "person3",
                OrderDate = DateTimeOffset.Parse("2023-08-03T21:14:07.3066349Z")
            };

            try
            {
                var result = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
        }

        [Fact, TestPriorityOrder(40)]
        public async Task RunHPKExample4()
        {
            // Create new item
            var o1 = new PersonOrder
            {
                PaymentAmount = 400,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid4",
                PaymentId = "pmtid4",
                PersonId = "person4",
                OrderDate = DateTimeOffset.Parse("2023-08-04T21:14:07.3066349Z")
            };

            try
            {
                //Create/update
                PersonOrder result1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);

                //Fetch
                var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
                int go = 20;
                while (getres1 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
                }
                Assert.NotNull(getres1);

                //Update a "normal" field
                getres1.Catalog = "Catalog2";
                var result2 = await _fixture.PersonOrderAdapter.UpsertAsync(getres1 ?? o1).ConfigureAwait(false);

                //Fetch again
                var getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
                go = 10;
                while (getres2 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
                }
                Assert.NotNull(getres2);

                //Update a "hpk" field
                getres2.OrderId = "orderidX";
                var result3 = await _fixture.PersonOrderAdapter.UpsertAsync(getres2).ConfigureAwait(false);

                //Last fetch...will the "new" record be there? ...and the old record deleted?
                var result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
                go = 10;
                while (result4 == null && --go > 0)
                {
                    Task.Delay(100).Wait();
                    result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
                }
                Assert.NotNull(result4);

            }
            catch (Exception ex)
            {
            }

            //int x = 100;
            //while (--x >= 0)
            //{
            //    Task.Delay(1000).Wait();
            //}

        }

        [Fact, TestPriorityOrder(50)]
        public async Task Test_ts()
        {
            DateTime dateTime = new DateTime(2023, 8, 24, 12, 01, 01);
            long unixEpoch = ConvertDateTimeToUnix(dateTime);

            long newEpoch = 1693064914; // Example Unix epoch timestamp
            dateTime = ConvertUnixToDateTime(newEpoch);
        }

        public static DateTime ConvertUnixToDateTime(long unixEpoch)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixEpoch);
        }
        public static long ConvertDateTimeToUnix(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime.ToUniversalTime() - epoch;
            return (long)timeSpan.TotalSeconds;
        }


        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            try
            {
                Optimizer.Clear();
                DiiCosmosContext.Reset();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
