using dii.storage.cosmos.tests.Models;
using System;
using System.Collections.Generic;
using Xunit;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class MultipleFakeEntityData : TheoryData<List<FakeEntity>>
    {
        public MultipleFakeEntityData()
        {
            var fakeEntities = new List<FakeEntity>
            {
                new FakeEntity
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
                    CompressedEnumValue = FakeEnum.Third,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        Soaps = "Dove1",
                        Tacos = "Bell1"
                    }
                },
                new FakeEntity
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
                    CompressedEnumValue = FakeEnum.Fourth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        Soaps = "Dove2",
                        Tacos = "Bell2"
                    }
                },
                new FakeEntity
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
                    CompressedEnumValue = FakeEnum.Fifth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        Soaps = "Dove3",
                        Tacos = "Bell3"
                    }
                }
            };

            Add(fakeEntities);
        }
    }
}