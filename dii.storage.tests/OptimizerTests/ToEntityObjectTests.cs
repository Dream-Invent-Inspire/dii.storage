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
                SearchableStringValue = $"fakeSearchableEntity: {nameof(FakeSearchableEntity.SearchableStringValue)}",
                CompressedStringValue = $"fakeSearchableEntity: {nameof(FakeSearchableEntity.CompressedStringValue)}",
                ComplexSearchable = new FakeSearchableEntityTwo
                {
                    SearchableStringValue = $"fakeSearchableEntity: {nameof(FakeSearchableEntity.ComplexSearchable)}.{nameof(FakeSearchableEntity.ComplexSearchable.SearchableStringValue)}",
                    CompressedStringValue = $"fakeSearchableEntity: {nameof(FakeSearchableEntity.ComplexSearchable)}.{nameof(FakeSearchableEntity.ComplexSearchable.CompressedStringValue)}"
                }
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntity>(fakeSearchableEntity);

            var searchableStringValue = entity.@string as string;
            var compressedStringValue = entity.p as string;
            var complexSearchableStringValue = entity.complex.@string as string;
            var complexCompressedStringValue = entity.complex.p as string;

            Assert.Equal(fakeSearchableEntity.SearchableStringValue, searchableStringValue);
            Assert.Equal("kdkrZmFrZVNlYXJjaGFibGVFbnRpdHk6IENvbXByZXNzZWRTdHJpbmdWYWx1ZQ==", compressedStringValue);
            Assert.Equal(fakeSearchableEntity.ComplexSearchable.SearchableStringValue, complexSearchableStringValue);
            Assert.Equal("kdk9ZmFrZVNlYXJjaGFibGVFbnRpdHk6IENvbXBsZXhTZWFyY2hhYmxlLkNvbXByZXNzZWRTdHJpbmdWYWx1ZQ==", complexCompressedStringValue);
        }

        [Fact(Skip = "Outstanding recursive sub-entity bug test."), TestPriorityOrder(102)]
        public void ToEntityObject_SuccessRecursive()
        {
            var optimizer = Optimizer.Get();

            var fakeSearchableEntityThree = new FakeSearchableEntityThree
            {
                SearchableStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.SearchableStringValue)}",
                CompressedStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.CompressedStringValue)}",
                RecursiveTypeReference = new FakeSearchableEntityThree
                {
                    SearchableStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.RecursiveTypeReference)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.SearchableStringValue)}",
                    CompressedStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.RecursiveTypeReference)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.CompressedStringValue)}"
                }
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntityThree>(fakeSearchableEntityThree);

            Assert.NotNull(entity);

            var searchableStringValue = entity.@string as string;

            // This may not work. Needs tested when bug is fixed.
            var complexSearchableStringValue = entity.complex.@string as string;

            Assert.Equal(fakeSearchableEntityThree.SearchableStringValue, searchableStringValue);
            Assert.Equal(fakeSearchableEntityThree.RecursiveTypeReference.SearchableStringValue, complexSearchableStringValue);
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