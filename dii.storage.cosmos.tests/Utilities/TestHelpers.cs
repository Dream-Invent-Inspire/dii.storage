using dii.storage.Models.Interfaces;
using dii.storage.cosmos.tests.Fixtures;
using dii.storage.cosmos.tests.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.Utilities
{
    public static class TestHelpers
    {
		#region Private Fields
		private static readonly BindingFlags _privateBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
		private static readonly BindingFlags _publicBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;
        #endregion Private Fields

        #region Public Methods

        #region Init
		public static async Task<Optimizer> InitContextAndOptimizerAsync(INoSqlDatabaseConfig noSqlDatabaseConfig, Optimizer optimizer, Type[] types)
        {
			var context = DiiCosmosContext.Init(noSqlDatabaseConfig);

			var dbExistsTask = await context.DoesDatabaseExistAsync().ConfigureAwait(false);

			if (!dbExistsTask)
			{
				throw new ApplicationException("AdapterFixture test database does not exist and failed to be created.");
			}

			if (optimizer == null)
			{
				optimizer = Optimizer.Init(types);
			}

			context.InitTables(optimizer.Tables.ToArray()).Wait();

			return optimizer;
		}
        #endregion Init

        #region Teardown
        public static async Task DeleteAllFakeEntitiesAsync(AdapterFixture adapterFixture)
		{
			for (var i = 0; i < adapterFixture.CreatedFakeEntities.Count; i++)
			{
				var success = await adapterFixture.FakeEntityAdapter.DeleteEntityAsync(adapterFixture.CreatedFakeEntities[i]).ConfigureAwait(false);
				var shouldBeNull = await adapterFixture.FakeEntityAdapter.GetAsync(adapterFixture.CreatedFakeEntities[i].Id, adapterFixture.CreatedFakeEntities[i].FakeEntityId).ConfigureAwait(false);

				Assert.True(success);
				Assert.Null(shouldBeNull);
			}

			adapterFixture.CreatedFakeEntities.Clear();
		}

		public static async Task TeardownCosmosDbAsync()
		{
			var context = DiiCosmosContext.Get();

			if (context.Db != null)
			{
				_ = await context.Db.DeleteAsync().ConfigureAwait(false);
			}
		}

		public static void ResetContextInstance()
		{
			var instance = DiiCosmosContext.Get();
			var type = typeof(DiiCosmosContext);

			PropertyInfo configField = type.GetProperty("Config", _publicBindingFlags);
			configField.SetValue(instance, null);

			FieldInfo clientField = type.GetField("Client", _publicBindingFlags);
			clientField.SetValue(instance, null);

			FieldInfo instanceField = type.GetField("_instance", _privateBindingFlags);
			instanceField.SetValue(instance, null);
		}

		public static void ResetOptimizerInstance()
		{
			var instance = Optimizer.Get();
			var type = typeof(Optimizer);

			FieldInfo builderField = type.GetField("_builder", _privateBindingFlags);
			builderField.SetValue(instance, null);

			FieldInfo packingField = type.GetField("_packing", _privateBindingFlags);
			packingField.SetValue(instance, null);

			FieldInfo unpackingField = type.GetField("_unpacking", _privateBindingFlags);
			unpackingField.SetValue(instance, null);

			FieldInfo ignoreField = type.GetField("_ignoreInvalidDiiEntities", _privateBindingFlags);
			ignoreField.SetValue(instance, false);

			FieldInfo autoDetectField = type.GetField("_autoDetectTypes", _privateBindingFlags);
			autoDetectField.SetValue(instance, false);

			FieldInfo tablesField = type.GetField("Tables", _publicBindingFlags);
			tablesField.SetValue(instance, null);

			FieldInfo tableMappingsField = type.GetField("TableMappings", _publicBindingFlags);
			tableMappingsField.SetValue(instance, null);

			FieldInfo subPropertyMappingField = type.GetField("SubPropertyMapping", _publicBindingFlags);
			subPropertyMappingField.SetValue(instance, null);

			FieldInfo instanceField = type.GetField("_instance", _privateBindingFlags);
			instanceField.SetValue(instance, null);
		}
		#endregion Teardown

		#region Assert
		public static void AssertOptimizerIsInitialized()
		{
			var optimizer = Optimizer.Get();

			Assert.NotNull(optimizer);

			var tablesInitialized = optimizer.Tables;
			var tableMappingsInitialized = optimizer.TableMappings;

			Assert.Equal(tablesInitialized.Count, optimizer.Tables.Count);
			Assert.Equal(tableMappingsInitialized.Count, optimizer.TableMappings.Count);

            for (var i = 0; i < tablesInitialized.Count; i++)
			{
				Assert.Equal(tablesInitialized[i].TableName, optimizer.Tables[i].TableName);
			}

			foreach (var tableType in tableMappingsInitialized.Keys)
			{
				Assert.Equal(tableMappingsInitialized[tableType].TableName, optimizer.TableMappings[tableType].TableName);
			}
		}

		public static void AssertContextAndOptimizerAreInitialized()
		{
			var context = DiiCosmosContext.Get();

			Assert.NotNull(context);
			Assert.NotNull(context.TableMappings[typeof(FakeEntity)]);

			var optimizer = Optimizer.Get();

			Assert.NotNull(optimizer);
			Assert.NotNull(optimizer.Tables.FirstOrDefault(x => x.TableName == nameof(FakeEntity)));
			Assert.NotNull(optimizer.TableMappings[typeof(FakeEntity)]);
		}

		public static void AssertFakeEntitiesMatch(FakeEntity expected, FakeEntity actual, bool checkPrimitiveProperties = false)
		{
			// Searchable Fields
			Assert.Equal(expected.FakeEntityId, actual.FakeEntityId);
			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.SearchableIntegerValue, actual.SearchableIntegerValue);
			Assert.Equal(expected.SearchableDecimalValue, actual.SearchableDecimalValue);
			Assert.Equal(expected.SearchableStringValue, actual.SearchableStringValue);
			Assert.Equal(expected.SearchableGuidValue, actual.SearchableGuidValue);
			Assert.Equal(expected.SearchableListValue.Count, actual.SearchableListValue.Count);

			for (var i = 0; i < expected.SearchableListValue.Count; i++)
			{
				Assert.Equal(expected.SearchableListValue[i], actual.SearchableListValue[i]);
			}

			Assert.Equal(expected.SearchableDateTimeValue, actual.SearchableDateTimeValue);
			Assert.Equal(expected.SearchableEnumValue, actual.SearchableEnumValue);

			if (checkPrimitiveProperties)
			{
				Assert.Equal(expected.DataVersion, actual.DataVersion);
				Assert.Equal(expected.SearchableCosmosPrimitive, actual.SearchableCosmosPrimitive);
			}

			// Compressed Top Level Fields
			Assert.Equal(expected.CompressedIntegerValue, actual.CompressedIntegerValue);
			Assert.Equal(expected.CompressedDecimalValue, actual.CompressedDecimalValue);
			Assert.Equal(expected.CompressedStringValue, actual.CompressedStringValue);
			Assert.Equal(expected.CompressedGuidValue, actual.CompressedGuidValue);
			Assert.Equal(expected.CompressedListValue.Count, actual.CompressedListValue.Count);

			for (var i = 0; i < expected.CompressedListValue.Count; i++)
			{
				Assert.Equal(expected.CompressedListValue[i], actual.CompressedListValue[i]);
			}

			Assert.Equal(expected.CompressedDateTimeValue, actual.CompressedDateTimeValue);
			Assert.Equal(expected.CompressedEnumValue, actual.CompressedEnumValue);

			// Complex Compressed Fields
			Assert.Equal(expected.CompressedPackedEntity.PackedIntegerValue, actual.CompressedPackedEntity.PackedIntegerValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedDecimalValue, actual.CompressedPackedEntity.PackedDecimalValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedStringValue, actual.CompressedPackedEntity.PackedStringValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedGuidValue, actual.CompressedPackedEntity.PackedGuidValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedListValue.Count, actual.CompressedPackedEntity.PackedListValue.Count);
			Assert.Equal(expected.CompressedListValue.Count, actual.CompressedListValue.Count);

			for (var i = 0; i < expected.CompressedListValue.Count; i++)
			{
				Assert.Equal(expected.CompressedListValue[i], actual.CompressedListValue[i]);
			}

			Assert.Equal(expected.CompressedPackedEntity.PackedDateTimeValue, actual.CompressedPackedEntity.PackedDateTimeValue);
			Assert.Equal(expected.CompressedPackedEntity.PackedEnumValue, actual.CompressedPackedEntity.PackedEnumValue);
		}
		#endregion Assert

		#endregion Public Methods
    }
}