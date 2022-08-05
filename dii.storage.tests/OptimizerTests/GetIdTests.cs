using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(GetIdTests))]
    [TestCollectionPriorityOrder(211)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class GetIdTests
    {
        [Fact, TestPriorityOrder(100)]
        public void GetId_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void GetId_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntity = new FakeEntity
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityId = Guid.NewGuid().ToString()
            };

            var entity = optimizer.GetId(fakeEntity);

            Assert.Equal(fakeEntity.Id, entity);
        }

        [Fact, TestPriorityOrder(102)]
        public void GetId_TypeNotFound()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new InvalidSearchableKeyEntity
            {
                Id = Guid.NewGuid().ToString(),
                InvalidSearchableKeyEntityId = Guid.NewGuid().ToString()
            };

            var unpackedEntity = optimizer.GetId(unregisteredEntity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(103)]
        public void GetId_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.GetId<FakeEntity>(null); });

            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
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