using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(IsKnownConcreteTypeTests))]
    [TestCollectionPriorityOrder(203)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class IsKnownConcreteTypeTests
    {
        [Fact, TestPriorityOrder(100)]
        public void IsKnownConcreteType_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void IsKnownConcreteType_True()
        {
            var optimizer = Optimizer.Get();

            Assert.True(optimizer.IsKnownConcreteType(typeof(FakeEntity)));
        }

        [Fact, TestPriorityOrder(102)]
        public void IsKnownConcreteType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownConcreteType(typeof(FakeInvalidEntity)));
        }

        [Fact, TestPriorityOrder(103)]
        public void IsKnownConcreteType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownConcreteType(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
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