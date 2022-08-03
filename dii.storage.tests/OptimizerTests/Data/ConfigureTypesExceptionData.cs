using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesExceptionData : TheoryData<Type, string, string, string>
    {
        public ConfigureTypesExceptionData()
        {
            Add(typeof(FakeInvalidEntity), Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity));
            Add(typeof(FakeInvalidEntityTwo), Constants.ReservedPartitionKeyKey, nameof(FakeInvalidEntityTwo.InvalidSearchableKeyStringPKValue), nameof(FakeInvalidEntityTwo));
            Add(typeof(FakeInvalidEntityThree), Constants.ReservedIdKey, nameof(FakeInvalidEntityThree.InvalidSearchableKeyStringIdValue), nameof(FakeInvalidEntityThree));
        }
    }
}