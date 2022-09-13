using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(ToEntityTests))]
    [TestCollectionPriorityOrder(205)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ToEntityTests
    {
        [Fact, TestPriorityOrder(100)]
        public void ToEntity_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo), typeof(FakeEntityFive));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void ToEntity_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityTwoId = Guid.NewGuid().ToString(),
                CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entity = (dynamic)optimizer.ToEntity(fakeEntityTwo);

            Assert.NotNull(entity);
            Assert.Equal(fakeEntityTwo.FakeEntityTwoId, entity.PK);
            Assert.Equal(fakeEntityTwo.Id, entity.id);
            Assert.Equal("kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", entity.p);
        }

        [Fact, TestPriorityOrder(102)]
        public void ToEntity_SuccessWithSameIdAndPKProperty()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityFive = new FakeEntityFive
            {
                FakeEntityFiveId = Guid.NewGuid().ToString(),
                SearchableStringValue = $"fakeEntityFive: {nameof(FakeEntityFive.SearchableStringValue)}",
                CompressedStringValue = $"fakeEntityFive: {nameof(FakeEntityFive.CompressedStringValue)}"
            };

            var entity = (dynamic)optimizer.ToEntity(fakeEntityFive);

            Assert.NotNull(entity);
            Assert.Equal(fakeEntityFive.FakeEntityFiveId, entity.id);
            Assert.Equal(fakeEntityFive.FakeEntityFiveId, entity.PK);
            Assert.Equal("fakeEntityFive: SearchableStringValue", entity.@string);
            Assert.Equal("kdklZmFrZUVudGl0eUZpdmU6IENvbXByZXNzZWRTdHJpbmdWYWx1ZQ==", entity.p);
        }

        [Fact, TestPriorityOrder(103)]
        public void ToEntity_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new InvalidSearchableKeyEntity
            {
                InvalidSearchableKeyEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.ToEntity(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(104)]
        public void ToEntity_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.ToEntity<FakeEntityTwo>(null); });

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