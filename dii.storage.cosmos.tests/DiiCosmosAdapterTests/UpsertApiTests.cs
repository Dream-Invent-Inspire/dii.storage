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
		[Theory, TestPriorityOrder(100), ClassData(typeof(SingleFakeEntityData))]
		public async Task UpsertAsync_Prep(FakeEntity fakeEntity)
		{
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

			var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
		}

		[Fact, TestPriorityOrder(101)]
		public async Task UpsertAsync_CreateSuccess()
		{
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

			var replacedFakeEntity = await _adapterFixture.FakeEntityAdapter.UpsertAsync(fakeEntity2).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, replacedFakeEntity);

			_adapterFixture.CreatedFakeEntities.Add(replacedFakeEntity);
		}

		[Fact, TestPriorityOrder(102)]
		public async Task UpsertAsync_ReplaceSuccess()
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
                CompressedEnumValue = FakeEnum.Fifth
            };

            var replacedFakeEntity = await _adapterFixture.FakeEntityAdapter.UpsertAsync(replacementFakeEntity).ConfigureAwait(false);

			TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity, replacedFakeEntity);

			_adapterFixture.CreatedFakeEntities[0] = replacedFakeEntity;
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
        [Theory, TestPriorityOrder(200), ClassData(typeof(SingleFakeEntityData))]
        public async Task UpsertBulkAsync_Prep(FakeEntity fakeEntity)
        {
            // Ensure context exists and is initialized.
            TestHelpers.AssertContextAndOptimizerAreInitialized();

            var savedFakeEntity = await _adapterFixture.FakeEntityAdapter.CreateAsync(fakeEntity).ConfigureAwait(false);

            TestHelpers.AssertFakeEntitiesMatch(fakeEntity, savedFakeEntity);

            _adapterFixture.CreatedFakeEntities.Add(savedFakeEntity);
        }

        [Fact, TestPriorityOrder(201)]
        public async Task UpsertBulkAsync_CreateSuccess()
        {
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
                    PackedStringValue = $"fakeEntity3: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
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

            var entitiesToCreate = new List<FakeEntity>
            {
                fakeEntity2,
                fakeEntity3
            };

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToCreate).ConfigureAwait(false);

            TestHelpers.AssertFakeEntitiesMatch(fakeEntity2, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity2.Id));
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity3, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity3.Id));

            _adapterFixture.CreatedFakeEntities.AddRange(savedFakeEntities);
        }

        [Fact, TestPriorityOrder(202)]
        public async Task UpsertBulkAsync_ReplaceSuccess()
        {
            var fakeEntity2 = _adapterFixture.CreatedFakeEntities[1];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

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
                CompressedEnumValue = FakeEnum.Fourth
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
                CompressedEnumValue = FakeEnum.Fifth
            };

            var entitiesToReplace = new List<FakeEntity>
            {
                replacementFakeEntity2,
                replacementFakeEntity3
            };

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToReplace).ConfigureAwait(false);

            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity2, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity2.Id));
            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity3, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id));

            _adapterFixture.CreatedFakeEntities[1] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity2.Id);
            _adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id);
        }

        [Fact, TestPriorityOrder(203)]
        public async Task UpsertBulkAsync_CreateAndReplaceSuccess()
        {
            var fakeEntity1 = _adapterFixture.CreatedFakeEntities[0];
            var fakeEntity3 = _adapterFixture.CreatedFakeEntities[2];

            var fakeEntity4 = new FakeEntity
            {
                FakeEntityId = DateTime.Now.Ticks.ToString(),
                SearchableIntegerValue = 4,
                SearchableDecimalValue = 4.04m,
                SearchableStringValue = $"fakeEntity4: {nameof(FakeEntity.SearchableStringValue)}",
                SearchableGuidValue = Guid.NewGuid(),
                SearchableListValue = new List<string>
                {
                    $"fakeEntity4: {nameof(FakeEntity.SearchableListValue)}[0]",
                    $"fakeEntity4: {nameof(FakeEntity.SearchableListValue)}[1]"
                },
                SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
                SearchableEnumValue = FakeEnum.Third,
                CompressedPackedEntity = new FakeMessagePackEntity
                {
                    PackedIntegerValue = 400,
                    PackedDecimalValue = 400.04m,
                    PackedStringValue = $"fakeEntity4: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                    PackedGuidValue = Guid.NewGuid(),
                    PackedListValue = new List<string>
                    {
                        $"fakeEntity4: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                        $"fakeEntity4: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                    },
                    PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
                    PackedEnumValue = FakeEnum.Fifth
                },
                CompressedIntegerValue = 40,
                CompressedDecimalValue = 40.04m,
                CompressedStringValue = $"fakeEntity4: {nameof(FakeEntity.CompressedStringValue)}",
                CompressedGuidValue = Guid.NewGuid(),
                CompressedListValue = new List<string>
                {
                    $"fakeEntity4: {nameof(FakeEntity.CompressedListValue)}[0]",
                    $"fakeEntity4: {nameof(FakeEntity.CompressedListValue)}[1]"
                },
                CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
                CompressedEnumValue = FakeEnum.Fourth
            };

            var fakeEntity5 = new FakeEntity
            {
                FakeEntityId = DateTime.Now.Ticks.ToString(),
                SearchableIntegerValue = 5,
                SearchableDecimalValue = 5.05m,
                SearchableStringValue = $"fakeEntity5: {nameof(FakeEntity.SearchableStringValue)}",
                SearchableGuidValue = Guid.NewGuid(),
                SearchableListValue = new List<string>
                {
                    $"fakeEntity5: {nameof(FakeEntity.SearchableListValue)}[0]",
                    $"fakeEntity5: {nameof(FakeEntity.SearchableListValue)}[1]"
                },
                SearchableDateTimeValue = DateTime.UtcNow.AddDays(5),
                SearchableEnumValue = FakeEnum.Fourth,
                CompressedPackedEntity = new FakeMessagePackEntity
                {
                    PackedIntegerValue = 500,
                    PackedDecimalValue = 500.05m,
                    PackedStringValue = $"fakeEntity5: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                    PackedGuidValue = Guid.NewGuid(),
                    PackedListValue = new List<string>
                    {
                        $"fakeEntity5: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                        $"fakeEntity5: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                    },
                    PackedDateTimeValue = DateTime.UtcNow.AddDays(500),
                    PackedEnumValue = FakeEnum.Sixth
                },
                CompressedIntegerValue = 50,
                CompressedDecimalValue = 50.05m,
                CompressedStringValue = $"fakeEntity5: {nameof(FakeEntity.CompressedStringValue)}",
                CompressedGuidValue = Guid.NewGuid(),
                CompressedListValue = new List<string>
                {
                    $"fakeEntity5: {nameof(FakeEntity.CompressedListValue)}[0]",
                    $"fakeEntity5: {nameof(FakeEntity.CompressedListValue)}[1]"
                },
                CompressedDateTimeValue = DateTime.UtcNow.AddDays(50),
                CompressedEnumValue = FakeEnum.Fifth
            };

            var replacementFakeEntity1 = new FakeEntity
            {
                Id = fakeEntity1.Id,
                FakeEntityId = fakeEntity1.FakeEntityId,
                SearchableIntegerValue = 3,
                SearchableDecimalValue = 3.03m,
                SearchableStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.SearchableStringValue)}",
                SearchableGuidValue = Guid.NewGuid(),
                SearchableListValue = new List<string>
                {
                    $"replacementFakeEntity1: {nameof(FakeEntity.SearchableListValue)}[0]",
                    $"replacementFakeEntity1: {nameof(FakeEntity.SearchableListValue)}[1]"
                },
                SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
                SearchableEnumValue = FakeEnum.Third,
                CompressedPackedEntity = new FakeMessagePackEntity
                {
                    PackedIntegerValue = 300,
                    PackedDecimalValue = 300.03m,
                    PackedStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                    PackedGuidValue = Guid.NewGuid(),
                    PackedListValue = new List<string>
                    {
                        $"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                        $"replacementFakeEntity1: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                    },
                    PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
                    PackedEnumValue = FakeEnum.Fifth
                },
                CompressedIntegerValue = 30,
                CompressedDecimalValue = 30.03m,
                CompressedStringValue = $"replacementFakeEntity1: {nameof(FakeEntity.CompressedStringValue)}",
                CompressedGuidValue = Guid.NewGuid(),
                CompressedListValue = new List<string>
                {
                    $"replacementFakeEntity1: {nameof(FakeEntity.CompressedListValue)}[0]",
                    $"replacementFakeEntity1: {nameof(FakeEntity.CompressedListValue)}[1]"
                },
                CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
                CompressedEnumValue = FakeEnum.Fourth
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
                CompressedEnumValue = FakeEnum.Fifth
            };

            var entitiesToCreateOrReplace = new List<FakeEntity>
            {
                replacementFakeEntity1,
                replacementFakeEntity3,
                fakeEntity4,
                fakeEntity5
            };

            var savedFakeEntities = await _adapterFixture.FakeEntityAdapter.UpsertBulkAsync(entitiesToCreateOrReplace).ConfigureAwait(false);

            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity1, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1.Id));
            TestHelpers.AssertFakeEntitiesMatch(replacementFakeEntity3, savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id));
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity4, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity4.Id));
            TestHelpers.AssertFakeEntitiesMatch(fakeEntity5, savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity5.Id));

            _adapterFixture.CreatedFakeEntities[0] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity1.Id);
            _adapterFixture.CreatedFakeEntities[2] = savedFakeEntities.FirstOrDefault(x => x.Id == replacementFakeEntity3.Id);
            _adapterFixture.CreatedFakeEntities.Add(savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity4.Id));
            _adapterFixture.CreatedFakeEntities.Add(savedFakeEntities.FirstOrDefault(x => x.Id == fakeEntity5.Id));
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