using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests.Data
{
    public class ContextEmptyDatabaseIdData : TheoryData<string>
    {
        public ContextEmptyDatabaseIdData()
        {
			Add(null);
            Add(string.Empty);
            Add(@"   ");
        }
    }
}