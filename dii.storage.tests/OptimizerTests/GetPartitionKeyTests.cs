using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(GetPartitionKeyTests))]
    [TestCollectionPriorityOrder(210)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class GetPartitionKeyTests
    {
        [Fact, TestPriorityOrder(100)]
        public void GetPartitionKey_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        #region GetPartitionKey As Type
        [Fact, TestPriorityOrder(101)]
        public void GetPartitionKey_AsType_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntity = new FakeEntity
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityId = Guid.NewGuid().ToString()
            };

            var entity = optimizer.GetPartitionKey<FakeEntity, Guid>(fakeEntity);

            Assert.Equal(new Guid(fakeEntity.FakeEntityId), entity);
        }

        [Fact, TestPriorityOrder(102)]
        public void GetPartitionKey_AsType_TypeNotFound()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                Id = Guid.NewGuid().ToString(),
                FakeInvalidEntityId = Guid.NewGuid().ToString()
            };

            var unpackedEntity = optimizer.GetPartitionKey<FakeInvalidEntity, Guid>(unregisteredEntity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(103)]
        public void GetPartitionKey_AsType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.GetPartitionKey<FakeEntity, Guid>(null); });

            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }
        #endregion GetPartitionKey As Type

        #region GetPartitionKey As String

        [Fact, TestPriorityOrder(200)]
        public void GetPartitionKey_AsString_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntity = new FakeEntity
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityId = Guid.NewGuid().ToString()
            };

            var entity = optimizer.GetPartitionKey(fakeEntity);

            Assert.Equal(fakeEntity.FakeEntityId, entity);
        }

        [Fact, TestPriorityOrder(201)]
        public void GetPartitionKey_AsString_TypeNotFound()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                Id = Guid.NewGuid().ToString(),
                FakeInvalidEntityId = Guid.NewGuid().ToString()
            };

            var unpackedEntity = optimizer.GetPartitionKey(unregisteredEntity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(202)]
        public void GetPartitionKey_AsString_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.GetPartitionKey<FakeEntity>(null); });

            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }
        #endregion GetPartitionKey As String

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public void Teardown()
        {
            TestHelpers.ResetOptimizerInstance();
        }
        #endregion
    }
}