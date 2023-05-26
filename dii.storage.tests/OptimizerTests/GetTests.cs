using dii.storage.Exceptions;
using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(GetTests))]
    [TestCollectionPriorityOrder(201)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class GetTests
    {
        [Fact, TestPriorityOrder(100)]
        public void Get_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { Optimizer.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(Optimizer)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(101)]
        public void Get_Success()
        {
            _ = Optimizer.Init(typeof(FakeEntity));

            TestHelpers.AssertOptimizerIsInitialized();

            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
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