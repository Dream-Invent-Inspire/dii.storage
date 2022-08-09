using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class ReplaceFakeEntityDataWithResponseFlag : TheoryData<FakeEntity, bool, int>
    {
        public ReplaceFakeEntityDataWithResponseFlag()
        {
			var replacementFakeEntity100 = new FakeEntity
			{
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"replacementFakeEntity100: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity100: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"replacementFakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity100: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"replacementFakeEntity100: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity100: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
				ComplexSearchable = new FakeSearchableEntity
				{
					SearchableStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
					CompressedStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
					ComplexSearchable = new FakeSearchableEntityTwo
					{
						SearchableStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity100: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
					}
				}
			};

			Add(replacementFakeEntity100, true, 0);

			var replacementFakeEntity101 = new FakeEntity
			{
				SearchableIntegerValue = 4,
				SearchableDecimalValue = 4.04m,
				SearchableStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.SearchableStringValue)}",
				SearchableGuidValue = Guid.NewGuid(),
				SearchableListValue = new List<string>
					{
						$"replacementFakeEntity101: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity101: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
				SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
				SearchableEnumValue = FakeEnum.Fourth,
				CompressedPackedEntity = new FakeMessagePackEntity
				{
					PackedIntegerValue = 400,
					PackedDecimalValue = 400.04m,
					PackedStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
					PackedGuidValue = Guid.NewGuid(),
					PackedListValue = new List<string>
						{
							$"replacementFakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity101: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
					PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
					PackedEnumValue = FakeEnum.Sixth
				},
				CompressedIntegerValue = 40,
				CompressedDecimalValue = 40.04m,
				CompressedStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.CompressedStringValue)}",
				CompressedGuidValue = Guid.NewGuid(),
				CompressedListValue = new List<string>
					{
						$"replacementFakeEntity101: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity101: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
				CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
				CompressedEnumValue = FakeEnum.Fifth,
				ComplexSearchable = new FakeSearchableEntity
				{
					SearchableStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
					CompressedStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
					ComplexSearchable = new FakeSearchableEntityTwo
					{
						SearchableStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity101: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
					}
				}
			};

			Add(replacementFakeEntity101, false, 1);
		}
    }
}