using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesInvalidPartitionKeyOrderExceptionData : TheoryData<string, Type, string, string, int>
    {
        public ConfigureTypesInvalidPartitionKeyOrderExceptionData()
        {
            Add("DB1", typeof(OrderInvalidPartitionKeyEntity), nameof(OrderInvalidPartitionKeyEntity.PK1), nameof(OrderInvalidPartitionKeyEntity.PK2), 0);
        }
    }
}