using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class EnableTimeToLiveExceptionData : TheoryData<int>
    {
        public EnableTimeToLiveExceptionData()
        {
			Add(0);
            Add(-3600);
            Add(int.MinValue);
        }
    }
}