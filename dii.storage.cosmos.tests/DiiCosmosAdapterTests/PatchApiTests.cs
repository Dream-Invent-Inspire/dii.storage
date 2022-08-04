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
    [Collection(nameof(PatchApiTests))]
    [TestCollectionPriorityOrder(404)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class PatchApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public PatchApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region PatchAsync
		[Theory, TestPriorityOrder(100), ClassData(typeof(CreateFakeEntityData))]
		public async Task PatchAsync_Prep(FakeEntity fakeEntity)
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Theory, TestPriorityOrder(101), ClassData(typeof(PatchDataWithResponseFlag))]
		public async Task PatchAsync_Success(List<(PatchOperation patchOperation, string expectedValue)> patchOperations, bool returnResult)
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];

			PatchItemRequestOptions requestOptions = null;

			if (!returnResult)
			{
				requestOptions = new PatchItemRequestOptions { EnableContentResponseOnWrite = false };
			}

			var patchedFakeEntity = await _adapterFixture.FakeEntityAdapter.PatchAsync(fakeEntity.Id, fakeEntity.FakeEntityId, patchOperations.Select(x => x.patchOperation).ToList(), requestOptions).ConfigureAwait(false);

			if (returnResult)
			{
				Assert.Equal(patchOperations.Select(x => x.expectedValue).FirstOrDefault(), patchedFakeEntity.SearchableStringValue);

				_adapterFixture.CreatedFakeEntities[0] = patchedFakeEntity;
			}
			else
			{
				Assert.Null(patchedFakeEntity);

				fakeEntity.SearchableStringValue = patchOperations.Select(x => x.expectedValue).FirstOrDefault();

				_adapterFixture.CreatedFakeEntities[0] = fakeEntity;
			}
		}

		[Fact, TestPriorityOrder(102)]
		public async Task PatchAsync_SuccessSequential()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];
			var newString = "fakeEntity: UPDATED Yet Again";
			var increment = 17;

			var patchOperations = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Replace("/string", fakeEntity.SearchableStringValue),
				PatchOperation.Add("/added", "addedValue"),
				PatchOperation.Remove("/added")
			};

			var patchedFakeEntity = await _adapterFixture.FakeEntityAdapter.PatchAsync(fakeEntity.Id, fakeEntity.FakeEntityId, patchOperations).ConfigureAwait(false);

			Assert.Equal(fakeEntity.SearchableStringValue, patchedFakeEntity.SearchableStringValue);
			Assert.Equal(fakeEntity.SearchableIntegerValue + increment, patchedFakeEntity.SearchableIntegerValue);

			_adapterFixture.CreatedFakeEntities[0] = patchedFakeEntity;
		}

		[Fact, TestPriorityOrder(103)]
		public async Task PatchAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
		}
		#endregion PatchAsync

		#region PatchBulkAsync
		[Theory, TestPriorityOrder(200), ClassData(typeof(CreateBulkFakeEntityData))]
		public async Task PatchBulkAsync_Prep(List<FakeEntity> fakeEntities)
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

		[Theory, TestPriorityOrder(201), ClassData(typeof(PatchBulkDataWithResponseFlag))]
		public async Task PatchBulkAsync_Success(List<(List<PatchOperation> patchOperation, string expectedValue)> patchOperations, bool returnResult)
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var patchBulkOperations = new List<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)>
            {
                (fakeEntity1.Id, fakeEntity1.FakeEntityId, patchOperations[0].patchOperation),
                (fakeEntity2.Id, fakeEntity2.FakeEntityId, patchOperations[1].patchOperation),
                (fakeEntity3.Id, fakeEntity3.FakeEntityId, patchOperations[2].patchOperation)
            };

			PatchItemRequestOptions requestOptions = null;

			if (!returnResult)
			{
				requestOptions = new PatchItemRequestOptions { EnableContentResponseOnWrite = false };
			}

			var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.PatchBulkAsync(patchBulkOperations, requestOptions).ConfigureAwait(false);

			if (returnResult)
			{
				Assert.Equal(patchOperations[0].expectedValue, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id).SearchableStringValue);
				Assert.Equal(patchOperations[1].expectedValue, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id).SearchableStringValue);
				Assert.Equal(patchOperations[2].expectedValue, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id).SearchableStringValue);

				_adapterFixture.CreatedFakeEntities[0] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id);
				_adapterFixture.CreatedFakeEntities[1] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id);
				_adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id);
			}
			else
			{
				Assert.Null(savedFakeEntities);

				fakeEntity1.SearchableStringValue = patchOperations[0].expectedValue;
				fakeEntity2.SearchableStringValue = patchOperations[1].expectedValue;
				fakeEntity3.SearchableStringValue = patchOperations[2].expectedValue;

				_adapterFixture.CreatedFakeEntities[0] = fakeEntity1;
				_adapterFixture.CreatedFakeEntities[1] = fakeEntity2;
				_adapterFixture.CreatedFakeEntities[2] = fakeEntity3;
			}
		}

		[Fact, TestPriorityOrder(202)]
		public async Task PatchBulkAsync_SuccessSequential()
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var newString1 = "fakeEntity1: UPDATED Again";
			var newString2 = "fakeEntity2: UPDATED Again";
			var newString3 = "fakeEntity3: UPDATED Again";
			var increment = 17;

			var patchOperations1 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString1),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Replace("/string", fakeEntity1.SearchableStringValue)
			};

			var patchOperations2 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString2),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Replace("/string", fakeEntity2.SearchableStringValue)
			};

			var patchOperations3 = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newString3),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Replace("/string", fakeEntity3.SearchableStringValue),
				PatchOperation.Increment("/int", increment),
				PatchOperation.Replace("/string", newString3)
			};

			var patchOperations = new List<(string id, string partitionKey, IReadOnlyList<PatchOperation> listOfPatchOperations)>
			{
				(fakeEntity1.Id, fakeEntity1.FakeEntityId, patchOperations1),
				(fakeEntity2.Id, fakeEntity2.FakeEntityId, patchOperations2),
				(fakeEntity3.Id, fakeEntity3.FakeEntityId, patchOperations3)
			};

			var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.PatchBulkAsync(patchOperations).ConfigureAwait(false);

			Assert.Equal(fakeEntity1.SearchableStringValue, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id).SearchableStringValue);
			Assert.Equal(fakeEntity1.SearchableIntegerValue + increment, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id).SearchableIntegerValue);

			Assert.Equal(fakeEntity2.SearchableStringValue, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id).SearchableStringValue);
			Assert.Equal(fakeEntity2.SearchableIntegerValue + increment, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id).SearchableIntegerValue);

			Assert.Equal(newString3, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id).SearchableStringValue);
			Assert.Equal(fakeEntity3.SearchableIntegerValue + (increment * 3), savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id).SearchableIntegerValue);

			_adapterFixture.CreatedFakeEntities[0] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity1.Id);
			_adapterFixture.CreatedFakeEntities[1] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id);
			_adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id);
		}

		[Fact, TestPriorityOrder(203)]
		public async Task PatchBulkAsync_Post()
		{
			await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
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