using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class CreateFakeEntityDataWithResponseFlag : TheoryData<FakeEntity, bool>
    {
        public CreateFakeEntityDataWithResponseFlag()
        {
			var fakeEntity100 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 1,
				SearchableDecimalValue = 1.01m,
				SearchableStringValue = $"fakeEntity100: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
				{
					$"fakeEntity100: {nameof(FakeEntity.SearchableListValue)}[0]",
					$"fakeEntity100: {nameof(FakeEntity.SearchableListValue)}[1]"
				},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(1),
				SearchableEnumValue = FakeEnum.First,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 100,
					PackedDecimalValue = 100.01m,
					PackedStringValue = $"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
					{
						$"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
						$"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
					},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(100),
					PackedEnumValue = FakeEnum.Third
				},
				CompressedIntegerValue = 10,
				CompressedDecimalValue = 10.01m,
				CompressedStringValue = $"fakeEntity100: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
				{
					$"fakeEntity100: {nameof(FakeEntity.CompressedListValue)}[0]",
					$"fakeEntity100: {nameof(FakeEntity.CompressedListValue)}[1]"
				},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(10),
				CompressedEnumValue = FakeEnum.Second,
				ComplexSearchable = new FakeSearchableEntity
                {
                    SearchableStringValue = $"fakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                    CompressedStringValue = $"fakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                    ComplexSearchable = new FakeSearchableEntityTwo
                    {
                        SearchableStringValue = $"fakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                    }
                }
			};

			Add(fakeEntity100, true);

            var fakeEntity101 = new FakeEntity
            {
                FakeEntityId = DateTime.Now.Ticks.ToString(),
                SearchableIntegerValue = 2,
                SearchableDecimalValue = 2.02m,
                SearchableStringValue = $"fakeEntity101: {nameof(FakeEntity.SearchableStringValue)}",
                SearchableGuidValue = Guid.NewGuid(),
                SearchableListValue = new List<string>
                    {
                        $"fakeEntity101: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity101: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
                SearchableEnumValue = FakeEnum.Second,
                CompressedPackedEntity = new FakeMessagePackEntity
                {
                    PackedIntegerValue = 200,
                    PackedDecimalValue = 200.02m,
                    PackedStringValue = $"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                    PackedGuidValue = Guid.NewGuid(),
                    PackedListValue = new List<string>
                        {
                            $"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                    PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
                    PackedEnumValue = FakeEnum.Fourth
                },
                CompressedIntegerValue = 20,
                CompressedDecimalValue = 20.02m,
                CompressedStringValue = $"fakeEntity101: {nameof(FakeEntity.CompressedStringValue)}",
                CompressedGuidValue = Guid.NewGuid(),
                CompressedListValue = new List<string>
                    {
                        $"fakeEntity101: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity101: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
                CompressedEnumValue = FakeEnum.Third,
                ComplexSearchable = new FakeSearchableEntity
                {
                    SearchableStringValue = $"fakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                    CompressedStringValue = $"fakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                    ComplexSearchable = new FakeSearchableEntityTwo
                    {
                        SearchableStringValue = $"fakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                    }
                }
            };

            Add(fakeEntity101, false);
        }
    }
}