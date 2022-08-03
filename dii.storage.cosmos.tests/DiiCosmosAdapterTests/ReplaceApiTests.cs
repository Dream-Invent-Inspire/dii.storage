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
using static dii.storage.cosmos.tests.Models.Enums;

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
		[Theory, TestPriorityOrder(100), ClassData(typeof(SingleFakeEntityData))]
		public async Task ReplaceAsync_Prep(FakeEntity fakeEntity)
		{
			// Ensure context exists and is initialized.
			TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task ReplaceAsync_Success()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];

			var replacementFakeEntity = new FakeEntity
			{
				Id = fakeEntity.Id,
				FakeEntityId = fakeEntity.FakeEntityId,
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"replacementFakeEntity: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"replacementFakeEntity: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"replacementFakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"replacementFakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"replacementFakeEntity: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"replacementFakeEntity: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
				ComplexSearchable = new FakeSearchableEntity
				{
					Soaps = "Dove",
					Tacos = "Bell"
				}
			};

			var replacedFakeEntity = await _adapterFixture.FakeEntityAdapter.ReplaceAsync(replacementFakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity, replacedFakeEntity);

			_adapterFixture.CreatedFakeEntities[0] = replacedFakeEntity;
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
		[Theory, TestPriorityOrder(200), ClassData(typeof(MultipleFakeEntityData))]
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

		[Fact, TestPriorityOrder(201)]
		public async Task ReplaceBulkAsync_Success()
		{
			var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
			var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var replacementFakeEntity1 = new FakeEntity
			{
				Id = fakeEntity1.Id,
				FakeEntityId = fakeEntity1.FakeEntityId,
				SearchableIntegerValue = 2,
				SearchableDecimalValue = 2.02m,
				SearchableStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"replacementFakeEntity1: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity1: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
				SearchableEnumValue = FakeEnum.Second,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 200,
					PackedDecimalValue = 200.02m,
					PackedStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
					PackedEnumValue = FakeEnum.Fourth
				},
				CompressedIntegerValue = 20,
				CompressedDecimalValue = 20.02m,
				CompressedStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"replacementFakeEntity1: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity1: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
				CompressedEnumValue = FakeEnum.Third,
				ComplexSearchable = new FakeSearchableEntity
				{
					Soaps = "Dove1",
					Tacos = "Bell1"
				}
			};

			var replacementFakeEntity2 = new FakeEntity
			{
				Id = fakeEntity2.Id,
				FakeEntityId = fakeEntity2.FakeEntityId,
				SearchableIntegerValue = 3,
				SearchableDecimalValue = 3.03m,
				SearchableStringValue = $"replacementFakeEntity2: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"replacementFakeEntity2: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"replacementFakeEntity2: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
				SearchableEnumValue = FakeEnum.Third,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 300,
					PackedDecimalValue = 300.03m,
					PackedStringValue = $"replacementFakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"replacementFakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"replacementFakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
					PackedEnumValue = FakeEnum.Fifth
				},
				CompressedIntegerValue = 30,
				CompressedDecimalValue = 30.03m,
				CompressedStringValue = $"replacementFakeEntity2: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"replacementFakeEntity2: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"replacementFakeEntity2: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
				CompressedEnumValue = FakeEnum.Fourth,
				ComplexSearchable = new FakeSearchableEntity
				{
					Soaps = "Dove2",
					Tacos = "Bell2"
				}
			};

            var replacementFakeEntity3 = new FakeEntity
			{
				Id = fakeEntity3.Id,
				FakeEntityId = fakeEntity3.FakeEntityId,
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"replacementFakeEntity3: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"replacementFakeEntity3: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"replacementFakeEntity3: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"replacementFakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"replacementFakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"replacementFakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"replacementFakeEntity3: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"replacementFakeEntity3: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"replacementFakeEntity3: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
				ComplexSearchable = new FakeSearchableEntity
				{
					Soaps = "Dove3",
					Tacos = "Bell3"
				}
			};

			var entitiesToReplace = new List<FakeEntity>
			{
				replacementFakeEntity1,
				replacementFakeEntity2,
				replacementFakeEntity3
			};

			var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.ReplaceBulkAsync(entitiesToReplace).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity1, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1.Id));
			TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity2, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity2.Id));
			TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity3, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id));

			_adapterFixture.CreatedFakeEntities[0] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1.Id);
			_adapterFixture.CreatedFakeEntities[1] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity2.Id);
			_adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id);
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