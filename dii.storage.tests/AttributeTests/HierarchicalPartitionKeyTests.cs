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
            _ = Optimizer.Init(typeof(MultipleHierarchicalPartitionKeyEntity), typeof(FirstPartitionKeySeparatorWinsEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Theory, TestPriorityOrder(100), ClassData(typeof(MultiplePKEntityData))]
        public void PartitionKey_Success(MultiplePartitionKeyEntity multiplePartitionKeyEntity, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(multiplePartitionKeyEntity);

            Assert.NotNull(entity);

            var pk = entity.PK as string;

            Assert.Equal(expected, pk);
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(FirstPartitionKeySeparatorWinsEntityData))]
        public void PartitionKey_FirstSeparatorWins(FirstPartitionKeySeparatorWinsEntity firstPartitionKeySeparatorWinsEntity, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(firstPartitionKeySeparatorWinsEntity);

            Assert.NotNull(entity);

            var pk = entity.PK as string;

            Assert.Equal(expected, pk);
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