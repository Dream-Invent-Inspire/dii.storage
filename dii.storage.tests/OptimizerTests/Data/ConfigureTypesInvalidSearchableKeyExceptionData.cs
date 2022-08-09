using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesInvalidSearchableKeyExceptionData : TheoryData<Type, string, string, string>
    {
        public ConfigureTypesInvalidSearchableKeyExceptionData()
        {
            Add(typeof(InvalidSearchableKeyEntity), Constants.ReservedCompressedKey, nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue), nameof(InvalidSearchableKeyEntity));
            Add(typeof(InvalidSearchableKeyEntityTwo), Constants.ReservedPartitionKeyKey, nameof(InvalidSearchableKeyEntityTwo.InvalidSearchableKeyStringPKValue), nameof(InvalidSearchableKeyEntityTwo));
            Add(typeof(InvalidSearchableKeyEntityThree), Constants.ReservedIdKey, nameof(InvalidSearchableKeyEntityThree.InvalidSearchableKeyStringIdValue), nameof(InvalidSearchableKeyEntityThree));
        }
    }
}