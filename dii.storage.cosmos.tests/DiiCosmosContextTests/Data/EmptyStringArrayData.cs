using System;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests.Data
{
    public class EmptyStringArrayData : TheoryData<string[]>
    {
        public EmptyStringArrayData()
        {
			Add(null);
            Add(Array.Empty<string>());
            Add(new string[0]);
        }
    }
}