using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(GetPartitionKeyTypeTests))]
    [TestCollectionPriorityOrder(212)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class GetPartitionKeyTypeTests
    {
        public GetPartitionKeyTypeTests()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(100)]
        public void GetPartitionKeyType_Success()
        {
            var optimizer = Optimizer.Get();

            var type = optimizer.GetPartitionKeyType<FakeEntity>();

            Assert.Equal(typeof(Guid), type);
        }

        [Fact, TestPriorityOrder(101)]
        public void GetPartitionKeyType_NotFound()
        {
            var optimizer = Optimizer.Get();

            var type = optimizer.GetPartitionKeyType<InvalidSearchableKeyEntity>();

            Assert.Null(type);
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