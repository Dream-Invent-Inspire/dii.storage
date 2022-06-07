using dii.cosmos.Exceptions;
using dii.cosmos.tests.Attributes;
using dii.cosmos.tests.Models;
using dii.cosmos.tests.Orderer;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace dii.cosmos.tests
{
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class OptimizerTests
    {
        [Fact, TestPriorityOrder(1)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void Init_NotInitialized()
        {
            Optimizer optimizer = null;

            Assert.Null(optimizer);

            var exception = Assert.Throws<DiiNotInitializedException>(() => { Optimizer.Get(); });

            Assert.NotNull(exception);
            Assert.Equal("Optimizer not initialized.", exception.Message);
        }

        [Fact, TestPriorityOrder(2)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void Init_Success()
        {
            var optimizer = Optimizer.Init(typeof(FakeEntity));

            Assert.NotNull(optimizer);
        }

        [Fact, TestPriorityOrder(3)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void Init_Get()
        {
            var optimizer = Optimizer.Get();

            Assert.NotNull(optimizer);
        }

        [Fact, TestPriorityOrder(4)]
        [Trait("Cosmos", "Optimizer Tests")]
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

        [Fact, TestPriorityOrder(5)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void ConfigureTypes_AddInvalidType()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            var exception = Assert.Throws<DiiReservedSearchableKeyException>(() => { optimizer.ConfigureTypes(typeof(FakeInvalidEntity)); });

            Assert.NotNull(exception);
            Assert.Equal("'p' is a reserved [Searchable(key)] key and cannot be used for property 'InvalidSearchableKeyStringValue' in object 'FakeInvalidEntity'.", exception.Message);

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(6)]
        [Trait("Cosmos", "Optimizer Tests")]
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

        [Fact, TestPriorityOrder(7)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void ConfigureTypes_EmptyTypeArray()
        {
            var optimizer = Optimizer.Get();

            Assert.Single(optimizer.Tables);

            var tablesInitialized = optimizer.Tables;
            var tableMappingsInitialized = optimizer.TableMappings;

            optimizer.ConfigureTypes(new Type[] { });

            Assert.Single(optimizer.Tables);
            Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
            Assert.Equal(tablesInitialized[0].TableName, optimizer.Tables[0].TableName);

            Assert.Single(optimizer.TableMappings);
            Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);
            Assert.Equal(tableMappingsInitialized[typeof(FakeEntity)].TableName, optimizer.TableMappings[typeof(FakeEntity)].TableName);
        }

        [Fact, TestPriorityOrder(8)]
        [Trait("Cosmos", "Optimizer Tests")]
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

        [Fact, TestPriorityOrder(9)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownConcreteType_True()
        {
            var optimizer = Optimizer.Get();

            Assert.True(optimizer.IsKnownConcreteType(typeof(FakeEntity)));
        }

        [Fact, TestPriorityOrder(10)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownConcreteType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownConcreteType(typeof(FakeInvalidEntity)));
        }

        [Fact, TestPriorityOrder(11)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownConcreteType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownConcreteType(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
        }

        [Fact, TestPriorityOrder(12)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownEmitType_True()
        {
            var optimizer = Optimizer.Get();

            var tableMetaData = optimizer.TableMappings[typeof(FakeEntity)];
            var storageType = tableMetaData.StorageType;

            Assert.True(optimizer.IsKnownEmitType(storageType));
        }

        [Fact, TestPriorityOrder(13)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownEmitType_False()
        {
            var optimizer = Optimizer.Get();

            Assert.False(optimizer.IsKnownEmitType(typeof(FakeInvalidEntity)));
        }

        [Fact, TestPriorityOrder(14)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void IsKnownEmitType_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.IsKnownEmitType(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'key')", exception.Message);
        }

        [Fact, TestPriorityOrder(15)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void ToEntity_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                FakeEntityTwoId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                CompressedStringValue = "fakeEntityTwo: CompressedStringValue"
            };

            var entity = (dynamic)optimizer.ToEntity(fakeEntityTwo);

            Assert.NotNull(entity);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", entity.PK);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", entity.id);
            Assert.Equal("kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl", entity.p);
        }

        [Fact, TestPriorityOrder(16)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void ToEntity_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                InvalidSearchableKeyStringValue = "fakeInvalidEntity: InvalidSearchableKeyStringValue"
            };

            var entity = optimizer.ToEntity(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(17)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void ToEntity_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.ToEntity<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(18)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                FakeEntityTwoId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                CompressedStringValue = "fakeEntityTwo: CompressedStringValue"
            };

            var entity = optimizer.ToEntity(fakeEntityTwo);

            Assert.NotNull(entity);

            var unpackedEntity = optimizer.FromEntity<FakeEntityTwo>(entity);

            Assert.NotNull(unpackedEntity);
            Assert.Equal(fakeEntityTwo.FakeEntityTwoId, unpackedEntity.FakeEntityTwoId);
            Assert.Equal(fakeEntityTwo.Id, unpackedEntity.Id);
            Assert.Equal(fakeEntityTwo.CompressedStringValue, unpackedEntity.CompressedStringValue);
        }

        [Fact, TestPriorityOrder(19)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                InvalidSearchableKeyStringValue = "fakeInvalidEntity: InvalidSearchableKeyStringValue"
            };

            var unpackedEntity = optimizer.FromEntity<FakeInvalidEntity>(unregisteredEntity);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(20)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_NotJObjectOrEntity()
        {
            var optimizer = Optimizer.Get();

            var randomObject = new object();

            var unpackedEntity = optimizer.FromEntity<FakeInvalidEntity>(randomObject);

            Assert.Equal(default, unpackedEntity);
        }

        [Fact, TestPriorityOrder(21)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.FromEntity<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(22)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_CosmosJsonSuccess()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = @"{
  ""id"": ""13E8310C-26EF-4E77-AE8C-9130316210EA"",
  ""_etag"": ""\""00000000-0000-0000-79bf-5e755a3201d8\"""",
  ""p"": ""kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl"",
  ""PK"": ""13E8310C-26EF-4E77-AE8C-9130316210EA"",
  ""_rid"": ""aKAHANyVWdQJAAAAAAAAAA=="",
  ""_self"": ""dbs/aKAHAA==/colls/aKAHANyVWdQ=/docs/aKAHANyVWdQJAAAAAAAAAA==/"",
  ""_attachments"": ""attachments/"",
  ""_ts"": 1654531583
}";

            var fakeEntityTwo = JObject.Parse(fakeEntityTwoJson);

            var unpackedEntity = optimizer.FromEntity<FakeEntityTwo>(fakeEntityTwo);

            Assert.NotNull(unpackedEntity);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", unpackedEntity.FakeEntityTwoId);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", unpackedEntity.Id);
            Assert.Equal("fakeEntityTwo: CompressedStringValue", unpackedEntity.CompressedStringValue);
            Assert.Equal("\"00000000-0000-0000-79bf-5e755a3201d8\"", unpackedEntity.Version);
        }

        [Fact, TestPriorityOrder(23)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void FromEntity_CosmosJsonEmpty()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = "{}";

            var fakeEntityTwo = JObject.Parse(fakeEntityTwoJson);

            var exception = Assert.Throws<ArgumentException>(() => { optimizer.FromEntity<FakeEntityTwo>(fakeEntityTwo); });
            Assert.NotNull(exception);
            Assert.Equal("Packed object contained no properties. (Parameter 'packedObject')", exception.Message);
        }

        [Fact, TestPriorityOrder(24)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void PackageToJson_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwo = new FakeEntityTwo
            {
                FakeEntityTwoId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                CompressedStringValue = "fakeEntityTwo: CompressedStringValue"
            };

            var entity = optimizer.PackageToJson(fakeEntityTwo);

            Assert.NotNull(entity);
            Assert.Equal("{\"id\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\",\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\"}", entity);
        }

        [Fact, TestPriorityOrder(25)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void PackageToJson_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new FakeInvalidEntity
            {
                FakeInvalidEntityId = "13E8310C-26EF-4E77-AE8C-9130316210EA",
                InvalidSearchableKeyStringValue = "fakeInvalidEntity: InvalidSearchableKeyStringValue"
            };

            var entity = optimizer.PackageToJson(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(26)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void PackageToJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.PackageToJson<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        [Fact, TestPriorityOrder(27)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_Success()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = "{\"id\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\",\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\"}";

            var entity = optimizer.UnpackageFromJson<FakeEntityTwo>(fakeEntityTwoJson);

            Assert.NotNull(entity);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", entity.FakeEntityTwoId);
            Assert.Equal("13E8310C-26EF-4E77-AE8C-9130316210EA", entity.Id);
            Assert.Equal("fakeEntityTwo: CompressedStringValue", entity.CompressedStringValue);
        }

        [Fact, TestPriorityOrder(28)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var fakeEntityTwoJson = "{\"id\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\",\"_etag\":null,\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\",\"PK\":\"13E8310C-26EF-4E77-AE8C-9130316210EA\"}";

            var entity = optimizer.UnpackageFromJson<FakeInvalidEntity>(fakeEntityTwoJson);

            Assert.Equal(default, entity);
        }

        [Fact, TestPriorityOrder(29)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(30)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_Empty()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>(string.Empty); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(31)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_Whitespace()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>("  "); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'json')", exception.Message);
        }

        [Fact, TestPriorityOrder(32)]
        [Trait("Cosmos", "Optimizer Tests")]
        public void UnpackageFromJson_EmptyJson()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentException>(() => { optimizer.UnpackageFromJson<FakeEntityTwo>("{}"); });
            Assert.NotNull(exception);
            Assert.Equal("Packed object contained no properties. (Parameter 'packedObject')", exception.Message);
        }
    }
}