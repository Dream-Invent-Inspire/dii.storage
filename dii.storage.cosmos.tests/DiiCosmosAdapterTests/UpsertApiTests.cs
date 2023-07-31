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
    [Collection(nameof(UpsertApiTests))]
    [TestCollectionPriorityOrder(403)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class UpsertApiTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public UpsertApiTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region UpsertAsync
		[Fact, TestPriorityOrder(100)]
		public void UpsertAsync_Prep()
		{
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();
		}

		[Theory, TestPriorityOrder(101), ClassData(typeof(CreateFakeEntityDataWithResponseFlag))]
		public async Task UpsertAsync_CreateSuccess(FakeEntity fakeEntity, bool returnResult)
        {
            ItemRequestOptions requestOptions = null;

            if (!returnResult)
            {
                requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
            }

            var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity, requestOptions).ConfigureAwait(false);

            if (returnResult)
            {
                TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

                _adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
            }
            else
            {
                Assert.Null(savedFakeEntity);

                _adapterFixture.CreatedFakeEntities.Add(fakeEntity);
            }
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(ReplaceFakeEntityDataWithResponseFlag))]
		public async Task UpsertAsync_ReplaceSuccess(FakeEntity replacementFakeEntity, bool returnResult, int index)
        {
            var fakeEntity = _adapterFixture.CreatedFakeEntities[index];

            replacementFakeEntity.Id = fakeEntity.Id;
            replacementFakeEntity.FakeEntityId = fakeEntity.FakeEntityId;

            ItemRequestOptions requestOptions = null;

            if (!returnResult)
            {
                requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
            }

            var replacedFakeEntity = await _adapterFixture.FakeEntityAdapter.UpsertAsync(replacementFakeEntity, requestOptions).ConfigureAwait(false);

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

        [Fact, TestPriorityOrder(103)]
        public async Task UpsertAsync_Idempotency()
        {
            var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntity>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntities[0]));

            _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue = 999999;
            _adapterFixture.CreatedFakeEntities[0] = await _adapterFixture.FakeEntityAdapter.UpsertAsync(_adapterFixture.CreatedFakeEntities[0]).ConfigureAwait(false);

            Assert.NotEqual(toUpdate.DataVersion, _adapterFixture.CreatedFakeEntities[0].DataVersion);
            Assert.Equal(999999L, _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue);

            toUpdate.SearchableIntegerValue = 888888;

            var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityAdapter.UpsertAsync(toUpdate); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
        }

        [Fact, TestPriorityOrder(104)]
		public async Task UpsertAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
        }
        #endregion UpsertAsync

        #region UpsertBulkAsync
        [Fact, TestPriorityOrder(200)]
        public void UpsertBulkAsync_Prep()
        {
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();
        }

        [Theory, TestPriorityOrder(201), ClassData(typeof(CreateBulkFakeEntityDataWithResponseFlag))]
        public async Task UpsertBulkAsync_CreateSuccess(List<FakeEntity> fakeEntities, bool returnResult)
        {
            ItemRequestOptions requestOptions = null;

            if (!returnResult)
            {
                requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
            }

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(fakeEntities, requestOptions).ConfigureAwait(false);

            foreach (var fakeEntity in fakeEntities)
            {
                if (returnResult)
                {
                    TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity.Id));
                }
                else
                {
                    Assert.Null(savedFakeEntities);
                }
            }

            if (returnResult)
            {
                _adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
            }
            else
            {
                _adapterFixture.CreatedFakeEntities.AddRange(fakeEntities);
            }
        }

        [Theory, TestPriorityOrder(202), ClassData(typeof(ReplaceBulkFakeEntityDataWithResponseFlag))]
        public async Task UpsertBulkAsync_ReplaceSuccess(List<(FakeEntity, int)> replacementFakeEntities, bool returnResult)
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

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(replacementFakeEntities.Select(x => x.Item1).ToList(), requestOptions).ConfigureAwait(false);

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

        [Theory, TestPriorityOrder(203), ClassData(typeof(UpsertBulkMixedFakeEntityData))]
        public async Task UpsertBulkAsync_CreateAndReplaceSuccess(List<FakeEntity> fakeEntities)
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var fakeEntity400 = fakeEntities[0];
            var fakeEntity500 = fakeEntities[1];

            var replacementFakeEntity1000 = fakeEntities[2];
            replacementFakeEntity1000.FakeEntityId = fakeEntity1.Id;

            var replacementFakeEntity3000 = fakeEntities[3];
            replacementFakeEntity3000.FakeEntityId = fakeEntity3.Id;

            var entitiesToCreateOrReplace = new List<FakeEntity>
            {
                replacementFakeEntity1000,
                replacementFakeEntity3000,
                fakeEntity400,
                fakeEntity500
            };

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToCreateOrReplace).ConfigureAwait(false);

            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity1000, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1000.Id));
            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity3000, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3000.Id));
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity400, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity400.Id));
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity500, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity500.Id));

            _adapterFixture.CreatedFakeEntities[0] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1000.Id);
            _adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3000.Id);
            _adapterFixture.CreatedFakeEntities.Add(savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity400.Id));
            _adapterFixture.CreatedFakeEntities.Add(savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity500.Id));
        }

        [Fact, TestPriorityOrder(204)]
        public async Task UpsertBulkAsync_Idempotency()
        {
            var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntity>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntities[0]));

            _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue = 999999;

            var entitiesToCreateOrReplace = new List<FakeEntity>
            {
                _adapterFixture.CreatedFakeEntities[0]
            };

            var result = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToCreateOrReplace).ConfigureAwait(false);
            var updatedEntity = result.FirstOrDefault();

            Assert.NotEqual(toUpdate.DataVersion, updatedEntity.DataVersion);
            Assert.Equal(999999L, _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue);

            toUpdate.SearchableIntegerValue = 888888;

            entitiesToCreateOrReplace = new List<FakeEntity>
            {
                toUpdate
            };

            var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToCreateOrReplace); }).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
        }

        [Fact, TestPriorityOrder(205)]
        public async Task UpsertBulkAsync_Post()
        {
            await TestHelpers.DeleteAllFakeEntitiesAsync(_adapterFixture).ConfigureAwait(false);
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