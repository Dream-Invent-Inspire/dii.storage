using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosAdapterTests;
using dii.storage.cosmos.tests.Orderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using dii.storage.Utilities;
using dii.storage.Models;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{

    [Collection(nameof(FetchApiTests))]
    [TestCollectionPriorityOrder(400)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class LookupTests
    {
        [Fact, TestPriorityOrder(101)]
        public void DynamicTypeTest()
        {
            Dictionary<int, PropertyInfo> lookupHpks = new Dictionary<int, PropertyInfo>{
                { 0, typeof(TestEntity).GetProperty("ClientId") },
                { 1, typeof(TestEntity).GetProperty("PersonId") }
            };

            Dictionary<int, PropertyInfo> lookupIds = new Dictionary<int, PropertyInfo>{
                { 0, typeof(TestEntity).GetProperty("SessionId") },
            };

            List<PropertyInfo> searchableFields = new List<PropertyInfo>
            {
                typeof(TestEntity).GetProperty("SearchableProperty")
            };

            TableMetaData lookupTableMetaData = new TableMetaData
            {
                ClassName = "TestType",
                TableName = "TestTable",
                HierarchicalPartitionKeys = new Dictionary<int, PropertyInfo>{
                    { 0, typeof(TestEntity).GetProperty("ClientId") },
                    { 1, typeof(TestEntity).GetProperty("PersonId") }
                },
                IdProperties = new Dictionary<int, PropertyInfo>{
                    { 0, typeof(TestEntity).GetProperty("ClientId") },
                    { 1, typeof(TestEntity).GetProperty("PersonId") }
                },
                LookupHpks = lookupHpks,
                LookupIds = lookupIds,
                SearchableFields = searchableFields,
                StorageType = typeof(TestEntity),
                ConcreteType = typeof(TestEntity),
                DbId = "TestDbId",
            };
            var type = DynamicTypeCreator.CreateLookupType(lookupHpks, lookupIds, searchableFields, lookupTableMetaData);
            Assert.NotNull(type);
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
        {
            try
            {
                Optimizer.Clear();
                DiiCosmosContext.Reset();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }

    public class TestEntity
    {
        public string ClientId { get; set; }
        public string PersonId { get; set; }
        public string SessionId { get; set; }

        public string SampleProperty { get; set; }

        public string SearchableProperty { get; set; }

        public string OtherAccessIdProperty { get; set; }
    }
}
