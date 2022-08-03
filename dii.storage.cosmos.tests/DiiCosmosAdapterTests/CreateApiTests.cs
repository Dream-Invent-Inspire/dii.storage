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
    [Collection(nameof(CreateApiTests))]
    [TestCollectionPriorityOrder(401)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class CreateApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public CreateApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region CreateAsync
		[Fact, TestPriorityOrder(100)]
		public void CreateAsync_Prep()
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();
		}

		[Theory, TestPriorityOrder(101), ClassData(typeof(SingleFakeEntityData))]
		public async Task CreateAsync_Success(FakeEntity fakeEntity)
		{
			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(102)]
		public async Task CreateAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion CreateAsync

		#region CreateBulkAsync
		[Fact, TestPriorityOrder(200)]
		public void CreateBulkAsync_Prep()
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();
		}

		[Theory, TestPriorityOrder(201), ClassData(typeof(MultipleFakeEntityData))]
		public async Task CreateBulkAsync_Success(List<FakeEntity> fakeEntities)
		{
			var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.CreateBulkAsync(fakeEntities).ConfigureAwait(false);

			foreach (var fakeEntity in fakeEntities)
			{
				TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity.Id));
			}

			_adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
		}

		[Fact, TestPriorityOrder(202)]
		public async Task CreateBulkAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion CreateBulkAsync

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