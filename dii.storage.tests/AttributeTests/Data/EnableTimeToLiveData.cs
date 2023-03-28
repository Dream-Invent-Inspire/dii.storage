using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class EnableTimeToLiveData : TheoryData<int>
    {
        public EnableTimeToLiveData()
        {
            Add(-1);
            Add(3600);
            Add(int.MaxValue);
        }
    }
}