using dii.storage.tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.tests.AttributeTests.Data
{
    public class MultipleHPKEntityData : TheoryData<MultipleHierarchicalPartitionKeyEntity, string>
    {
        public MultipleHPKEntityData()
        {
            Add(new MultipleHierarchicalPartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = "pt2",
                PK3 = "pt3"
            }, "pt1_pt2_pt3");

            Add(new MultipleHierarchicalPartitionKeyEntity
            {
                PK1 = "pt1",
                PK3 = "pt3"
            }, "pt1_pt3");

            Add(new MultipleHierarchicalPartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = null,
                PK3 = "pt3"
            }, "pt1_pt3");

            Add(new MultipleHierarchicalPartitionKeyEntity
            {
                PK1 = "pt1",
                PK2 = string.Empty,
                PK3 = "pt3"
            }, "pt1_pt3");

        }
    }
}
