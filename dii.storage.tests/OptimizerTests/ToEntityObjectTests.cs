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
        [Fact, TestPriorityOrder(100)]
        public void ToEntityObject_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void ToEntityObject_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeSearchableEntity = new FakeSearchableEntity
            {
                Tacos = "Bell",
                Soaps = "Dove"
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntity>(fakeSearchableEntity);

            Assert.NotNull(entity);

            var xtacos = entity.xtacos as string;
            var xsoaps = entity.xsoaps as string;

            Assert.Equal(fakeSearchableEntity.Tacos, xtacos);
            Assert.Equal(fakeSearchableEntity.Soaps, xsoaps);
        }

        [Fact(Skip = "Outstanding nested sub-entity bug test."), TestPriorityOrder(102)]
        public void ToEntityObject_SuccessNested()
        {
            var optimizer = Optimizer.Get();

            var fakeSearchableEntity = new FakeSearchableEntity
            {
                Tacos = "Bell",
                Soaps = "Dove",
                Nesting = new FakeSearchableEntity
                {
                    Tacos = "Grande",
                    Soaps = "Ivy"
                }
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntity>(fakeSearchableEntity);

            Assert.NotNull(entity);

            var xtacos = entity.xtacos as string;
            var xsoaps = entity.xsoaps as string;

            // This may not work. Needs tested when bug is fixed.
            var xnesting_xtacos = entity.xnesting.xtacos as string;
            var xnesting_xsoaps = entity.xnesting.xsoaps as string;

            Assert.Equal(fakeSearchableEntity.Tacos, xtacos);
            Assert.Equal(fakeSearchableEntity.Soaps, xsoaps);
            Assert.Equal(fakeSearchableEntity.Nesting.Tacos, xnesting_xtacos);
            Assert.Equal(fakeSearchableEntity.Nesting.Soaps, xnesting_xsoaps);
        }

        [Fact, TestPriorityOrder(103)]
        public void ToEntityObject_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.ToEntityObject<FakeSearchableEntity>(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(104)]
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