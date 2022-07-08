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
    [Collection(nameof(UpsertAdapterApiTests))]
    [TestCollectionPriorityOrder(6)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class UpsertAdapterApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public UpsertAdapterApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region UpsertAsync
		[Fact, TestPriorityOrder(100)]
		public async Task UpsertAsync_Prep()
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

			var savedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.CreateAsync(fakeEntityTwo1).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, savedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwo);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task UpsertAsync_CreateSuccess()
		{
			var fakeEntityTwo2 = new FakeEntityTwo
			{
				Id = DateTime.Now.AddMinutes(-2).Ticks.ToString(),
				FakeEntityTwoId = DateTime.Now.AddMinutes(-3).Ticks.ToString(),
				SearchableStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"fakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.UpsertAsync(fakeEntityTwo2).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, replacedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos.Add(replacedFakeEntityTwo);
		}

		[Fact, TestPriorityOrder(102)]
		public async Task UpsertAsync_ReplaceSuccess()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var replacementFakeEntityTwo1 = new FakeEntityTwo
			{
				Id = fakeEntityTwo.Id,
				FakeEntityTwoId = fakeEntityTwo.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.UpsertAsync(replacementFakeEntityTwo1).ConfigureAwait(false);

			TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo1, replacedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos[0] = replacedFakeEntityTwo;
        }

        [Fact, TestPriorityOrder(103)]
        public async Task UpsertAsync_Idempotency()
        {
            var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntityTwo>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntityTwos[0]));

            _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue = 999999L;
            _adapterFixture.CreatedFakeEntityTwos[0] = await _adapterFixture.FakeEntityTwoAdapter.UpsertAsync(_adapterFixture.CreatedFakeEntityTwos[0]).ConfigureAwait(false);

            Assert.NotEqual(toUpdate.DataVersion, _adapterFixture.CreatedFakeEntityTwos[0].DataVersion);
            Assert.Equal(999999L, _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue);

            toUpdate.SearchableLongValue = 888888L;

            var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityTwoAdapter.UpsertAsync(toUpdate); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
        }

        [Fact, TestPriorityOrder(104)]
		public async Task UpsertAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion UpsertAsync

        #region UpsertBulkAsync
        [Fact, TestPriorityOrder(200)]
        public async Task UpsertBulkAsync_Prep()
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

            var entitiesToCreate = new List<FakeEntityTwo>
            {
                fakeEntityTwo1,
                fakeEntityTwo2
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.CreateBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo1.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo2.Id));

            _adapterFixture.CreatedFakeEntityTwos.AddRange(savedFakeEntityTwos);
        }

        [Fact, TestPriorityOrder(201)]
        public async Task UpsertBulkAsync_CreateSuccess()
        {
            var fakeEntityTwo3 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-4).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-5).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo4 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-6).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-7).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo4: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo4: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToCreate = new List<FakeEntityTwo>
            {
                fakeEntityTwo3,
                fakeEntityTwo4
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.UpsertBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo3.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo4, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo4.Id));

            _adapterFixture.CreatedFakeEntityTwos.AddRange(savedFakeEntityTwos);
        }

        [Fact, TestPriorityOrder(202)]
        public async Task UpsertBulkAsync_ReplaceSuccess()
        {
            var fakeEntityTwo2 = _adapterFixture.CreatedFakeEntityTwos[1];
            var fakeEntityTwo4 = _adapterFixture.CreatedFakeEntityTwos[3];

            var replacementFakeEntityTwo2 = new FakeEntityTwo
            {
                Id = fakeEntityTwo2.Id,
                FakeEntityTwoId = fakeEntityTwo2.FakeEntityTwoId,
                SearchableStringValue = $"replacementFakeEntityTwo2: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"replacementFakeEntityTwo2: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var replacementFakeEntityTwo4 = new FakeEntityTwo
            {
                Id = fakeEntityTwo4.Id,
                FakeEntityTwoId = fakeEntityTwo4.FakeEntityTwoId,
                SearchableStringValue = $"replacementFakeEntityTwo4: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"replacementFakeEntityTwo4: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToReplace = new List<FakeEntityTwo>
            {
                replacementFakeEntityTwo2,
                replacementFakeEntityTwo4
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.UpsertBulkAsync(entitiesToReplace).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo2, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo2.Id));
            TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo4, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo4.Id));

            _adapterFixture.CreatedFakeEntityTwos[1] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo2.Id);
            _adapterFixture.CreatedFakeEntityTwos[3] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo4.Id);
        }

        [Fact, TestPriorityOrder(203)]
        public async Task UpsertBulkAsync_CreateAndReplaceSuccess()
        {
            var fakeEntityTwo1 = _adapterFixture.CreatedFakeEntityTwos[0];
            var fakeEntityTwo3 = _adapterFixture.CreatedFakeEntityTwos[2];

            var fakeEntityTwo5 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-8).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-9).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo5: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo5: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var fakeEntityTwo6 = new FakeEntityTwo
            {
                Id = DateTime.Now.AddMinutes(-10).Ticks.ToString(),
                FakeEntityTwoId = DateTime.Now.AddMinutes(-11).Ticks.ToString(),
                SearchableStringValue = $"fakeEntityTwo6: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityTwo6: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var replacementFakeEntityTwo1 = new FakeEntityTwo
            {
                Id = fakeEntityTwo1.Id,
                FakeEntityTwoId = fakeEntityTwo1.FakeEntityTwoId,
                SearchableStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"replacementFakeEntityTwo1: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var replacementFakeEntityTwo3 = new FakeEntityTwo
            {
                Id = fakeEntityTwo3.Id,
                FakeEntityTwoId = fakeEntityTwo3.FakeEntityTwoId,
                SearchableStringValue = $"replacementFakeEntityTwo3: {nameof(FakeEntityTwo.SearchableStringValue)}",
                CompressedStringValue = $"replacementFakeEntityTwo3: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entitiesToCreateOrReplace = new List<FakeEntityTwo>
            {
                replacementFakeEntityTwo1,
                replacementFakeEntityTwo3,
                fakeEntityTwo5,
                fakeEntityTwo6
            };

            var savedFakeEntityTwos = await _adapterFixture.FakeEntityTwoAdapter.UpsertBulkAsync(entitiesToCreateOrReplace).ConfigureAwait(false);

            TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo1, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo1.Id));
            TestHelpers.AssertFakeEntityTwosMatch(replacementFakeEntityTwo3, savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo3.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo5, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo5.Id));
            TestHelpers.AssertFakeEntityTwosMatch(fakeEntityTwo6, savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo6.Id));

            _adapterFixture.CreatedFakeEntityTwos[0] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo1.Id);
            _adapterFixture.CreatedFakeEntityTwos[2] = savedFakeEntityTwos.FirstOrDefault(x => x.Id == replacementFakeEntityTwo3.Id);
            _adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo5.Id));
            _adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwos.FirstOrDefault(x => x.Id == fakeEntityTwo6.Id));
        }

        [Fact, TestPriorityOrder(204)]
        public async Task UpsertBulkAsync_Idempotency()
        {
            var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntityTwo>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntityTwos[0]));

            _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue = 999999L;

            var entitiesToCreateOrReplace = new List<FakeEntityTwo>
            {
                _adapterFixture.CreatedFakeEntityTwos[0]
            };

            var result = await _adapterFixture.FakeEntityTwoAdapter.UpsertBulkAsync(entitiesToCreateOrReplace).ConfigureAwait(false);
            var updatedEntity = result.FirstOrDefault();

            Assert.NotEqual(toUpdate.DataVersion, updatedEntity.DataVersion);
            Assert.Equal(999999L, _adapterFixture.CreatedFakeEntityTwos[0].SearchableLongValue);

            toUpdate.SearchableLongValue = 888888L;

            entitiesToCreateOrReplace = new List<FakeEntityTwo>
            {
                toUpdate
            };

            var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityTwoAdapter.UpsertBulkAsync(entitiesToCreateOrReplace); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
        }

        [Fact, TestPriorityOrder(205)]
        public async Task UpsertBulkAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntityTwosAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion UpsertBulkAsync

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