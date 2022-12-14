using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.Fixtures;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosReadOnlyAdapterTests
{
    [Collection(nameof(ReadOnlyFetchApiTests))]
    [TestCollectionPriorityOrder(501)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ReadOnlyFetchApiTests : IClassFixture<ReadOnlyAdapterFixture>
    {
        private readonly ReadOnlyAdapterFixture _adapterFixture;

        public ReadOnlyFetchApiTests(ReadOnlyAdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region GetAsync
		[Fact, TestPriorityOrder(100)]
		public async Task GetAsync_Success()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];
			var fetchedFakeEntity = await _adapterFixture.FakeEntityReadOnlyAdapter.GetAsync(fakeEntity.Id, fakeEntity.FakeEntityId).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, fetchedFakeEntity, true);
        }
        #endregion GetAsync

        #region GetManyAsync
        [Fact, TestPriorityOrder(101)]
        public async Task GetManyAsync_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var items = new List<(string, string)>
            {
                (fakeEntity1.Id, fakeEntity1.FakeEntityId),
                (fakeEntity3.Id, fakeEntity3.FakeEntityId)
            };

            var fetchedFakeEntities = await _adapterFixture.FakeEntityReadOnlyAdapter.GetManyAsync(items).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(2, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }
        #endregion GetManyAsync

        #region GetPagedAsync (w/QueryDefinition)
        [Fact, TestPriorityOrder(102)]
        public async Task GetPagedAsync_QueryDefinition_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var queryText = new QueryDefinition("SELECT * FROM fakeentity fe WHERE fe.int >= 1");

            var fetchedFakeEntities = await _adapterFixture.FakeEntityReadOnlyAdapter.GetPagedAsync(queryText).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(3, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }
        #endregion GetPagedAsync (w/QueryDefinition)

        #region GetPagedAsync (w/Query Text)
        [Fact, TestPriorityOrder(103)]
        public async Task GetPagedAsync_QueryText_Success()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var queryText = "SELECT * FROM fakeentity fe WHERE fe.int >= 1";

            var fetchedFakeEntities = await _adapterFixture.FakeEntityReadOnlyAdapter.GetPagedAsync(queryText).ConfigureAwait(false);

            Assert.NotNull(fetchedFakeEntities);
            Assert.Equal(3, fetchedFakeEntities.Count);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity1, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id), true);
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, fetchedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id), true);
        }
        #endregion GetPagedAsync (w/Query Text)

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            await _adapterFixture.TeardownCosmosDbAsync(_adapterFixture.NoSqlDatabaseConfig).ConfigureAwait(false);
        }
        #endregion
    }
}