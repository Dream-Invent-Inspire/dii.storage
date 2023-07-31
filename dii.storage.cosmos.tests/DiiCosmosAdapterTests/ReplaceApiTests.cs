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
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests
{
    [Collection(nameof(ReplaceApiTests))]
    [TestCollectionPriorityOrder(402)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ReplaceApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public ReplaceApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region ReplaceAsync
		[Theory, TestPriorityOrder(100), ClassData(typeof(ReplaceFakeEntityData))]
		public async Task ReplaceAsync_Prep(FakeEntity fakeEntity)
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Theory, TestPriorityOrder(101), ClassData(typeof(ReplaceFakeEntityDataWithResponseFlag))]
		public async Task ReplaceAsync_Success(FakeEntity replacementFakeEntity, bool returnResult, int index)
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[index];

			replacementFakeEntity.Id = fakeEntity.Id;
			replacementFakeEntity.FakeEntityId = fakeEntity.FakeEntityId;

			ItemRequestOptions requestOptions = null;

			if (!returnResult)
			{
				requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			}

			var replacedFakeEntity = await _adapterFixture.FakeEntityAdapter.ReplaceAsync(replacementFakeEntity, requestOptions).ConfigureAwait(false);

			if (returnResult)
			{
				TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity, replacedFakeEntity);

				_adapterFixture.CreatedFakeEntities[index] = replacedFakeEntity;
			}
			else
			{
				Assert.Null(replacedFakeEntity);

				_adapterFixture.CreatedFakeEntities[index] = replacementFakeEntity;
			}
		}

		[Fact, TestPriorityOrder(102)]
		public async Task ReplaceAsync_Idempotency()
		{
			var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntity>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntities[0]));

			_adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue = 999999;
			_adapterFixture.CreatedFakeEntities[0] = await _adapterFixture.FakeEntityAdapter.ReplaceAsync(_adapterFixture.CreatedFakeEntities[0]).ConfigureAwait(false);

			Assert.NotEqual(toUpdate.DataVersion, _adapterFixture.CreatedFakeEntities[0].DataVersion);
			Assert.Equal(999999, _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue);

			toUpdate.SearchableIntegerValue = 888888;

			var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityAdapter.ReplaceAsync(toUpdate); }).ConfigureAwait(false);

			Assert.NotNull(exception);
			Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
		}

		[Fact, TestPriorityOrder(103)]
		public async Task ReplaceAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion ReplaceAsync

		#region ReplaceBulkAsync
		[Theory, TestPriorityOrder(200), ClassData(typeof(ReplaceBulkFakeEntityData))]
		public async Task ReplaceBulkAsync_Prep(List<FakeEntity> fakeEntities)
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

		[Theory, TestPriorityOrder(201), ClassData(typeof(ReplaceBulkFakeEntityDataWithResponseFlag))]
		public async Task ReplaceBulkAsync_Success(List<(FakeEntity, int)> replacementFakeEntities, bool returnResult)
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[replacementFakeEntities[0].Item2];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[replacementFakeEntities[1].Item2];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[replacementFakeEntities[2].Item2];

			replacementFakeEntities[0].Item1.FakeEntityId = fakeEntity1.FakeEntityId;
			replacementFakeEntities[1].Item1.FakeEntityId = fakeEntity2.FakeEntityId;
			replacementFakeEntities[2].Item1.FakeEntityId = fakeEntity3.FakeEntityId;

			ItemRequestOptions requestOptions = null;

			if (!returnResult)
			{
				requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			}

			var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.ReplaceBulkAsync(replacementFakeEntities.Select(x => x.Item1).ToList(), requestOptions).ConfigureAwait(false);

			foreach (var replacementFakeEntity in replacementFakeEntities)
			{
				if (returnResult)
				{
					TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity.Item1, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity.Item1.Id));
					_adapterFixture.CreatedFakeEntities[replacementFakeEntity.Item2] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity.Item1.Id);
				}
				else
				{
					Assert.Null(savedFakeEntities);
					_adapterFixture.CreatedFakeEntities[replacementFakeEntity.Item2] = replacementFakeEntity.Item1;
				}
			}
		}

		[Fact, TestPriorityOrder(202)]
		public async Task ReplaceBulkAsync_Idempotency()
		{
			var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntity>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntities[0]));

			_adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue = 999999;

			var entitiesToReplace = new List<FakeEntity>
			{
				_adapterFixture.CreatedFakeEntities[0]
			};

			var result = await _adapterFixture.FakeEntityAdapter.ReplaceBulkAsync(entitiesToReplace).ConfigureAwait(false);
			var updatedEntity = result.FirstOrDefault();

			Assert.NotEqual(toUpdate.DataVersion, updatedEntity.DataVersion);
			Assert.Equal(999999L, _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue);

			toUpdate.SearchableIntegerValue = 888888;

			entitiesToReplace = new List<FakeEntity>
			{
				toUpdate
			};

			var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityAdapter.ReplaceBulkAsync(entitiesToReplace); }).ConfigureAwait(false);

			Assert.NotNull(exception);
			Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
		}

		[Fact, TestPriorityOrder(203)]
		public async Task ReplaceBulkAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
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