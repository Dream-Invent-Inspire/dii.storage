using dii.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.cosmos.tests.Models.Enums;

namespace dii.cosmos.tests.Data
{
    public class SingleFakeEntityData : TheoryData<FakeEntity>
    {
        public SingleFakeEntityData()
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
				CompressedEnumValue = FakeEnum.Second,
				ComplexSearchable = new FakeSearchableEntity
				{
					Soaps = "Dove",
					Tacos = "Bell"
				}
			};

			Add(fakeEntity);
        }
    }
}