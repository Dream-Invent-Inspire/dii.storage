using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Fixtures;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using dii.cosmos.tests.Utilities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests.CosmosTests
{
    [Collection(nameof(ReplaceAdapterApiTests))]
    [TestCollectionPriorityOrder(5)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ReplaceAdapterApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public ReplaceAdapterApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region ReplaceAsync
		[Fact, TestPriorityOrder(100)]
		public async Task ReplaceAsync_Prep()
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
		public async Task ReplaceAsync_Success()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var replacementFakeEntityTwo = new FakeEntityTwo
			{
				Id = fakeEntityTwo.Id,
				FakeEntityTwoId = fakeEntityTwo.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.ReplaceAsync(replacementFakeEntityTwo).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo, replacedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos[0] = replacedFakeEntityTwo;
		}

		[Fact, TestPriorityOrder(102)]
		public async Task ReplaceAsync_Idempotency()
		{
			var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntityTwo>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntityTwos[0]));

			_adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue = 999999L;
			_adapterFixture.CreatedFakeEntityTwos[0] = await _adapterFixture.FakeEntityTwoAdapter.ReplaceAsync(_adapterFixture.CreatedFakeEntityTwos[0]).ConfigureAwait(false);

			Assert.NotEqual(toUpdate.DataVersion, _adapterFixture.CreatedFakeEntityTwos[0].DataVersion);
			Assert.Equal(999999L, _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue);

			toUpdate.SearchableLongValue = 888888L;

			var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityTwoAdapter.ReplaceAsync(toUpdate); }).ConfigureAwait(false);

			Assert.NotNull(exception);
			Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
		}

		[Fact, TestPriorityOrder(103)]
		public async Task ReplaceAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion ReplaceAsync

		#region ReplaceBulkAsync
		[Fact, TestPriorityOrder(200)]
		public async Task ReplaceBulkAsync_Prep()
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
		public async Task ReplaceBulkAsync_Success()
		{
			var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
			var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
			var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

			var replacementFakeEntityTwo1 = new FakeEntityTwo
			{
				Id = fakeEntityTwo1.Id,
				FakeEntityTwoId = fakeEntityTwo1.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacementFakeEntityTwo2 = new FakeEntityTwo
			{
				Id = fakeEntityTwo2.Id,
				FakeEntityTwoId = fakeEntityTwo2.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacementFakeEntityTwo3 = new FakeEntityTwo
			{
				Id = fakeEntityTwo3.Id,
				FakeEntityTwoId = fakeEntityTwo3.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var entitiesToReplace = new List<FakeEntityTwo>
			{
				replacementFakeEntityTwo1,
				replacementFakeEntityTwo2,
				replacementFakeEntityTwo3
			};

			var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.ReplaceBulkAsync(entitiesToReplace).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo1.Id));
			TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo2.Id));
			TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo3.Id));

			_adapterFixture.CreatedFakeEntityTwos[0] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo1.Id);
			_adapterFixture.CreatedFakeEntityTwos[1] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo2.Id);
			_adapterFixture.CreatedFakeEntityTwos[2] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo3.Id);
		}

		[Fact, TestPriorityOrder(202)]
		public async Task ReplaceBulkAsync_Idempotency()
		{
			var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntityTwo>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntityTwos[0]));

			_adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue = 999999L;

			var entitiesToReplace = new List<FakeEntityTwo>
			{
				_adapterFixture.CreatedFakeEntityTwos[0]
			};

			var result = await _adapterFixture.FakeEntityTwoAdapter.ReplaceBulkAsync(entitiesToReplace).ConfigureAwait(false);
			var updatedEntity = result.FirstOrDefault();

			Assert.NotEqual(toUpdate.DataVersion, updatedEntity.DataVersion);
			Assert.Equal(999999L, _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue);

			toUpdate.SearchableLongValue = 888888L;

			entitiesToReplace = new List<FakeEntityTwo>
			{
				toUpdate
			};

			var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityTwoAdapter.ReplaceBulkAsync(entitiesToReplace); }).ConfigureAwait(false);

			Assert.NotNull(exception);
			Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
		}

		[Fact, TestPriorityOrder(203)]
		public async Task ReplaceBulkAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion ReplaceBulkAsync

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