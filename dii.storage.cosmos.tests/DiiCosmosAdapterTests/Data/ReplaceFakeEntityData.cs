using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class ReplaceFakeEntityData : TheoryData<FakeEntity>
    {
        public ReplaceFakeEntityData()
        {
			var fakeEntity100 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"fakeEntity100: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"fakeEntity100: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"fakeEntity100: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"fakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"fakeEntity100: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"fakeEntity100: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"fakeEntity100: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
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

			Add(fakeEntity100);

			var fakeEntity101 = new FakeEntity
			{
				FakeEntityId = DateTime.Now.Ticks.ToString(),
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"fakeEntity101: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"fakeEntity101: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"fakeEntity101: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"fakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"fakeEntity101: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"fakeEntity101: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"fakeEntity101: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
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

			Add(fakeEntity101);
		}
    }
}