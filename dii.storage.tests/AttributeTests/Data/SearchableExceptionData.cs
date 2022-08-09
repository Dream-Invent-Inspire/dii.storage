using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class SearchableExceptionData : TheoryData<string>
    {
        public SearchableExceptionData()
        {
			Add(null);
			Add(string.Empty);
            Add(@"   ");
        }
    }
}