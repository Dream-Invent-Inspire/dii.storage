using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(PackageToJsonTests))]
    [TestCollectionPriorityOrder(208)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class PackageToJsonTests
    {
        [Fact, TestPriorityOrder(100)]
        public void PackageToJson_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void PackageToJson_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityTwoId = Guid.NewGuid().ToString(),
                CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entity = optimizer.PackageToJson(fakeEntityTwo);

            Assert.NotNull(entity);
            Assert.Equal($"{{\"string\":null,\"long\":0,\"complex\":null,\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"{fakeEntityTwo.FakeEntityTwoId}\",\"id\":\"{fakeEntityTwo.Id}\"}}", entity);
        }

        [Fact, TestPriorityOrder(102)]
        public void PackageToJson_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.PackageToJson(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(103)]
        public void PackageToJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.PackageToJson<FakeEntityTwo>(null); });
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