using dii.cosmos.Exceptions;
using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using dii.cosmos.tests.Utilities;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace dii.cosmos.tests
{
    [Collection(nameof(OptimizerTests))]
    [TestCollectionPriorityOrder(2)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class OptimizerTests
    {
        [Fact, TestPriorityOrder(1)]
        public void Init_NotInitialized()
        {
            var exception = Assert.Throws<DiiNotInitializedException>(() => { Optimizer.Get(); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiNotInitializedException(nameof(Optimizer)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(2)]
        public void Init_Empty()
        {
            var optimizer = Optimizer.Init();

            Assert.NotNull(optimizer);
            Assert.Empty(optimizer.Tables);
            Assert.Empty(optimizer.TableMappings);

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(3)]
        public void Init_PassedTypes()
        {
            var optimizer = Optimizer.Init(typeof(FakeEntity));

            Assert.NotNull(optimizer);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(4)]
        public void Init_PassedTypesIgnoreInvalidDiiCosmosEntities()
        {
            var optimizer = Optimizer.Init(true, typeof(FakeInvalidEntity));

            Assert.NotNull(optimizer);

            Assert.NotNull(optimizer);
            Assert.Empty(optimizer.Tables);
            Assert.Empty(optimizer.TableMappings);

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(5)]
        public void Init_PassedTypesThrowOnInvalidDiiCosmosEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(false, typeof(FakeInvalidEntity)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(6)]
        public void Init_AutoDetectTypesThrowOnInvalidDiiCosmosEntities()
        {
            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { Optimizer.Init(true); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity)).Message, exception.Message);
        }

        [Fact, TestPriorityOrder(7)]
        public void Init_AutoDetectTypesIgnoreInvalidDiiCosmosEntities()
        {
            var optimizer = Optimizer.Init(true, true);

            Assert.NotNull(optimizer);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);
            Assert.Equal(tablesInitialized[1].TableName, optimizer.Tables[1].TableName);

            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntityTwo)].TableName, optimizer.TableMappings[typeof(FakeEntityTwo)].TableName);

            TestHelpers.ResetOptimizerInstance();
        }

        [Fact, TestPriorityOrder(8)]
        public void StageData()
        {
            var optimizer = Optimizer.Init(typeof(FakeEntity));

            Assert.NotNull(optimizer);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(9)]
        public void Init_Get()
        {
            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
        }

        [Fact, TestPriorityOrder(10)]
        public void ConfigureTypes_AddSameType()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            optimizer.ConfigureTypes(typeof(FakeEntity));

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(11)]
        public void ConfigureTypes_AddInvalidTypePKey()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { optimizer.ConfigureTypes(typeof(FakeInvalidEntity)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue), nameof(FakeInvalidEntity)).Message, exception.Message);

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(12)]
        public void ConfigureTypes_AddInvalidTypePKKey()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { optimizer.ConfigureTypes(typeof(FakeInvalidEntityTwo)); });

            Assert.NotNull(exception);
            Assert.Equal(new DiiReservedSearchableKeyException(Constants.ReservedPartitionKeyKey, nameof(FakeInvalidEntityTwo.InvalidSearchableKeyStringPKValue), nameof(FakeInvalidEntityTwo)).Message, exception.Message);

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(13)]
        public void ConfigureTypes_Null()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            optimizer.ConfigureTypes(null);

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(14)]
        public void ConfigureTypes_EmptyTypeArray()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            optimizer.ConfigureTypes(Array.Empty<Type>());

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(15)]
        public void ConfigureTypes_AddNewType()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            optimizer.ConfigureTypes(typeof(FakeEntityTwo));

            Assert.Equal(2, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);
            Assert.Equal(nameof(FakeEntityTwo), optimizer.Tables[1].TableName);


            Assert.Equal(2, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
            Assert.Equal(nameof(FakeEntityTwo), optimizer.TableMappings[typeof(FakeEntityTwo)].TableName);
        }

        [Fact, TestPriorityOrder(16)]
        public void IsKnownConcreteType_True()
        {
            var optimizer = Optimizer.Get();

            Assert.True(optimizer.IsKnownConcreteType(typeof(FakeEntity)));
        }

        [Fact, TestPriorityOrder(17)]
        public void IsKnownConcreteType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownConcreteType(typeof(FakeInvalidEntity)));
        }

        [Fact, TestPriorityOrder(18)]
        public void IsKnownConcreteType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownConcreteType(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
        }

        [Fact, TestPriorityOrder(19)]
        public void IsKnownEmitType_True()
        {
            var optimizer = Optimizer.Get();

            var tableMetaData = optimizer.TableMappings[typeof(FakeEntity)];
            var storageType = tableMetaData.StorageType;

            Assert.True(optimizer.IsKnownEmitType(storageType));
        }

        [Fact, TestPriorityOrder(20)]
        public void IsKnownEmitType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownEmitType(typeof(FakeInvalidEntity)));
        }

        [Fact, TestPriorityOrder(21)]
        public void IsKnownEmitType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownEmitType(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
        }

        [Fact, TestPriorityOrder(22)]
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

        [Fact, TestPriorityOrder(23)]
        public void ToEntity_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.ToEntity(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(24)]
        public void ToEntity_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.ToEntity<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(25)]
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

        [Fact, TestPriorityOrder(26)]
        public void FromEntity_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(FakeInvalidEntity.InvalidSearchableKeyStringPValue)}"
            };

            var unpackedEntity = optimizer.FromEntity<FakeInvalidEntity>(unregisteredEntity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(27)]
        public void FromEntity_NotJObjectOrEntity()
        {
            var optimizer = Optimizer.Get();

            var randomObject = new object();

            var unpackedEntity = optimizer.FromEntity<FakeInvalidEntity>(randomObject);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(28)]
        public void FromEntity_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.FromEntity<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(29)]
        public void FromEntity_CosmosJsonSuccess()
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

        [Fact, TestPriorityOrder(30)]
        public void FromEntity_CosmosJsonEmpty()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = "{}";

            var fakeEntityTwo = JObject.Parse(fakeEntityTwoJson);

            var exception = Assert.Throws<ArgumentException>(() => { optimizer.FromEntity<FakeEntityTwo>(fakeEntityTwo); });
            Assert.NotNull(exception);
            Assert.Equal("Packed object contained no properties. (Parameter 'packedObject')", exception.Message);
        }

        [Fact, TestPriorityOrder(31)]
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
            Assert.Equal($"{{\"string\":null,\"long\":0,\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"{fakeEntityTwo.FakeEntityTwoId}\",\"id\":\"{fakeEntityTwo.Id}\"}}", entity);
        }

        [Fact, TestPriorityOrder(32)]
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

        [Fact, TestPriorityOrder(33)]
        public void PackageToJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.PackageToJson<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(34)]
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

        [Fact, TestPriorityOrder(35)]
        public void UnpackageFromJson_UnregisteredType()
        {
            var optimizer = Optimizer.Get();
            var id = Guid.NewGuid().ToString();

            var fakeEntityTwoJson = $"{{\"id\":\"{id}\",\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"{id}\"}}";

            var entity = optimizer.UnpackageFromJson<FakeInvalidEntity>(fakeEntityTwoJson);

            Assert.Equal(default, entity);
        }

        [Fact, TestPriorityOrder(36)]
        public void UnpackageFromJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(37)]
        public void UnpackageFromJson_Empty()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>(string.Empty); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(38)]
        public void UnpackageFromJson_Whitespace()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>("  "); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(39)]
        public void UnpackageFromJson_EmptyJson()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>("{}"); });
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