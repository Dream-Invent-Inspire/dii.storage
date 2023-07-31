using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class SearchableData : TheoryData<string>
    {
        public SearchableData()
        {
			Add("abc");
            Add("_property");
        }
    }
}