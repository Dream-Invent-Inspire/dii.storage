using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesInvalidIdOrderExceptionData : TheoryData<Type, string, string, int>
    {
        public ConfigureTypesInvalidIdOrderExceptionData()
        {
            Add(typeof(OrderInvalidIdEntity), nameof(OrderInvalidIdEntity.Id1), nameof(OrderInvalidIdEntity.Id2), 0);
        }
    }
}