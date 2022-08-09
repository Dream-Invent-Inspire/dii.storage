using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesInvalidPartitionKeyOrderExceptionData : TheoryData<Type, string, string, int>
    {
        public ConfigureTypesInvalidPartitionKeyOrderExceptionData()
        {
            Add(typeof(OrderInvalidPartitionKeyEntity), nameof(OrderInvalidPartitionKeyEntity.PK1), nameof(OrderInvalidPartitionKeyEntity.PK2), 0);
        }
    }
}