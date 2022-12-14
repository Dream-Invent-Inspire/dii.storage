using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
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