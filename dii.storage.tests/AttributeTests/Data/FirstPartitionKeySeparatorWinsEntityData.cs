using dii.storage.tests.Models;
using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class FirstPartitionKeySeparatorWinsEntityData : TheoryData<FirstPartitionKeySeparatorWinsEntity, string>
    {
        public FirstPartitionKeySeparatorWinsEntityData()
        {
			Add(new FirstPartitionKeySeparatorWinsEntity
            {
                PK1 = "pt1",
                PK2 = "pt2",
                PK3 = "pt3"
            }, "pt1#pt2#pt3");

            Add(new FirstPartitionKeySeparatorWinsEntity
            {
                PK1 = "pt1",
                PK3 = "pt3"
            }, "pt1#pt3");

            Add(new FirstPartitionKeySeparatorWinsEntity
            {
                PK1 = "pt1",
                PK2 = null,
                PK3 = "pt3"
            }, "pt1#pt3");

            Add(new FirstPartitionKeySeparatorWinsEntity
            {
                PK1 = "pt1",
                PK2 = string.Empty,
                PK3 = "pt3"
            }, "pt1#pt3");

            Add(new FirstPartitionKeySeparatorWinsEntity
            {
                PK1 = "pt1",
                PK2 = @"   ",
                PK3 = "pt3"
            }, "pt1#pt3");
        }
    }
}