using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data;
using dii.storage.cosmos.tests.Fixtures;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests
{
    [Collection(nameof(FetchApiTests))]
    [TestCollectionPriorityOrder(401)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class FetchApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public FetchApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region GetAsync
		[Theory, TestPriorityOrder(100), ClassData(typeof(CreateFakeEntityData))]
		public async Task GetAsync_Prep(FakeEntity fakeEntity)
		{
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

            var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task GetAsync_Success()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];
			var fetchedFakeEntity = await _adapterFixture.FakeEntityAdapter.GetAsync(fakeEntity.Id, fakeEntity.FakeEntityId).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, fetchedFakeEntity, true);
		}

		[Fact, TestPriorityOrder(102)]
		public async Task GetAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
		}
        #endregion GetAsync

        #region GetManyAsync
        [Theory, TestPriorityOrder(200), ClassData(typeof(CreateBulkFakeEntityData))]
        public async Task GetManyAsync_Prep(List<FakeEntity> fakeEntities)
        {
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.CreateBulkAsync(fakeEntities).ConfigureAwait(false);

            foreach (var fakeEntity in fakeEntities)
            {
                TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity.Id));
            }

            _adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
        }

        [Fact, TestPriorityOrder(201)]
        public async Task GetManyAsync_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var items = new List<(string, string)>
            {
                (fakeEntity1.Id, fakeEntity1.FakeEntityId),
                (fakeEntity3.Id, fakeEntity3.FakeEntityId)
            };

            var fetchedFakeEntities = await _adapterFixture.FakeEntityAdapter.GetManyAsync(items).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(2, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }

        [Fact, TestPriorityOrder(202)]
        public async Task GetManyAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion GetManyAsync

        #region GetPagedAsync (w/QueryDefinition)
        [Theory, TestPriorityOrder(300), ClassData(typeof(CreateBulkFakeEntityData))]
        public async Task GetPagedAsync_QueryDefinition_Prep(List<FakeEntity> fakeEntities)
        {
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.CreateBulkAsync(fakeEntities).ConfigureAwait(false);

            foreach (var fakeEntity in fakeEntities)
            {
                TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity.Id));
            }

            _adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
        }

        [Fact, TestPriorityOrder(301)]
        public async Task GetPagedAsync_QueryDefinition_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var queryText = new QueryDefinition("SELECT * FROM fakeentity fe WHERE fe.int >= 1");

            var fetchedFakeEntities = await _adapterFixture.FakeEntityAdapter.GetPagedAsync(queryText).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(3, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }

        [Fact, TestPriorityOrder(302)]
        public async Task GetPagedAsync_QueryDefinition_Post()
        {
            await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion GetPagedAsync (w/QueryDefinition)

        #region GetPagedAsync (w/Query Text)
        [Theory, TestPriorityOrder(400), ClassData(typeof(CreateBulkFakeEntityData))]
        public async Task GetPagedAsync_QueryText_Prep(List<FakeEntity> fakeEntities)
        {
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.CreateBulkAsync(fakeEntities).ConfigureAwait(false);

            foreach (var fakeEntity in fakeEntities)
            {
                TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity.Id));
            }

            _adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
        }

        [Fact, TestPriorityOrder(401)]
        public async Task GetPagedAsync_QueryText_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var queryText = "SELECT * FROM fakeentity fe WHERE fe.int >= 1";

            var fetchedFakeEntities = await _adapterFixture.FakeEntityAdapter.GetPagedAsync(queryText).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(3, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }

        [Fact, TestPriorityOrder(402)]
        public async Task GetPagedAsync_QueryText_Post()
        {
            await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
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