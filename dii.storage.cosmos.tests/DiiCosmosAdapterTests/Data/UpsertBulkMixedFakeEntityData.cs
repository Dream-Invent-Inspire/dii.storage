using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class UpsertBulkMixedFakeEntityData : TheoryData<List<FakeEntity>>
    {
        public UpsertBulkMixedFakeEntityData()
        {
            var fakeEntities400 = new List<FakeEntity>
            {
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 4,
                    SearchableDecimalValue = 4.04m,
                    SearchableStringValue = $"fakeEntity400: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity400: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity400: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
                    SearchableEnumValue = FakeEnum.Third,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 400,
                        PackedDecimalValue = 400.04m,
                        PackedStringValue = $"fakeEntity400: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity400: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity400: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
                        PackedEnumValue = FakeEnum.Fifth
                    },
                    CompressedIntegerValue = 40,
                    CompressedDecimalValue = 40.04m,
                    CompressedStringValue = $"fakeEntity400: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity400: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity400: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
                    CompressedEnumValue = FakeEnum.Fourth
                },
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 5,
                    SearchableDecimalValue = 5.05m,
                    SearchableStringValue = $"fakeEntity500: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity500: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity500: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(5),
                    SearchableEnumValue = FakeEnum.Fourth,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 500,
                        PackedDecimalValue = 500.05m,
                        PackedStringValue = $"fakeEntity500: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity500: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity500: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(500),
                        PackedEnumValue = FakeEnum.Sixth
                    },
                    CompressedIntegerValue = 50,
                    CompressedDecimalValue = 50.05m,
                    CompressedStringValue = $"fakeEntity500: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity500: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity500: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(50),
                    CompressedEnumValue = FakeEnum.Fifth
                },
                new FakeEntity
                {
                    SearchableIntegerValue = 3,
                    SearchableDecimalValue = 3.03m,
                    SearchableStringValue = $"replacementFakeEntity1000: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"replacementFakeEntity1000: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"replacementFakeEntity1000: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
                    SearchableEnumValue = FakeEnum.Third,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 300,
                        PackedDecimalValue = 300.03m,
                        PackedStringValue = $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
                        PackedEnumValue = FakeEnum.Fifth
                    },
                    CompressedIntegerValue = 30,
                    CompressedDecimalValue = 30.03m,
                    CompressedStringValue = $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"replacementFakeEntity1000: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
                    CompressedEnumValue = FakeEnum.Fourth
                },
                new FakeEntity
                {
                    SearchableIntegerValue = 4,
                    SearchableDecimalValue = 4.04m,
                    SearchableStringValue = $"replacementFakeEntity3000: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"replacementFakeEntity3000: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"replacementFakeEntity3000: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
                    SearchableEnumValue = FakeEnum.Fourth,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 400,
                        PackedDecimalValue = 400.04m,
                        PackedStringValue = $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
                        PackedEnumValue = FakeEnum.Sixth
                    },
                    CompressedIntegerValue = 40,
                    CompressedDecimalValue = 40.04m,
                    CompressedStringValue = $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"replacementFakeEntity3000: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
                    CompressedEnumValue = FakeEnum.Fifth
                }
            };

            Add(fakeEntities400);
        }
    }
}