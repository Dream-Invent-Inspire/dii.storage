using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Fixtures;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using dii.cosmos.tests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests.CosmosTests
{
    [Collection(nameof(DeleteAdapterApiTests))]
    [TestCollectionPriorityOrder(7)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class DeleteAdapterApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public DeleteAdapterApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region DeleteAsync
		[Fact, TestPriorityOrder(100)]
		public async Task DeleteAsync_Prep()
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
		public async Task DeleteAsync_Success()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];

			var success = await _adapterFixture.FakeEntityTwoAdapter.DeleteAsync(fakeEntityTwo.Id, fakeEntityTwo.FakeEntityTwoId).ConfigureAwait(false);

			Assert.True(success);

			var shouldBeNull = await _adapterFixture.FakeEntityTwoAdapter.GetAsync(fakeEntityTwo.Id, fakeEntityTwo.FakeEntityTwoId).ConfigureAwait(false);

			Assert.Null(shouldBeNull);

			_adapterFixture.CreatedFakeEntityTwos.Clear();
		}
		#endregion DeleteAsync

		#region DeleteBulkAsync
		[Fact, TestPriorityOrder(200)]
		public async Task DeleteBulkAsync_Prep()
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
		public async Task DeleteBulkAsync_Success()
		{
			var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
			var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
			var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

			var idsToDelete = new List<(string id, string partitionKey)>
			{
				(fakeEntityTwo1.Id, fakeEntityTwo1.FakeEntityTwoId),
				(fakeEntityTwo2.Id, fakeEntityTwo2.FakeEntityTwoId),
				(fakeEntityTwo3.Id, fakeEntityTwo3.FakeEntityTwoId)
			};

			var success = await _adapterFixture.FakeEntityTwoAdapter.DeleteBulkAsync(idsToDelete).ConfigureAwait(false);

			Assert.True(success);

			var idsToCheck = new List<(string id, string partitionKey)>
			{
				(fakeEntityTwo1.Id, fakeEntityTwo1.FakeEntityTwoId),
				(fakeEntityTwo2.Id, fakeEntityTwo2.FakeEntityTwoId),
				(fakeEntityTwo3.Id, fakeEntityTwo3.FakeEntityTwoId)
			};

			var shouldBeNull = await _adapterFixture.FakeEntityTwoAdapter.GetManyAsync(idsToCheck).ConfigureAwait(false);

			Assert.Empty(shouldBeNull);

			_adapterFixture.CreatedFakeEntityTwos.Clear();
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