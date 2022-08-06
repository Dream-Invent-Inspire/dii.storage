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
            _ = Optimizer.Init(typeof(FakeEntityFour), typeof(FakeEntityThree));

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

        [Fact, TestPriorityOrder(102)]
        public void ToEntityObject_SuccessMultipleSameSubEntity()
        {
            var optimizer = Optimizer.Get();

            var fakeSearchableEntityFour = new FakeSearchableEntityFour
            {
                SearchableStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.SearchableStringValue)}",
                CompressedStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.CompressedStringValue)}",
                ComplexSearchable1 = new FakeSearchableEntityTwo
                {
                    SearchableStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.ComplexSearchable1)}.{nameof(FakeSearchableEntityFour.ComplexSearchable1.SearchableStringValue)}",
                    CompressedStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.ComplexSearchable1)}.{nameof(FakeSearchableEntityFour.ComplexSearchable1.CompressedStringValue)}"
                },
                ComplexSearchable2 = new FakeSearchableEntityTwo
                {
                    SearchableStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.ComplexSearchable2)}.{nameof(FakeSearchableEntityFour.ComplexSearchable2.SearchableStringValue)}",
                    CompressedStringValue = $"fakeSearchableEntityFour: {nameof(FakeSearchableEntityFour.ComplexSearchable2)}.{nameof(FakeSearchableEntityFour.ComplexSearchable2.CompressedStringValue)}"
                }
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntityFour>(fakeSearchableEntityFour);

            Assert.NotNull(entity);

            var searchableStringValue = entity.@string as string;
            var compressedStringValue = entity.p as string;

            var complex1SearchableStringValue = entity.complex1.@string as string;
            var complex1CompressedStringValue = entity.complex1.p as string;
            var complex2SearchableStringValue = entity.complex2.@string as string;
            var complex2CompressedStringValue = entity.complex2.p as string;

            Assert.Equal(fakeSearchableEntityFour.SearchableStringValue, searchableStringValue);
            Assert.Equal("kdkvZmFrZVNlYXJjaGFibGVFbnRpdHlGb3VyOiBDb21wcmVzc2VkU3RyaW5nVmFsdWU=", compressedStringValue);
            Assert.Equal(fakeSearchableEntityFour.ComplexSearchable1.SearchableStringValue, complex1SearchableStringValue);
            Assert.Equal("kdlCZmFrZVNlYXJjaGFibGVFbnRpdHlGb3VyOiBDb21wbGV4U2VhcmNoYWJsZTEuQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", complex1CompressedStringValue);
            Assert.Equal(fakeSearchableEntityFour.ComplexSearchable2.SearchableStringValue, complex2SearchableStringValue);
            Assert.Equal("kdlCZmFrZVNlYXJjaGFibGVFbnRpdHlGb3VyOiBDb21wbGV4U2VhcmNoYWJsZTIuQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", complex2CompressedStringValue);
        }

        [Fact, TestPriorityOrder(103)]
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
                    CompressedStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.RecursiveTypeReference)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.CompressedStringValue)}",
                    RecursiveTypeReference = new FakeSearchableEntityThree
                    {
                        SearchableStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.RecursiveTypeReference)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.SearchableStringValue)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.RecursiveTypeReference.SearchableStringValue)}",
                        CompressedStringValue = $"fakeSearchableEntityThree: {nameof(FakeSearchableEntityThree.RecursiveTypeReference)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.CompressedStringValue)}.{nameof(FakeSearchableEntityThree.RecursiveTypeReference.RecursiveTypeReference.CompressedStringValue)}"
                    }
                }
            };

            var entity = (dynamic)optimizer.ToEntityObject<FakeSearchableEntityThree>(fakeSearchableEntityThree);

            Assert.NotNull(entity);

            var searchableStringValue = entity.@string as string;
            var compressedStringValue = entity.p as string;

            // This may not work. Needs tested when bug is fixed.
            var complexSelfSearchableStringValue = entity.complexself.@string as string;
            var complexSelfCompressedStringValue = entity.complexself.p as string;

            var complexSelfComplexSelfSearchableStringValue = entity.complexself.complexself.@string as string;
            var complexSelfComplexSelfCompressedStringValue = entity.complexself.complexself.p as string;

            Assert.Equal(fakeSearchableEntityThree.SearchableStringValue, searchableStringValue);
            Assert.Equal("kdkwZmFrZVNlYXJjaGFibGVFbnRpdHlUaHJlZTogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", compressedStringValue);
            Assert.Equal(fakeSearchableEntityThree.RecursiveTypeReference.SearchableStringValue, complexSelfSearchableStringValue);
            Assert.Equal("kdlHZmFrZVNlYXJjaGFibGVFbnRpdHlUaHJlZTogUmVjdXJzaXZlVHlwZVJlZmVyZW5jZS5Db21wcmVzc2VkU3RyaW5nVmFsdWU=", complexSelfCompressedStringValue);
            Assert.Equal(fakeSearchableEntityThree.RecursiveTypeReference.RecursiveTypeReference.SearchableStringValue, complexSelfComplexSelfSearchableStringValue);
            Assert.Equal("kdldZmFrZVNlYXJjaGFibGVFbnRpdHlUaHJlZTogUmVjdXJzaXZlVHlwZVJlZmVyZW5jZS5Db21wcmVzc2VkU3RyaW5nVmFsdWUuQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", complexSelfComplexSelfCompressedStringValue);
        }

        [Fact, TestPriorityOrder(104)]
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

        [Fact, TestPriorityOrder(105)]
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