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
    [Collection(nameof(PatchAdapterApiTests))]
    [TestCollectionPriorityOrder(8)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class PatchAdapterApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public PatchAdapterApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region PatchAsync
		[Fact, TestPriorityOrder(100)]
		public async Task PatchAsync_Prep()
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
				SearchableLongValue = 15L,
				CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var savedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.CreateAsync(fakeEntityTwo).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo, savedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwo);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task PatchAsync_Success()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var newValue = "fakeEntityTwo: UPDATED";

			var patchOperations = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newValue),
			};

			var patchedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.PatchAsync(fakeEntityTwo.Id, fakeEntityTwo.FakeEntityTwoId, patchOperations).ConfigureAwait(false);

			Assert.Equal(newValue, patchedFakeEntityTwo.SearchableStringValue);

			_adapterFixture.CreatedFakeEntityTwos[0] = patchedFakeEntityTwo;
		}

		[Fact, TestPriorityOrder(102)]
		public async Task PatchAsync_SuccessSequential()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var newString = "fakeEntityTwo: UPDATED Again";
			var increment = 17L;

			var patchOperations = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Replace("/string", fakeEntityTwo.SearchableStringValue),
				PatchOperation.Add("/added", "addedValue"),
				PatchOperation.Remove("/added")
			};

			var patchedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.PatchAsync(fakeEntityTwo.Id, fakeEntityTwo.FakeEntityTwoId, patchOperations).ConfigureAwait(false);

			Assert.Equal(fakeEntityTwo.SearchableStringValue, patchedFakeEntityTwo.SearchableStringValue);
			Assert.Equal(fakeEntityTwo.SearchableLongValue + increment, patchedFakeEntityTwo.SearchableLongValue);

			_adapterFixture.CreatedFakeEntityTwos[0] = patchedFakeEntityTwo;
		}

		[Fact, TestPriorityOrder(103)]
		public async Task PatchAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion PatchAsync

		#region PatchBulkAsync
		[Fact, TestPriorityOrder(200)]
		public async Task PatchBulkAsync_Prep()
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
				SearchableLongValue = 15L,
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
				SearchableLongValue = 35L,
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
		public async Task PatchBulkAsync_Success()
		{
			var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
			var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
			var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

			var newValue1 = "fakeEntityTwo1: UPDATED";
			var newValue2 = "fakeEntityTwo2: UPDATED";
			var newValue3 = "fakeEntityTwo3: UPDATED";

			var patchOperations1 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newValue1),
			};

			var patchOperations2 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newValue2),
			};

			var patchOperations3 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newValue3),
			};

			var patchOperations = new List<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)>
			{
				(fakeEntityTwo1.Id, fakeEntityTwo1.FakeEntityTwoId, patchOperations1),
				(fakeEntityTwo2.Id, fakeEntityTwo2.FakeEntityTwoId, patchOperations2),
				(fakeEntityTwo3.Id, fakeEntityTwo3.FakeEntityTwoId, patchOperations3)
			};

			var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.PatchBulkAsync(patchOperations).ConfigureAwait(false);

			Assert.Equal(newValue1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id).SearchableStringValue);
			Assert.Equal(newValue2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id).SearchableStringValue);
			Assert.Equal(newValue3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id).SearchableStringValue);

			_adapterFixture.CreatedFakeEntityTwos[0] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id);
			_adapterFixture.CreatedFakeEntityTwos[1] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id);
			_adapterFixture.CreatedFakeEntityTwos[2] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id);
		}

		[Fact, TestPriorityOrder(202)]
		public async Task PatchBulkAsync_SuccessSequential()
		{
			var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
			var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
			var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

			var newString1 = "fakeEntityTwo1: UPDATED Again";
			var newString2 = "fakeEntityTwo2: UPDATED Again";
			var newString3 = "fakeEntityTwo3: UPDATED Again";
			var increment = 17L;

			var patchOperations1 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString1),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Replace("/string", fakeEntityTwo1.SearchableStringValue)
			};

			var patchOperations2 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString2),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Replace("/string", fakeEntityTwo2.SearchableStringValue)
			};

			var patchOperations3 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString3),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Replace("/string", fakeEntityTwo3.SearchableStringValue),
				PatchOperation.Increment("/long", increment),
				PatchOperation.Replace("/string", newString3)
			};

			var patchOperations = new List<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)>
			{
				(fakeEntityTwo1.Id, fakeEntityTwo1.FakeEntityTwoId, patchOperations1),
				(fakeEntityTwo2.Id, fakeEntityTwo2.FakeEntityTwoId, patchOperations2),
				(fakeEntityTwo3.Id, fakeEntityTwo3.FakeEntityTwoId, patchOperations3)
			};

			var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.PatchBulkAsync(patchOperations).ConfigureAwait(false);

			Assert.Equal(fakeEntityTwo1.SearchableStringValue, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id).SearchableStringValue);
			Assert.Equal(fakeEntityTwo1.SearchableLongValue + increment, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id).SearchableLongValue);

			Assert.Equal(fakeEntityTwo2.SearchableStringValue, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id).SearchableStringValue);
			Assert.Equal(fakeEntityTwo2.SearchableLongValue + increment, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id).SearchableLongValue);

			Assert.Equal(newString3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id).SearchableStringValue);
			Assert.Equal(fakeEntityTwo3.SearchableLongValue + (increment * 3), savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id).SearchableLongValue);

			_adapterFixture.CreatedFakeEntityTwos[0] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id);
			_adapterFixture.CreatedFakeEntityTwos[1] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id);
			_adapterFixture.CreatedFakeEntityTwos[2] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id);
		}

		[Fact, TestPriorityOrder(203)]
		public async Task PatchBulkAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion PatchBulkAsync

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