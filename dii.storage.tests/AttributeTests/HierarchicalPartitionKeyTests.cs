using dii.storage.tests.Attributes;
using dii.storage.tests.AttributeTests.Data;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(PartitionKeyTests))]
    [TestCollectionPriorityOrder(300)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class HierarchicalPartitionKeyTests
    {
        public HierarchicalPartitionKeyTests()
        {
            _ = Optimizer.Init("FakeDb", typeof(MultipleHierarchicalPartitionKeyEntity), typeof(FirstPartitionKeySeparatorWinsEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Theory, TestPriorityOrder(100), ClassData(typeof(MultipleHPKEntityData))]
        public void PartitionKey_Success(MultipleHierarchicalPartitionKeyEntity multiplePartitionKeyEntity, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(multiplePartitionKeyEntity);

            Assert.NotNull(entity);

        }


        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public void Teardown()
        {
            TestHelpers.ResetOptimizerInstance();
        }
        #endregion
    }
}