using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.OptimizerTests.Data;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(UnpackageFromJsonTests))]
    [TestCollectionPriorityOrder(209)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class UnpackageFromJsonTests
    {
        [Fact, TestPriorityOrder(100)]
        public void UnpackageFromJson_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void UnpackageFromJson_Success()
        {
            var optimizer = Optimizer.Get();
            var id = Guid.NewGuid().ToString();

            var fakeEntityTwoJson = $"{{\"id\":\"{id}\",\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"{id}\"}}";

            var entity = optimizer.UnpackageFromJson<FakeEntityTwo>(fakeEntityTwoJson);

            Assert.NotNull(entity);
            Assert.Equal(id, entity.FakeEntityTwoId);
            Assert.Equal(id, entity.Id);
            Assert.Equal("fakeEntityTwo: CompressedStringValue", entity.CompressedStringValue);
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(UnpackageFromJsonReturnDefaultData))]
        public void UnpackageFromJson_UnregisteredType(string json)
        {
            var optimizer = Optimizer.Get();

            var entity = optimizer.UnpackageFromJson<InvalidSearchableKeyEntity>(json);

            Assert.Equal(default, entity);
        }

        [Theory, TestPriorityOrder(103), ClassData(typeof(UnpackageFromJsonExceptionData))]
        public void UnpackageFromJson_Exception(string json, Type exceptionType, string exceptionMessage)
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws(exceptionType, () => { optimizer.UnpackageFromJson<FakeEntityTwo>(json); });
            Assert.NotNull(exception);
            Assert.Equal(exceptionMessage, exception.Message);
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