using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Fixtures;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using dii.cosmos.tests.Utilities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests.CosmosTests
{
    [Collection(nameof(FetchAdapterApiTests))]
    [TestCollectionPriorityOrder(3)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class FetchAdapterApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public FetchAdapterApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region GetAsync
		[Fact, TestPriorityOrder(100)]
		public async Task GetAsync_Prep()
		{
			// Ensure context exists and is initialized.
			var context = DiiCosmosContext.Get();

			Assert.NotNull(context);
			Assert.NotNull(context.TableMappings[typeof(FakeEntityTwo)]);

			var optimizer = Optimizer.Get();

			Assert.NotNull(optimizer);
			Assert.NotNull(optimizer.Tables.FirstOrDefault(x => x.TableName == nameof(FakeEntityTwo)));
			Assert.NotNull(optimizer.TableMappings[typeof(FakeEntityTwo)]);

			var fakeEntityTwo = new FakeEntityTwo
			{
				Id = DateTime.Now.Ticks.ToString(),
				FakeEntityTwoId = DateTime.Now.AddMinutes(-1).Ticks.ToString(),
				SearchableStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var savedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.CreateAsync(fakeEntityTwo).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo, savedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwo);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task GetAsync_Success()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var fetchedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.GetAsync(fakeEntityTwo.Id, fakeEntityTwo.FakeEntityTwoId).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo, fetchedFakeEntityTwo, true);
		}

		[Fact, TestPriorityOrder(102)]
		public async Task GetAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
		}
        #endregion GetAsync

        #region GetManyAsync
        [Fact, TestPriorityOrder(200)]
        public async Task GetManyAsync_Prep()
        {
            // Ensure context exists and is initialized.
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.NotNull(context.TableMappings[typeof(FakeEntityTwo)]);

            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
            Assert.NotNull(optimizer.Tables.FirstOrDefault(x => x.TableName == nameof(FakeEntityTwo)));
            Assert.NotNull(optimizer.TableMappings[typeof(FakeEntityTwo)]);

            var fakeEntityTwo1 = new FakeEntityTwo
            {
                Id = DateTime.Now.Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-1).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo2 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-2).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-3).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo3 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-4).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-5).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToCreate = new List<FakeEntityTwo>
            {
                fakeEntityTwo1,
                fakeEntityTwo2,
                fakeEntityTwo3
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.CreateBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id));

            _adapterFixture.CreatedFakeEntityTwos.AddRange(savedFakeEntityTwos);
        }

        [Fact, TestPriorityOrder(201)]
        public async Task GetManyAsync_Success()
        {
            var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
            var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

            var items = new List<(string, string)>
            {
                (fakeEntityTwo1.Id, fakeEntityTwo1.FakeEntityTwoId),
                (fakeEntityTwo3.Id, fakeEntityTwo3.FakeEntityTwoId)
            };

            var fetchedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.GetManyAsync(items).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntityTwos);
            Assert.Equal(2, fetchedFakeEntityTwos.Count);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id), true);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id), true);
        }

        [Fact, TestPriorityOrder(202)]
        public async Task GetManyAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion GetManyAsync

        #region GetPagedAsync (w/QueryDefinition)
        [Fact, TestPriorityOrder(300)]
        public async Task GetPagedAsync_QueryDefinition_Prep()
        {
            // Ensure context exists and is initialized.
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.NotNull(context.TableMappings[typeof(FakeEntityTwo)]);

            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
            Assert.NotNull(optimizer.Tables.FirstOrDefault(x => x.TableName == nameof(FakeEntityTwo)));
            Assert.NotNull(optimizer.TableMappings[typeof(FakeEntityTwo)]);

            var fakeEntityTwo1 = new FakeEntityTwo
            {
                Id = DateTime.Now.Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-1).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo2 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-2).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-3).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo3 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-4).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-5).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToCreate = new List<FakeEntityTwo>
            {
                fakeEntityTwo1,
                fakeEntityTwo2,
                fakeEntityTwo3
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.CreateBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id));

            _adapterFixture.CreatedFakeEntityTwos.AddRange(savedFakeEntityTwos);
        }

        [Fact, TestPriorityOrder(301)]
        public async Task GetPagedAsync_QueryDefinition_Success()
        {
            var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
            var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

            var idsToFetch = new Dictionary<string, string>()
            {
                { $"@id1", fakeEntityTwo1.Id },
                { $"@id2", fakeEntityTwo3.Id }
            };

            var queryDefinition = new QueryDefinition($"SELECT * FROM fakeentitytwo fet WHERE fet.id IN ({string.Join(", ", idsToFetch.Keys)})");

            foreach (var id in idsToFetch)
            {
                queryDefinition.WithParameter(id.Key, id.Value);
            }

            var fetchedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.GetPagedAsync(queryDefinition).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntityTwos);
            Assert.Equal(2, fetchedFakeEntityTwos.Count);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id), true);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id), true);
        }

        [Fact, TestPriorityOrder(302)]
        public async Task GetPagedAsync_QueryDefinition_Post()
        {
            await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion GetPagedAsync (w/QueryDefinition)

        #region GetPagedAsync (w/Query Text)
        [Fact, TestPriorityOrder(400)]
        public async Task GetPagedAsync_QueryText_Prep()
        {
            // Ensure context exists and is initialized.
            var context = DiiCosmosContext.Get();

            Assert.NotNull(context);
            Assert.NotNull(context.TableMappings[typeof(FakeEntityTwo)]);

            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
            Assert.NotNull(optimizer.Tables.FirstOrDefault(x => x.TableName == nameof(FakeEntityTwo)));
            Assert.NotNull(optimizer.TableMappings[typeof(FakeEntityTwo)]);

            var fakeEntityTwo1 = new FakeEntityTwo
            {
                Id = DateTime.Now.Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-1).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
                SearchableLongValue = 20L,
                CompressedStringValue = $"fakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo2 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-2).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-3).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
                SearchableLongValue = 25L,
                CompressedStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo3 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-4).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-5).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
                SearchableLongValue = 78658L,
                CompressedStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToCreate = new List<FakeEntityTwo>
            {
                fakeEntityTwo1,
                fakeEntityTwo2,
                fakeEntityTwo3
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.CreateBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id));

            _adapterFixture.CreatedFakeEntityTwos.AddRange(savedFakeEntityTwos);
        }

        [Fact, TestPriorityOrder(401)]
        public async Task GetPagedAsync_QueryText_Success()
        {
            var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
            var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
            var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

            var queryText = $"SELECT * FROM fakeentitytwo fet WHERE fet.long >= 20";

            var fetchedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.GetPagedAsync(queryText).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntityTwos);
            Assert.Equal(3, fetchedFakeEntityTwos.Count);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id), true);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id), true);
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, fetchedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id), true);
        }

        [Fact, TestPriorityOrder(402)]
        public async Task GetPagedAsync_QueryText_Post()
        {
            await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion GetPagedAsync (w/Query Text)

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            await TestHelpers.TeardownCosmosDbAsync().ConfigureAwait(false);

            TestHelpers.ResetContextInstance();
            TestHelpers.ResetOptimizerInstance();
        }
		#endregion
	}
}