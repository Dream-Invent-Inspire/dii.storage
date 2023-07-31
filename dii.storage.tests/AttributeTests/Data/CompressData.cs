using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class CompressData : TheoryData<int>
    {
        public CompressData()
        {
			Add(0);
            Add(34123);
            Add(int.MaxValue);
        }
    }
}