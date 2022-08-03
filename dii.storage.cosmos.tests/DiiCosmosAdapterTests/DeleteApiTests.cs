using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data;
using dii.storage.cosmos.tests.Fixtures;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Orderer;
using dii.storage.cosmos.tests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests
{
    [Collection(nameof(DeleteApiTests))]
    [TestCollectionPriorityOrder(405)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class DeleteApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public DeleteApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region DeleteEntityAsync
		[Theory, TestPriorityOrder(100), ClassData(typeof(SingleFakeEntityData))]
		public async Task DeleteEntityAsync_Prep(FakeEntity fakeEntity)
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task DeleteEntityAsync_Success()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];

			var success = await _adapterFixture.FakeEntityAdapter.DeleteEntityAsync(fakeEntity).ConfigureAwait(false);

			Assert.True(success);

			var shouldBeNull = await _adapterFixture.FakeEntityAdapter.GetAsync(fakeEntity.Id, fakeEntity.FakeEntityId).ConfigureAwait(false);

			Assert.Null(shouldBeNull);

			_adapterFixture.CreatedFakeEntities.Clear();
		}
		#endregion DeleteEntityAsync

		#region DeleteAsync
		[Theory, TestPriorityOrder(200), ClassData(typeof(SingleFakeEntityData))]
		public async Task DeleteAsync_Prep(FakeEntity fakeEntity)
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(201)]
		public async Task DeleteAsync_Success()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];

			var success = await _adapterFixture.FakeEntityAdapter.DeleteAsync(fakeEntity.Id, fakeEntity.FakeEntityId).ConfigureAwait(false);

			Assert.True(success);

			var shouldBeNull = await _adapterFixture.FakeEntityAdapter.GetAsync(fakeEntity.Id, fakeEntity.FakeEntityId).ConfigureAwait(false);

			Assert.Null(shouldBeNull);

			_adapterFixture.CreatedFakeEntities.Clear();
		}
		#endregion DeleteAsync

		#region DeleteEntitiesBulkAsync
		[Theory, TestPriorityOrder(300), ClassData(typeof(MultipleFakeEntityData))]
		public async Task DeleteEntitiesBulkAsync_Prep(List<FakeEntity> fakeEntities)
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
		public async Task DeleteEntitiesBulkAsync_Success()
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var success = await _adapterFixture.FakeEntityAdapter.DeleteEntitiesBulkAsync(new[] { fakeEntity1, fakeEntity2, fakeEntity3 }).ConfigureAwait(false);

			Assert.True(success);

			var idsToCheck = new List<(string id, string partitionKey)>
			{
				(fakeEntity1.Id, fakeEntity1.FakeEntityId),
				(fakeEntity2.Id, fakeEntity2.FakeEntityId),
				(fakeEntity3.Id, fakeEntity3.FakeEntityId)
			};

			var shouldBeNull = await _adapterFixture.FakeEntityAdapter.GetManyAsync(idsToCheck).ConfigureAwait(false);

			Assert.Empty(shouldBeNull);

			_adapterFixture.CreatedFakeEntities.Clear();
		}
		#endregion DeleteEntitiesBulkAsync

		#region DeleteBulkAsync
		[Theory, TestPriorityOrder(400), ClassData(typeof(MultipleFakeEntityData))]
		public async Task DeleteBulkAsync_Prep(List<FakeEntity> fakeEntities)
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
		public async Task DeleteBulkAsync_Success()
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var items = new List<(string id, string partitionKey)>
			{
				(fakeEntity1.Id, fakeEntity1.FakeEntityId),
				(fakeEntity2.Id, fakeEntity2.FakeEntityId),
				(fakeEntity3.Id, fakeEntity3.FakeEntityId)
			};

			var success = await _adapterFixture.FakeEntityAdapter.DeleteBulkAsync(items).ConfigureAwait(false);

			Assert.True(success);

			var shouldBeNull = await _adapterFixture.FakeEntityAdapter.GetManyAsync(items).ConfigureAwait(false);

			Assert.Empty(shouldBeNull);

			_adapterFixture.CreatedFakeEntities.Clear();
		}
		#endregion DeleteBulkAsync

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