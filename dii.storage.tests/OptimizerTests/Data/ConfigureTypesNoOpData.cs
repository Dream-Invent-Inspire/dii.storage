using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class ConfigureTypesNoOpData : TheoryData<Type[]>
    {
        public ConfigureTypesNoOpData()
        {
            Add(new[] { typeof(FakeEntity) });
            Add(null);
            Add(Array.Empty<Type>());
        }
    }
}