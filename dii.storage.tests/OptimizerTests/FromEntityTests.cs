using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.OptimizerTests.Data;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(FromEntityTests))]
    [TestCollectionPriorityOrder(207)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class FromEntityTests
    {
        [Fact, TestPriorityOrder(100)]
        public void FromEntity_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void FromEntity_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityTwoId = Guid.NewGuid().ToString(),
                CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entity = optimizer.ToEntity(fakeEntityTwo);

            Assert.NotNull(entity);

            var unpackedEntity = optimizer.FromEntity<FakeEntityTwo>(entity);

            Assert.NotNull(unpackedEntity);
            Assert.Equal(fakeEntityTwo.FakeEntityTwoId, unpackedEntity.FakeEntityTwoId);
            Assert.Equal(fakeEntityTwo.Id, unpackedEntity.Id);
            Assert.Equal(fakeEntityTwo.CompressedStringValue, unpackedEntity.CompressedStringValue);
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(FromEntityReturnDefaultData))]
        public void FromEntity_ReturnDefault(object entity)
        {
            var optimizer = Optimizer.Get();

            var unpackedEntity = optimizer.FromEntity<FakeInvalidEntity>(entity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(103)]
        public void FromEntity_JsonSuccess()
        {
            var optimizer = Optimizer.Get();

            var id = Guid.NewGuid().ToString();
            var fakeEntityTwoJson = $@"{{
  ""id"": ""{id}"",
  ""_etag"": ""\""00000000-0000-0000-79bf-5e755a3201d8\"""",
  ""p"": ""kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl"",
  ""PK"": ""{id}"",
  ""_rid"": ""aKAHANyVWdQJAAAAAAAAAA=="",
  ""_self"": ""dbs/aKAHAA==/colls/aKAHANyVWdQ=/docs/aKAHANyVWdQJAAAAAAAAAA==/"",
  ""_attachments"": ""attachments/"",
  ""_ts"": 1654531583
}}";

            var fakeEntityTwo = JObject.Parse(fakeEntityTwoJson);

            var unpackedEntity = optimizer.FromEntity<FakeEntityTwo>(fakeEntityTwo);

            Assert.NotNull(unpackedEntity);
            Assert.Equal(id, unpackedEntity.FakeEntityTwoId);
            Assert.Equal(id, unpackedEntity.Id);
            Assert.Equal("fakeEntityTwo: CompressedStringValue", unpackedEntity.CompressedStringValue);
            Assert.Equal("\"00000000-0000-0000-79bf-5e755a3201d8\"", unpackedEntity.DataVersion);
        }

        [Fact, TestPriorityOrder(104)]
        public void FromEntity_JsonEmpty()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = "{}";

            var fakeEntityTwo = JObject.Parse(fakeEntityTwoJson);

            var exception = Assert.Throws<ArgumentException>(() => { optimizer.FromEntity<FakeEntityTwo>(fakeEntityTwo); });
            Assert.NotNull(exception);
            Assert.Equal("Packed object contained no properties. (Parameter 'packedObject')", exception.Message);
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