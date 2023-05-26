using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(IsKnownEmitTypeTests))]
    [TestCollectionPriorityOrder(204)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class IsKnownEmitTypeTests
    {
        public IsKnownEmitTypeTests()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(100)]
        public void IsKnownEmitType_True()
        {
            var optimizer = Optimizer.Get();

            var tableMetaData = optimizer.TableMappings[typeof(FakeEntity)];
            var storageType = tableMetaData.StorageType;

            Assert.True(optimizer.IsKnownEmitType(storageType));
        }

        [Fact, TestPriorityOrder(101)]
        public void IsKnownEmitType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownEmitType(typeof(InvalidSearchableKeyEntity)));
        }

        [Fact, TestPriorityOrder(102)]
        public void IsKnownEmitType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownEmitType(null); });
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