using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class FromEntityReturnDefaultData : TheoryData<object>
    {
        public FromEntityReturnDefaultData()
        {
            Add(new FakeInvalidEntity
            {
                FakeInvalidEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue)}"
            });
            Add(null);
            Add(new object());
        }
    }
}