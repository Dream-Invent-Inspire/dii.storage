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
            _ = Optimizer.Init(true, typeof(FakeInvalidEntity));

            TestHelpers.AssertOptimizerIsInitialized();

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(104)]
        public void Init_PassedTypesThrowOnInvalidDiiEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(false, typeof(FakeInvalidEntity)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(105)]
        public void Init_AutoDetectTypesThrowOnInvalidDiiEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(true); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(106)]
        public void Init_AutoDetectTypesIgnoreInvalidDiiEntities()
        {
            _ = Optimizer.Init(true, true);

            TestHelpers.AssertOptimizerIsInitialized();
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