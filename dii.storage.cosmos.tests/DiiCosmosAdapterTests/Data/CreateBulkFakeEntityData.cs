using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class CreateBulkFakeEntityData : TheoryData<List<FakeEntity>>
    {
        public CreateBulkFakeEntityData()
        {
            var fakeEntities200 = new List<FakeEntity>
            {
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 2,
                    SearchableDecimalValue = 2.02m,
                    SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity200: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity200: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
                    SearchableEnumValue = FakeEnum.Second,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 200,
                        PackedDecimalValue = 200.02m,
                        PackedStringValue = $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
                        PackedEnumValue = FakeEnum.Fourth
                    },
                    CompressedIntegerValue = 20,
                    CompressedDecimalValue = 20.02m,
                    CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity200: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity200: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
                    CompressedEnumValue = FakeEnum.Third,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                },
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 3,
                    SearchableDecimalValue = 3.03m,
                    SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity201: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity201: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
                    SearchableEnumValue = FakeEnum.Third,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 300,
                        PackedDecimalValue = 300.03m,
                        PackedStringValue = $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
                        PackedEnumValue = FakeEnum.Fifth
                    },
                    CompressedIntegerValue = 30,
                    CompressedDecimalValue = 30.03m,
                    CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity201: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity201: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
                    CompressedEnumValue = FakeEnum.Fourth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                },
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 4,
                    SearchableDecimalValue = 4.04m,
                    SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity202: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity202: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
                    SearchableEnumValue = FakeEnum.Fourth,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 400,
                        PackedDecimalValue = 400.04m,
                        PackedStringValue = $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
                        PackedEnumValue = FakeEnum.Sixth
                    },
                    CompressedIntegerValue = 40,
                    CompressedDecimalValue = 40.04m,
                    CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity202: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity202: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
                    CompressedEnumValue = FakeEnum.Fifth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                }
            };

            Add(fakeEntities200);
        }
    }
}