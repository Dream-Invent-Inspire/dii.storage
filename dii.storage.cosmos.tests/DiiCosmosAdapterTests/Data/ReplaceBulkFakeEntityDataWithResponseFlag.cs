using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class ReplaceBulkFakeEntityDataWithResponseFlag : TheoryData<List<(FakeEntity, int)>, bool>
    {
        public ReplaceBulkFakeEntityDataWithResponseFlag()
        {
			var replacementFakeEntities200 = new List<(FakeEntity, int)>
			{
				(new FakeEntity
				{
					SearchableIntegerValue = 2,
					SearchableDecimalValue = 2.02m,
					SearchableStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
						{
							$"replacementFakeEntity200: {nameof(FakeEntity.SearchableListValue)}[0]",
							$"replacementFakeEntity200: {nameof(FakeEntity.SearchableListValue)}[1]"
						},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
					SearchableEnumValue = FakeEnum.Second,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 200,
						PackedDecimalValue = 200.02m,
						PackedStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
							{
								$"replacementFakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
								$"replacementFakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
							},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
						PackedEnumValue = FakeEnum.Fourth
					},
					CompressedIntegerValue = 20,
					CompressedDecimalValue = 20.02m,
					CompressedStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
						{
							$"replacementFakeEntity200: {nameof(FakeEntity.CompressedListValue)}[0]",
							$"replacementFakeEntity200: {nameof(FakeEntity.CompressedListValue)}[1]"
						},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
					CompressedEnumValue = FakeEnum.Third,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 0),
				(new FakeEntity
				{
					SearchableIntegerValue = 3,
					SearchableDecimalValue = 3.03m,
					SearchableStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
					{
						$"replacementFakeEntity201: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity201: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
					SearchableEnumValue = FakeEnum.Third,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 300,
						PackedDecimalValue = 300.03m,
						PackedStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
						{
							$"replacementFakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
						PackedEnumValue = FakeEnum.Fifth
					},
					CompressedIntegerValue = 30,
					CompressedDecimalValue = 30.03m,
					CompressedStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
					{
						$"replacementFakeEntity201: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity201: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
					CompressedEnumValue = FakeEnum.Fourth,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 1),
				(new FakeEntity
				{
					SearchableIntegerValue = 4,
					SearchableDecimalValue = 4.04m,
					SearchableStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
					{
						$"replacementFakeEntity202: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity202: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
					SearchableEnumValue = FakeEnum.Fourth,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 400,
						PackedDecimalValue = 400.04m,
						PackedStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
						{
							$"replacementFakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
						PackedEnumValue = FakeEnum.Sixth
					},
					CompressedIntegerValue = 40,
					CompressedDecimalValue = 40.04m,
					CompressedStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
					{
						$"replacementFakeEntity202: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity202: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
					CompressedEnumValue = FakeEnum.Fifth,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 2)
			};

			Add(replacementFakeEntities200, true);

			var replacementFakeEntities300 = new List<(FakeEntity, int)>
			{
				(new FakeEntity
				{
					SearchableIntegerValue = 2,
					SearchableDecimalValue = 2.02m,
					SearchableStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
						{
							$"replacementFakeEntity300: {nameof(FakeEntity.SearchableListValue)}[0]",
							$"replacementFakeEntity300: {nameof(FakeEntity.SearchableListValue)}[1]"
						},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
					SearchableEnumValue = FakeEnum.Second,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 200,
						PackedDecimalValue = 200.02m,
						PackedStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
							{
								$"replacementFakeEntity300: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
								$"replacementFakeEntity300: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
							},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
						PackedEnumValue = FakeEnum.Fourth
					},
					CompressedIntegerValue = 20,
					CompressedDecimalValue = 20.02m,
					CompressedStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
						{
							$"replacementFakeEntity300: {nameof(FakeEntity.CompressedListValue)}[0]",
							$"replacementFakeEntity300: {nameof(FakeEntity.CompressedListValue)}[1]"
						},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
					CompressedEnumValue = FakeEnum.Third,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity300: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 3),
				(new FakeEntity
				{
					SearchableIntegerValue = 3,
					SearchableDecimalValue = 3.03m,
					SearchableStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
					{
						$"replacementFakeEntity301: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity301: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
					SearchableEnumValue = FakeEnum.Third,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 300,
						PackedDecimalValue = 300.03m,
						PackedStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
						{
							$"replacementFakeEntity301: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity301: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
						PackedEnumValue = FakeEnum.Fifth
					},
					CompressedIntegerValue = 30,
					CompressedDecimalValue = 30.03m,
					CompressedStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
					{
						$"replacementFakeEntity301: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity301: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
					CompressedEnumValue = FakeEnum.Fourth,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity301: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 4),
				(new FakeEntity
				{
					SearchableIntegerValue = 4,
					SearchableDecimalValue = 4.04m,
					SearchableStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.SearchableStringValue)}",
					SearchableGuidValue = Guid.NewGuid(),
					SearchableListValue = new List<string>
					{
						$"replacementFakeEntity302: {nameof(FakeEntity.SearchableListValue)}[0]",
						$"replacementFakeEntity302: {nameof(FakeEntity.SearchableListValue)}[1]"
					},
					SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
					SearchableEnumValue = FakeEnum.Fourth,
					CompressedPackedEntity = new FakeMessagePackEntity
					{
						PackedIntegerValue = 400,
						PackedDecimalValue = 400.04m,
						PackedStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
						PackedGuidValue = Guid.NewGuid(),
						PackedListValue = new List<string>
						{
							$"replacementFakeEntity302: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
							$"replacementFakeEntity302: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
						},
						PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
						PackedEnumValue = FakeEnum.Sixth
					},
					CompressedIntegerValue = 40,
					CompressedDecimalValue = 40.04m,
					CompressedStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.CompressedStringValue)}",
					CompressedGuidValue = Guid.NewGuid(),
					CompressedListValue = new List<string>
					{
						$"replacementFakeEntity302: {nameof(FakeEntity.CompressedListValue)}[0]",
						$"replacementFakeEntity302: {nameof(FakeEntity.CompressedListValue)}[1]"
					},
					CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
					CompressedEnumValue = FakeEnum.Fifth,
					ComplexSearchable = new FakeSearchableEntity
					{
						SearchableStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
						CompressedStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
						ComplexSearchable = new FakeSearchableEntityTwo
						{
							SearchableStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
							CompressedStringValue = $"replacementFakeEntity302: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
						}
					}
				}, 5)
			};

			Add(replacementFakeEntities300, false);
		}
    }
}