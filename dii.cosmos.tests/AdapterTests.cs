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
using static dii.cosmos.tests.Models.Enums;

namespace dii.cosmos.tests
{
    [Collection(nameof(AdapterTests))]
    [TestCollectionPriorityOrder(3)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class AdapterTests : IClassFixture<AdapterFixture>
    {
        private readonly AdapterFixture _adapterFixture;

        public AdapterTests(AdapterFixture adapterFixture)
        {
            _adapterFixture = adapterFixture ?? throw new ArgumentNullException(nameof(adapterFixture));
        }

		#region Scenario 1
		[Fact, TestPriorityOrder(1)]
		public async Task S1a__Save_FakeEntity()
		{
			var fakeEntity = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 1,
				SearchableDecimalValue = 1.01m,
				SearchableStringValue = $"fakeEntity: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
                {
					$"fakeEntity: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"fakeEntity: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(1),
				SearchableEnumValue = FakeEnum.First,
				CompressedPackedEntity = new FakeMessagePackEntity
                {
					PackedIntegerValue = 100,
					PackedDecimalValue = 100.01m,
					PackedStringValue = $"fakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"fakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"fakeEntity: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(100),
					PackedEnumValue = FakeEnum.Third
				},
				CompressedIntegerValue = 10,
				CompressedDecimalValue = 10.01m,
				CompressedStringValue = $"fakeEntity: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"fakeEntity: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"fakeEntity: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(10),
				CompressedEnumValue = FakeEnum.Second
			};

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity).ConfigureAwait(false);
			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);

            AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);
		}

		[Fact, TestPriorityOrder(2)]
		public async Task S1b__Get_FakeEntity()
		{
			var fakeEntity = _adapterFixture.CreatedFakeEntities[0];
			var fetchedFakeEntity = await _adapterFixture.FakeEntityAdapter.GetAsync(fakeEntity.Id, new PartitionKey(fakeEntity.FakeEntityId)).ConfigureAwait(false);

            AssertFakeEntitiesMatch(fakeEntity, fetchedFakeEntity, true);
		}

		[Fact, TestPriorityOrder(3)]
		public async Task S1c__Idempotent_FakeEntity()
		{
			var toUpdate = _adapterFixture.Optimizer.UnpackageFromJson<FakeEntity>(_adapterFixture.Optimizer.PackageToJson(_adapterFixture.CreatedFakeEntities[0]));

			_adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue = 999999;
			_adapterFixture.CreatedFakeEntities[0] = await _adapterFixture.FakeEntityAdapter.UpsertAsync(_adapterFixture.CreatedFakeEntities[0]).ConfigureAwait(false);

			Assert.NotEqual(toUpdate.DataVersion, _adapterFixture.CreatedFakeEntities[0].DataVersion);
			Assert.Equal(999999, _adapterFixture.CreatedFakeEntities[0].SearchableIntegerValue);

			toUpdate.SearchableIntegerValue = 888888;

			var exception = await Assert.ThrowsAsync<CosmosException>(() => { return _adapterFixture.FakeEntityAdapter.UpsertAsync(toUpdate); }).ConfigureAwait(false);

			Assert.NotNull(exception);
			Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
		}

		[Fact, TestPriorityOrder(4)]
		public async Task S1d__Delete_FakeEntity()
		{
			var success = await _adapterFixture.FakeEntityAdapter.DeleteAsync(_adapterFixture.CreatedFakeEntities[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[0].FakeEntityId)).ConfigureAwait(false);
			var shouldBeNull = await _adapterFixture.FakeEntityAdapter.GetAsync(_adapterFixture.CreatedFakeEntities[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[0].FakeEntityId)).ConfigureAwait(false);

			Assert.True(success);
			Assert.Null(shouldBeNull);

			_adapterFixture.CreatedFakeEntities.Clear();
		}
		#endregion Scenario 1

		#region Scenario 2
		[Fact, TestPriorityOrder(5)]
		public async Task S2a__Save_FakeEntity()
		{
			var fakeEntity1 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 2,
				SearchableDecimalValue = 2.02m,
				SearchableStringValue = $"fakeEntity1: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"fakeEntity1: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"fakeEntity1: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
				SearchableEnumValue = FakeEnum.Second,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 200,
					PackedDecimalValue = 200.02m,
					PackedStringValue = $"fakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"fakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"fakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
					PackedEnumValue = FakeEnum.Fourth
				},
				CompressedIntegerValue = 20,
				CompressedDecimalValue = 20.02m,
				CompressedStringValue = $"fakeEntity1: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"fakeEntity1: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"fakeEntity1: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
				CompressedEnumValue = FakeEnum.Third
			};

			var savedFakeEntity1 = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity1).ConfigureAwait(false);
			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity1);

            AssertFakeEntitiesMatch(fakeEntity1, savedFakeEntity1);

			var fakeEntity2 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 3,
				SearchableDecimalValue = 3.03m,
				SearchableStringValue = $"fakeEntity2: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"fakeEntity2: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"fakeEntity2: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
				SearchableEnumValue = FakeEnum.Third,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 300,
					PackedDecimalValue = 300.03m,
					PackedStringValue = $"fakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"fakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"fakeEntity2: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
					PackedEnumValue = FakeEnum.Fifth
				},
				CompressedIntegerValue = 30,
				CompressedDecimalValue = 30.03m,
				CompressedStringValue = $"fakeEntity2: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"fakeEntity2: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"fakeEntity2: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
				CompressedEnumValue = FakeEnum.Fourth
			};

			var savedFakeEntity2 = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity2).ConfigureAwait(false);
			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity2);

            AssertFakeEntitiesMatch(fakeEntity2, savedFakeEntity2);

			var fakeEntity3 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"fakeEntity3: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"fakeEntity3: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"fakeEntity3: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"fakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"fakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"fakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"fakeEntity3: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"fakeEntity3: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"fakeEntity3: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth
			};

			var savedFakeEntity3 = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity3).ConfigureAwait(false);
			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity3);

            AssertFakeEntitiesMatch(fakeEntity3, savedFakeEntity3);
		}

		[Fact, TestPriorityOrder(6)]
		public async Task S2b__Get_FakeEntity()
		{
			var persistedFakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
			var persistedFakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

			var idsToFetch = new Dictionary<string, string>()
			{
				{ $"@id{persistedFakeEntity1.Id}", persistedFakeEntity1.Id },
				{ $"@id{persistedFakeEntity3.Id}", persistedFakeEntity3.Id }
			};

			var queryDefinition = new QueryDefinition($"SELECT * FROM fakeentity fe WHERE fe.id IN ({string.Join(", ", idsToFetch.Keys)})");

			foreach (var id in idsToFetch)
			{
				queryDefinition.WithParameter(id.Key, id.Value);
			}

			var fetchedFakeEntities = await _adapterFixture.FakeEntityAdapter.GetPagedAsync(queryDefinition).ConfigureAwait(false);

			var fetchedFakeEntity1 = fetchedFakeEntities.FirstOrDefault(x => x.Id == persistedFakeEntity1.Id);
			Assert.NotNull(fetchedFakeEntity1);

            AssertFakeEntitiesMatch(persistedFakeEntity1, fetchedFakeEntity1, true);

			var fetchedFakeEntity3 = fetchedFakeEntities.FirstOrDefault(x => x.Id == persistedFakeEntity3.Id);
			Assert.NotNull(fetchedFakeEntity3);

            AssertFakeEntitiesMatch(persistedFakeEntity3, fetchedFakeEntity3, true);
		}

		[Fact, TestPriorityOrder(7)]
		public async Task S2c__Delete_FakeEntity()
		{
			var success1 = await _adapterFixture.FakeEntityAdapter.DeleteAsync(_adapterFixture.CreatedFakeEntities[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[0].FakeEntityId)).ConfigureAwait(false);
			var shouldBeNull1 = await _adapterFixture.FakeEntityAdapter.GetAsync(_adapterFixture.CreatedFakeEntities[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[0].FakeEntityId)).ConfigureAwait(false);

			Assert.True(success1);
			Assert.Null(shouldBeNull1);

			var success2 = await _adapterFixture.FakeEntityAdapter.DeleteAsync(_adapterFixture.CreatedFakeEntities[1].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[1].FakeEntityId)).ConfigureAwait(false);
			var shouldBeNull2 = await _adapterFixture.FakeEntityAdapter.GetAsync(_adapterFixture.CreatedFakeEntities[1].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[1].FakeEntityId)).ConfigureAwait(false);

			Assert.True(success2);
			Assert.Null(shouldBeNull2);

			var success3 = await _adapterFixture.FakeEntityAdapter.DeleteAsync(_adapterFixture.CreatedFakeEntities[2].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[2].FakeEntityId)).ConfigureAwait(false);
			var shouldBeNull3 = await _adapterFixture.FakeEntityAdapter.GetAsync(_adapterFixture.CreatedFakeEntities[2].Id, new PartitionKey(_adapterFixture.CreatedFakeEntities[2].FakeEntityId)).ConfigureAwait(false);

			Assert.True(success3);
			Assert.Null(shouldBeNull3);

			_adapterFixture.CreatedFakeEntities = null;
		}
		#endregion Scenario 2

		#region Scenario 3
		[Fact, TestPriorityOrder(8)]
		public async Task S3a__Save_FakeEntityTwo()
		{
			var fakeEntityTwo = new FakeEntityTwo
			{
				Id = DateTime.Now.Ticks.ToString(),
				FakeEntityTwoId = DateTime.Now.Ticks.ToString(),
				SearchableStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var savedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.CreateAsync(fakeEntityTwo).ConfigureAwait(false);
			_adapterFixture.CreatedFakeEntityTwos.Add(savedFakeEntityTwo);

			AssertFakeEntityTwosMatch(fakeEntityTwo, savedFakeEntityTwo);
		}

		[Fact, TestPriorityOrder(9)]
		public async Task S3b__GetMany_FakeEntityTwo()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];

			var items = new List<(string, PartitionKey)>
			{
				(fakeEntityTwo.Id, new PartitionKey(fakeEntityTwo.FakeEntityTwoId))
			};

			var fetchedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.GetManyAsync(items).ConfigureAwait(false);

			Assert.NotNull(fetchedFakeEntityTwo);
			Assert.Single(fetchedFakeEntityTwo);

			AssertFakeEntityTwosMatch(fakeEntityTwo, fetchedFakeEntityTwo.FirstOrDefault(), true);
		}

		[Fact, TestPriorityOrder(10)]
		public async Task S3c__Patch_FakeEntityTwo()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var newValue = "fakeEntityTwo: UPDATED";

			var patchOperations = new List<PatchOperation>()
			{
				PatchOperation.Replace("/string", newValue),
			};

			var patchedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.PatchAsync(fakeEntityTwo.Id, new PartitionKey(fakeEntityTwo.FakeEntityTwoId), patchOperations).ConfigureAwait(false);

			Assert.Equal(newValue, patchedFakeEntityTwo.SearchableStringValue);

			_adapterFixture.CreatedFakeEntityTwos[0] = patchedFakeEntityTwo;
		}

		[Fact, TestPriorityOrder(11)]
		public async Task S3d__Replace_FakeEntityTwo()
		{
			var fakeEntityTwo = _adapterFixture.CreatedFakeEntityTwos[0];
			var replacementFakeEntityTwo = new FakeEntityTwo
			{
				Id = DateTime.Now.Ticks.ToString(),
				FakeEntityTwoId = fakeEntityTwo.FakeEntityTwoId,
				SearchableStringValue = $"replacementFakeEntityTwo: {nameof(FakeEntityTwo.SearchableStringValue)}",
				CompressedStringValue = $"replacementFakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
			};

			var replacedFakeEntityTwo = await _adapterFixture.FakeEntityTwoAdapter.ReplaceAsync(replacementFakeEntityTwo, fakeEntityTwo.Id, new PartitionKey(fakeEntityTwo.FakeEntityTwoId)).ConfigureAwait(false);

			AssertFakeEntityTwosMatch(replacementFakeEntityTwo, replacedFakeEntityTwo);

			_adapterFixture.CreatedFakeEntityTwos[0] = replacedFakeEntityTwo;
		}

		[Fact, TestPriorityOrder(12)]
		public async Task S3e__Delete_FakeEntityTwo()
		{
			var success = await _adapterFixture.FakeEntityTwoAdapter.DeleteAsync(_adapterFixture.CreatedFakeEntityTwos[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntityTwos[0].FakeEntityTwoId)).ConfigureAwait(false);
			var shouldBeNull = await _adapterFixture.FakeEntityTwoAdapter.GetAsync(_adapterFixture.CreatedFakeEntityTwos[0].Id, new PartitionKey(_adapterFixture.CreatedFakeEntityTwos[0].FakeEntityTwoId)).ConfigureAwait(false);

			Assert.True(success);
			Assert.Null(shouldBeNull);

			_adapterFixture.CreatedFakeEntityTwos.Clear();
		}
		#endregion Scenario 3

		#region Teardown
		[Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            await TestHelpers.TeardownCosmosDbAsync().ConfigureAwait(false);

            TestHelpers.ResetContextInstance();
            TestHelpers.ResetOptimizerInstance();
        }
		#endregion

		#region Helper Methods
		private static void AssertFakeEntitiesMatch(FakeEntity expected, FakeEntity actual, bool checkPrimitiveProperties = false)
        {
			// Searchable Fields
			Assert.Equal(expected.FakeEntityId, actual.FakeEntityId);
			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.SearchableIntegerValue, actual.SearchableIntegerValue);
			Assert.Equal(expected.SearchableDecimalValue, actual.SearchableDecimalValue);
			Assert.Equal(expected.SearchableStringValue, actual.SearchableStringValue);
			Assert.Equal(expected.SearchableGuidValue, actual.SearchableGuidValue);
			Assert.Equal(expected.SearchableListValue.Count, actual.SearchableListValue.Count);

			for (var i = 0; i < expected.SearchableListValue.Count; i++)
			{
				Assert.Equal(expected.SearchableListValue[i], actual.SearchableListValue[i]);
			}

			Assert.Equal(expected.SearchableDateTimeValue, actual.SearchableDateTimeValue);
			Assert.Equal(expected.SearchableEnumValue, actual.SearchableEnumValue);

			if (checkPrimitiveProperties)
			{
				Assert.Equal(expected.DataVersion, actual.DataVersion);
				Assert.Equal(expected.SearchableCosmosPrimitive, actual.SearchableCosmosPrimitive);
			}

			// Compressed Top Level Fields
			Assert.Equal(expected.CompressedIntegerValue, actual.CompressedIntegerValue);
			Assert.Equal(expected.CompressedDecimalValue, actual.CompressedDecimalValue);
			Assert.Equal(expected.CompressedStringValue, actual.CompressedStringValue);
			Assert.Equal(expected.CompressedGuidValue, actual.CompressedGuidValue);
			Assert.Equal(expected.CompressedListValue.Count, actual.CompressedListValue.Count);

			for (var i = 0; i < expected.CompressedListValue.Count; i++)
			{
				Assert.Equal(expected.CompressedListValue[i], actual.CompressedListValue[i]);
			}

			Assert.Equal(expected.CompressedDateTimeValue, actual.CompressedDateTimeValue);
			Assert.Equal(expected.CompressedEnumValue, actual.CompressedEnumValue);

			// Complex Compressed Fields
			Assert.Equal(expected.CompressedPackedEntity.PackedIntegerValue, actual.CompressedPackedEntity.PackedIntegerValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedDecimalValue, actual.CompressedPackedEntity.PackedDecimalValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedStringValue, actual.CompressedPackedEntity.PackedStringValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedGuidValue, actual.CompressedPackedEntity.PackedGuidValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedListValue.Count, actual.CompressedPackedEntity.PackedListValue.Count);
			Assert.Equal(expected.CompressedListValue.Count, actual.CompressedListValue.Count);

			for (var i = 0; i < expected.CompressedListValue.Count; i++)
			{
				Assert.Equal(expected.CompressedListValue[i], actual.CompressedListValue[i]);
			}

			Assert.Equal(expected.CompressedPackedEntity.PackedDateTimeValue, actual.CompressedPackedEntity.PackedDateTimeValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedEnumValue, actual.CompressedPackedEntity.PackedEnumValue);
		}

		private static void AssertFakeEntityTwosMatch(FakeEntityTwo expected, FakeEntityTwo actual, bool checkPrimitiveProperties = false)
		{
			// Searchable Fields
			Assert.Equal(expected.FakeEntityTwoId, actual.FakeEntityTwoId);
			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.SearchableStringValue, actual.SearchableStringValue);

			if (checkPrimitiveProperties)
			{
				Assert.Equal(expected.DataVersion, actual.DataVersion);
			}

			// Compressed Top Level Fields
			Assert.Equal(expected.CompressedStringValue, actual.CompressedStringValue);
		}
		#endregion Helper Methods
	}
}