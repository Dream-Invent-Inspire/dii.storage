using dii.storage.tests.Models;
using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class MultiplePKEntityData : TheoryData<MultiplePartitionKeyEntity, string>
    {
        public MultiplePKEntityData()
        {
			Add(new MultiplePartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = "pt2",
                PK3 = "pt3"
            }, "pt1_pt2_pt3");

            Add(new MultiplePartitionKeyEntity
            {
                PK1 = "pt1",
                PK3 = "pt3"
            }, "pt1_pt3");

            Add(new MultiplePartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = null,
                PK3 = "pt3"
            }, "pt1_pt3");

            Add(new MultiplePartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = string.Empty,
                PK3 = "pt3"
            }, "pt1_pt3");

            Add(new MultiplePartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = @"   ",
                PK3 = "pt3"
            }, "pt1_pt3");
        }
    }
}