using dii.storage.Exceptions;
using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(InitTests))]
    [TestCollectionPriorityOrder(200)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class InitTests
    {
        [Fact, TestPriorityOrder(100)]
        public void Init_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { Optimizer.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(Optimizer)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(101)]
        public void Init_Empty()
        {
            _ = Optimizer.Init();

            TestHelpers.AssertOptimizerIsInitialized();

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(102)]
        public void Init_PassedTypes()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(103)]
        public void Init_PassedTypesIgnoreInvalidDiiEntities()
        {
            _ = Optimizer.Init(true, typeof(InvalidSearchableKeyEntity));

            TestHelpers.AssertOptimizerIsInitialized();

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(104)]
        public void Init_PassedTypesThrowOnInvalidDiiEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(false, typeof(InvalidSearchableKeyEntity)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue), nameof(InvalidSearchableKeyEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(105)]
        public void Init_AutoDetectTypesThrowOnInvalidDiiEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(true); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue), nameof(InvalidSearchableKeyEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(106)]
        public void Init_AutoDetectTypesIgnoreInvalidDiiEntities()
        {
            _ = Optimizer.Init(true, true);

            TestHelpers.AssertOptimizerIsInitialized();

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(107)]
        public void Init_InvalidNestingException()
        {
            var exception = Assert.Throws<DiiInvalidNestingException>(() => { Optimizer.Init(typeof(InvalidSelfReferenceEntity)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiInvalidNestingException(nameof(InvalidSelfReferenceEntity)).Message, exception.Message);
        }

        #region Teardown
        //[Fact, TestPriorityOrder(int.MaxValue)]
        //public void Teardown()
        //{
        //}
        #endregion
    }
}