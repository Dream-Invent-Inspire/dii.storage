using dii.storage.tests.Models;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests.Data
{
    public class FromEntityReturnDefaultData : TheoryData<object>
    {
        public FromEntityReturnDefaultData()
        {
            Add(new InvalidSearchableKeyEntity
            {
                InvalidSearchableKeyEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue)}"
            });
            Add(null);
            Add(new object());
        }
    }
}