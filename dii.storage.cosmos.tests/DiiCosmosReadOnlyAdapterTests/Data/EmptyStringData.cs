using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosReadOnlyAdapterTests.Data
{
    public class EmptyStringData : TheoryData<string>
    {
        public EmptyStringData()
        {
			Add(null);
            Add(string.Empty);
            Add(@"   ");
        }
    }
}