﻿using dii.storage.cosmos.examples.Attributes;
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
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace dii.storage.cosmos.examples
{
    [Collection(nameof(LookupExampleTests))]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class LookupExampleTests : IClassFixture<LookupExampleFixture>
    {
        private readonly LookupExampleFixture _fixture;
        private readonly ILogger<LookupExampleTests> _logger;

        public LookupExampleTests(LookupExampleFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            //_logger = _fixture.Logger;

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
                OrderDate = DateTime.Parse("2023-07-02T21:14:37.3066349Z"),
                ReceiptNumber = "Rec1",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem100"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "2",
                PaymentId = "2",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28b",
                OrderDate = DateTime.Parse("2023-07-20T21:14:37.3066349Z"),
                ReceiptNumber = "Rec2",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem200"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 300,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "3",
                PaymentId = "3",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28c",
                OrderDate = DateTime.Parse("2023-07-23T21:14:37.3066349Z"),
                ReceiptNumber = "Rec3",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem300"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 400,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "4",
                PaymentId = "4",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28d",
                OrderDate = DateTime.Parse("2023-07-24T21:14:37.3066349Z"),
                ReceiptNumber = "Rec4",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem400"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 500,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "5",
                PaymentId = "5",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28e",
                OrderDate = DateTime.Parse("2023-07-25T21:14:37.3066349Z"),
                ReceiptNumber = "Rec5",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem500"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 600,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "6",
                PaymentId = "6",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28e",
                OrderDate = DateTime.Parse("2023-07-12T21:14:37.3066349Z"),
                ReceiptNumber = "Rec6",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem600"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 700,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "7",
                PaymentId = "7",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f28f",
                OrderDate = DateTime.Parse("2023-07-26T21:14:37.3066349Z"),
                ReceiptNumber = "Rec7",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem700"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 800,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "8",
                PaymentId = "8",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29a",
                OrderDate = DateTime.Parse("2023-07-27T21:14:37.3066349Z"),
                ReceiptNumber = "Rec8",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem800"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 900,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "9",
                PaymentId = "9",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29b",
                OrderDate = DateTime.Parse("2023-07-28T21:14:37.3066349Z"),
                ReceiptNumber = "Rec9",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem900"
            });

            orders.Add(new PersonOrder
            {
                PaymentAmount = 1000,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "10",
                PaymentId = "10",
                PersonId = "8411f20f-be3e-416a-a3e7-dcd5a3c1f29c",
                OrderDate = DateTime.Parse("2023-07-29T22:14:37.3066349Z"),
                ReceiptNumber = "Rec10",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem1000"
            });
            #endregion

            return orders;
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
                OrderDate = DateTimeOffset.Parse("2023-08-22T21:14:07.3066349Z"),
                ReceiptNumber = "R123",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem10"
            };

            //Create/update
            PersonOrder result1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 100;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            //Update a "normal" field
            getres1.Catalog = "Catalog2";
            var result2 = await _fixture.PersonOrderAdapter.UpsertAsync(getres1 ?? o1).ConfigureAwait(false);

            //Fetch again
            var getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
            go = 100;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
            }
            if (go > 0) Task.Delay(1000).Wait();
            Assert.NotNull(getres2);

            //Update a "id" field
            getres2.PaymentType = "Cash";
            var result3 = await _fixture.PersonOrderAdapter.UpsertAsync(getres2).ConfigureAwait(false);

            //Last fetch...will the "new" record be there? ...and the old record deleted?
            var result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
            go = 100;
            while (result4 == null && --go > 0)
            {
                await Task.Delay(100);
                result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(result4);

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
                OrderDate = DateTimeOffset.Parse("2023-08-03T21:14:07.3066349Z"),
                ReceiptNumber = "Rec3",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem101"
            };

            var result = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
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
                OrderDate = DateTimeOffset.Parse("2023-08-04T21:14:07.3066349Z"),
                ReceiptNumber = "Rec4",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem102"
            };

            //Create/update
            PersonOrder result1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 100;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            //Update a "normal" field
            getres1.Catalog = "Catalog2";
            var result2 = await _fixture.PersonOrderAdapter.UpsertAsync(getres1 ?? o1).ConfigureAwait(false);

            //Fetch again
            var getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
            go = 60;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result2.PaymentType, result2.PaymentAmount.ToString(), result2.ClientId, result2.PersonId, result2.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres2);

            //Update a "hpk" field
            getres2.PersonId = "personX";
            var result3 = await _fixture.PersonOrderAdapter.UpsertAsync(getres2).ConfigureAwait(false);

            //Last fetch...will the "new" record be there? ...and the old record deleted?
            var result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
            go = 60;
            while (result4 == null && --go > 0)
            {
                await Task.Delay(100);
                result4 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(result3.PaymentType, result3.PaymentAmount.ToString(), result3.ClientId, result3.PersonId, result3.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(result4);
        }

        [Fact, TestPriorityOrder(50)]
        public async Task RunHPKExample5()
        {
            // Create new item
            var o1 = new PersonOrder
            {
                PaymentAmount = 500,
                PaymentType = "Cash",
                ClientId = "SomeEnterprise",
                OrderId = "orderid5",
                PaymentId = "pmtid5",
                PersonId = "person6",
                OrderDate = DateTimeOffset.Parse("2023-08-05T21:14:07.3066349Z"),
                ReceiptNumber = "Rec5",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem103"
            };

            //Create/update
            PersonOrder result1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 50;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            var result = await _fixture.PersonOrderAdapter.DeleteEntityAsync(o1).ConfigureAwait(false);

            var getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            go = 50;
            while (getres2 != null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.Null(getres2);
        }

        [Fact, TestPriorityOrder(60)]
        public async Task RunHPKExample6()
        {
            // Create orders
            await _fixture.PersonOrderAdapter.BulkUpsertAsync(_fixture.Orders).ConfigureAwait(false);

            //bulk patch
            List<(string, Dictionary<string, string>)> ops = _fixture.Orders.Select(x => (x.PaymentId, new Dictionary<string, string> { { "ClientId", x.ClientId }, { "OrderId", x.OrderId } } )).ToList();
            var res = await _fixture.PersonOrderAdapter.PatchBulkAsync(ops).ConfigureAwait(false);

            //bulk fetch
            var res2 = await _fixture.PersonOrderAdapter.GetManyByOrderIdsAsync(ops);
            Assert.Equal(ops.Count, res2.Count);

            //Page thru
            var res3 = await _fixture.PersonOrderAdapter.GetManyByClientIdAsync("SomeEnterprise").ConfigureAwait(false);
            var res4 = await _fixture.PersonOrderAdapter.GetManyByClientIdAsync("SomeEnterprise", res3.ContinuationToken).ConfigureAwait(false);

        }

        [Fact, TestPriorityOrder(70)]
        public async Task RunHPKExample7()
        {
            // Create orders
            foreach (var order in _fixture.Orders)
            {
                await _fixture.PersonOrderAdapter.UpsertAsync(order).ConfigureAwait(false);
                await Task.Delay(100);
            }

            //bulk fetch
            var res2 = await _fixture.PersonOrderAdapter.GetManyByClientIdAsync("SomeEnterprise").ConfigureAwait(false);
            //Assert.Equal(_fixture.Orders.Count, res2.Count);
        }

        [Fact, TestPriorityOrder(80)]
        public async Task RunHPKExample8()
        {
            // Create orders
            //foreach (var order in _fixture.Orders)
            //{
            //    await _fixture.PersonOrderAdapter.UpsertAsync(order).ConfigureAwait(false);
            //    await Task.Delay(100);
            //}

            //bulk fetch
            Task.Delay(100).Wait();

            var dtfrom = DateTime.Parse("2023-07-01");
            var dtto = DateTime.Parse("2023-08-03");
            var res1 = await _fixture.PersonOrderAdapter.GetManyByOrderDateAsync("SomeEnterprise", dtfrom, dtto).ConfigureAwait(false);
            int go = 100;
            while (res1 == null && --go > 0)
            {
                await Task.Delay(100);
                res1 = await _fixture.PersonOrderAdapter.GetManyByOrderDateAsync("SomeEnterprise", dtfrom, dtto).ConfigureAwait(false);
            }
            //Assert.Equal(_fixture.Orders.Count, res2.Count);
        }

        [Fact, TestPriorityOrder(90)]
        public async Task RunHPKExample9()
        {
            var o1 = new PersonOrder
            {
                PaymentAmount = 1200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid12",
                PaymentId = "pmtid12",
                PersonId = "person12",
                OrderDate = DateTime.UtcNow,
                ReceiptNumber = "R12",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem115"
            };

            var res1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 50;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            var getres2 = await _fixture.PersonOrderAdapter.GetByOrderDateAndItemAsync(o1.ClientId, o1.OrderDate.DateTime.AddDays(-1), o1.OrderDate.DateTime, o1.MasterItemId).ConfigureAwait(false);
            go = 50;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByOrderDateAndItemAsync(o1.ClientId, o1.OrderDate.DateTime.AddDays(-1), o1.OrderDate.DateTime, o1.MasterItemId).ConfigureAwait(false);
            }
            Assert.NotNull(getres2);
        }

        [Fact, TestPriorityOrder(100)]
        public async Task RunHPKExample10()
        {
            var o1 = _fixture.Orders[0];

            //Replace
            o1.PaymentType = "Examplle10-CC";
            var res1 = await _fixture.PersonOrderAdapter.ReplaceAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 100;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);
        }

        [Fact, TestPriorityOrder(110)]
        public async Task RunHPKExample11()
        {
            var tmporders = _fixture.Orders.Select(x => new PersonOrder
            {
                ClientId = x.ClientId,
                OrderId = x.OrderId,
                OrderDate = x.OrderDate,
                PaymentAmount = x.PaymentAmount,
                PaymentId = x.PaymentId,
                PaymentType = "Examplle11-CC", // x.PaymentType,
                PersonId = x.PersonId,
                ReceiptNumber = $"{x.PaymentId}-Rec",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem104"
            }).ToList();

            //Replace
            var res1 = await _fixture.PersonOrderAdapter.ReplaceBulkAsync(tmporders).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var o1 = tmporders[tmporders.Count - 1];
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 200;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);
        }

        [Fact, TestPriorityOrder(120)]
        public async Task RunHPKExample12()
        {
            var o1 = new PersonOrder
            {
                PaymentAmount = 1200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid12",
                PaymentId = "pmtid12",
                PersonId = "person12",
                OrderDate = DateTimeOffset.Parse("2022-08-22T21:14:07.3066349Z"),
                ReceiptNumber = "R12",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem106"
            };

            var res1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 50;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            var getres2 = await _fixture.PersonOrderAdapter.GetByReceiptAsync(o1.ReceiptNumber, o1.ClientId).ConfigureAwait(false);
            go = 500;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByReceiptAsync(o1.ReceiptNumber, o1.ClientId).ConfigureAwait(false);
            }
            Assert.NotNull(getres2);

            //Assert.Equal(_fixture.Orders.Count, res2.Count);
        }

        [Fact, TestPriorityOrder(120)]
        public async Task RunHPKExample14()
        {
            var o1 = new PersonOrder
            {
                PaymentAmount = 1200,
                PaymentType = "Credit",
                ClientId = "SomeEnterprise",
                OrderId = "orderid12",
                PaymentId = "pmtid12",
                PersonId = "person12",
                OrderDate = DateTimeOffset.Parse("2022-08-22T21:14:07.3066349Z"),
                ReceiptNumber = "R12",
                Catalog = "Catalog1",
                MasterItemId = "MasterItem106"
            };

            var res1 = await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            await Task.Delay(100);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            int go = 50;
            while (getres1 == null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.NotNull(getres1);

            var getres2 = await _fixture.PersonOrderAdapter.GetByReceiptAsync(o1.ReceiptNumber, o1.ClientId).ConfigureAwait(false);
            go = 50;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetByReceiptAsync(o1.ReceiptNumber, o1.ClientId).ConfigureAwait(false);
            }
            Assert.NotNull(getres2);

            //now delete
            var bok = await _fixture.PersonOrderAdapter.DeleteByReceiptAsync(getres2.ReceiptNumber, getres2.ClientId).ConfigureAwait(false);
            Assert.True(bok);

            getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            go = 50;
            while (getres1 != null && --go > 0)
            {
                await Task.Delay(100);
                getres1 = await _fixture.PersonOrderAdapter.GetByPersonIdAsync(o1.PaymentType, o1.PaymentAmount.ToString(), o1.ClientId, o1.PersonId, o1.OrderDate).ConfigureAwait(false);
            }
            Assert.Null(getres1);
        }

        [Fact, TestPriorityOrder(120)]
        public async Task RunHPKExample15()
        {
            string chkNum = "888";
            var o1 = _fixture.Orders[0];
            o1.CheckNumber = "101";

            //await _fixture.PersonOrderAdapter.UpsertAsync(o1).ConfigureAwait(false);
            //await Task.Delay(5000);

            //Fetch
            var getres1 = await _fixture.PersonOrderAdapter.FetchAsync(o1.PaymentId, o1.ClientId, o1.OrderId).ConfigureAwait(false);
            if (getres1 != null)
            {
                getres1.CheckNumber = chkNum;
                await _fixture.PersonOrderAdapter.UpsertAsync(getres1).ConfigureAwait(false);
                await Task.Delay(10000);
            }
            else
            {
                //???
            }

            var getres2 = await _fixture.PersonOrderAdapter.GetOrderByCheckNumber(o1.ClientId, chkNum).ConfigureAwait(false);
            int go = 100;
            while (getres2 == null && --go > 0)
            {
                await Task.Delay(100);
                getres2 = await _fixture.PersonOrderAdapter.GetOrderByCheckNumber(o1.ClientId, chkNum).ConfigureAwait(false);
            }
            Assert.NotNull(getres2);
        }


        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            // Delete the card from cosmos.
            var ids = _fixture.Orders
                .Select(x => (x.PaymentId, new Dictionary<string, string> { { "ClientId", x.ClientId }, { "OrderId", x.OrderId } }))
                .ToList();

            //Add 1 offs
            ids.Add(("pmtid222", new Dictionary<string, string> { { "ClientId", "SomeEnterprise" }, { "OrderId", "orderid222" } }));
            ids.Add(("pmtid3", new Dictionary<string, string> { { "ClientId", "SomeEnterprise" }, { "OrderId", "orderid3" } }));
            ids.Add(("pmtid4", new Dictionary<string, string> { { "ClientId", "SomeEnterprise" }, { "OrderId", "orderid4" } }));
            ids.Add(("pmtid5", new Dictionary<string, string> { { "ClientId", "SomeEnterprise" }, { "OrderId", "orderid5" } }));
            ids.Add(("pmtid12", new Dictionary<string, string> { { "ClientId", "SomeEnterprise" }, { "OrderId", "orderid12" } }));

            var res = await _fixture.PersonOrderAdapter.GetManyByOrderIdsAsync(ids).ConfigureAwait(false);

            _ = await _fixture.PersonOrderAdapter.DeleteBulkAsync(res).ConfigureAwait(false);

            int x = 0;
            bool runagain = true;
            do
            {
                await Task.Delay(2000);

                // Confirm records are gone.
                //res = await _fixture.PersonOrderAdapter.GetManyByOrderIdsAsync(ids).ConfigureAwait(false);
                res = await _fixture.PersonOrderAdapter
                        .GetManyByOrderDateAsync("SomeEnterprise", 
                            res.Min(x => x.OrderDate).DateTime,
                            res.Max(x => x.OrderDate).DateTime)
                        .ConfigureAwait(false);
                runagain = res.Count > 0;
                if (runagain)
                {
                    ids = res.Select(x => (x.PaymentId, new Dictionary<string, string> { { "ClientId", x.ClientId }, { "OrderId", x.OrderId } })).ToList();
                }
            } while (runagain && x++ < 50);

            await Task.Delay(5000);

            Assert.False(runagain);

        }
        #endregion
    }
}
