using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(ToEntityObjectTests))]
    [TestCollectionPriorityOrder(206)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ToEntityObjectTests
    {
        public ToEntityObjectTests()
        {
            _ = Optimizer.Init(typeof(FakeEntityThree));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(100)]
        public void ToEntityObject_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new InvalidSearchableKeyEntity
            {
                InvalidSearchableKeyEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.ToEntityObject<FakeSearchableEntity>(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(101)]
        public void ToEntityObject_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.ToEntityObject<FakeSearchableEntity>(null); });

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