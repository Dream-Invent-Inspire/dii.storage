using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class CompressExceptionData : TheoryData<int>
    {
        public CompressExceptionData()
        {
			Add(-1);
            Add(-34123);
            Add(int.MinValue);
        }
    }
}