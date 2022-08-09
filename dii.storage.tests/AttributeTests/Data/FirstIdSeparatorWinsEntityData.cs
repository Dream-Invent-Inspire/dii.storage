using dii.storage.tests.Models;
using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class FirstIdSeparatorWinsEntityData : TheoryData<FirstIdSeparatorWinsEntity, string>
    {
        public FirstIdSeparatorWinsEntityData()
        {
			Add(new FirstIdSeparatorWinsEntity
            {
                Id1 = "pt1",
                Id2 = "pt2",
                Id3 = "pt3"
            }, "pt1*pt2*pt3");

            Add(new FirstIdSeparatorWinsEntity
            {
                Id1 = "pt1",
                Id3 = "pt3"
            }, "pt1*pt3");

            Add(new FirstIdSeparatorWinsEntity
            {
                Id1 = "pt1",
                Id2 = null,
                Id3 = "pt3"
            }, "pt1*pt3");

            Add(new FirstIdSeparatorWinsEntity
            {
                Id1 = "pt1",
                Id2 = string.Empty,
                Id3 = "pt3"
            }, "pt1*pt3");

            Add(new FirstIdSeparatorWinsEntity
            {
                Id1 = "pt1",
                Id2 = @"   ",
                Id3 = "pt3"
            }, "pt1*pt3");
        }
    }
}