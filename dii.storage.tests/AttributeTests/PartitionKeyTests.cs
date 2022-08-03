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
    public class PartitionKeyTests
    {
        [Fact, TestPriorityOrder(100)]
        public void PartitionKey_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityThree));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(FakeEntityThreePartitionKeyData))]
        public void PartitionKey_Success(FakeEntityThree fakeEntityThree, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(fakeEntityThree);

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