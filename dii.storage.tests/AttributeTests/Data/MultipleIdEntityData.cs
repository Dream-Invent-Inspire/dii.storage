using dii.storage.tests.Models;
using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class MultipleIdEntityData : TheoryData<MultipleIdEntity, string>
    {
        public MultipleIdEntityData()
        {
			Add(new MultipleIdEntity
            {
                Id1 = "pt1",
                Id2 = "pt2",
                Id3 = "pt3"
            }, "pt1#pt2#pt3");

            Add(new MultipleIdEntity
            {
                Id1 = "pt1",
                Id3 = "pt3"
            }, "pt1#pt3");

            Add(new MultipleIdEntity
            {
                Id1 = "pt1",
                Id2 = null,
                Id3 = "pt3"
            }, "pt1#pt3");

            Add(new MultipleIdEntity
            {
                Id1 = "pt1",
                Id2 = string.Empty,
                Id3 = "pt3"
            }, "pt1#pt3");

            Add(new MultipleIdEntity
            {
                Id1 = "pt1",
                Id2 = @"   ",
                Id3 = "pt3"
            }, "pt1#pt3");
        }
    }
}