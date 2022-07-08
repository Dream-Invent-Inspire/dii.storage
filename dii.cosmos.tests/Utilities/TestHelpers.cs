using dii.cosmos.tests.Fixtures;
using dii.cosmos.tests.Models;
using Microsoft.Azure.Cosmos;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace dii.cosmos.tests.Utilities
{
    public static class TestHelpers
    {
		#region Private Fields
		private static BindingFlags _privateBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
		private static BindingFlags _publicBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;
		#endregion Private Fields

		#region Public Methods

		#region Teardown
		public static async Task DeleteAllFakeEntitiesAsync(AdapterFixture adapterFixture)
		{
			for (var i = 0; i < adapterFixture.CreatedFakeEntities.Count; i++)
			{
				var success = await adapterFixture.FakeEntityAdapter.DeleteAsync(adapterFixture.CreatedFakeEntities[i].Id, adapterFixture.CreatedFakeEntities[i].FakeEntityId).ConfigureAwait(false);
				var shouldBeNull = await adapterFixture.FakeEntityAdapter.GetAsync(adapterFixture.CreatedFakeEntities[i].Id, adapterFixture.CreatedFakeEntities[i].FakeEntityId).ConfigureAwait(false);

				Assert.True(success);
				Assert.Null(shouldBeNull);
			}

			adapterFixture.CreatedFakeEntityTwos.Clear();
		}

		public static async Task DeleteAllFakeEntityTwosAsync(AdapterFixture adapterFixture)
		{
			for (var i = 0; i < adapterFixture.CreatedFakeEntityTwos.Count; i++)
			{
				var success = await adapterFixture.FakeEntityTwoAdapter.DeleteAsync(adapterFixture.CreatedFakeEntityTwos[i].Id, adapterFixture.CreatedFakeEntityTwos[i].FakeEntityTwoId).ConfigureAwait(false);
				var shouldBeNull = await adapterFixture.FakeEntityTwoAdapter.GetAsync(adapterFixture.CreatedFakeEntityTwos[i].Id, adapterFixture.CreatedFakeEntityTwos[i].FakeEntityTwoId).ConfigureAwait(false);

				Assert.True(success);
				Assert.Null(shouldBeNull);
			}

			adapterFixture.CreatedFakeEntityTwos.Clear();
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

			FieldInfo ignoreField = type.GetField("_ignoreInvalidDiiCosmosEntities", _privateBindingFlags);
			ignoreField.SetValue(instance, false);

			FieldInfo autoDetectField = type.GetField("_autoDetectTypes", _privateBindingFlags);
			autoDetectField.SetValue(instance, false);

			FieldInfo tablesField = type.GetField("Tables", _publicBindingFlags);
			tablesField.SetValue(instance, null);

			FieldInfo tableMappingsField = type.GetField("TableMappings", _publicBindingFlags);
			tableMappingsField.SetValue(instance, null);

			FieldInfo instanceField = type.GetField("_instance", _privateBindingFlags);
			instanceField.SetValue(instance, null);
		}
		#endregion Teardown

		#region Assert
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

		public static void AssertFakeEntityTwosMatch(FakeEntityTwo expected, FakeEntityTwo actual, bool checkPrimitiveProperties = false)
		{
			// Searchable Fields
			Assert.Equal(expected.FakeEntityTwoId, actual.FakeEntityTwoId);
			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.SearchableStringValue, actual.SearchableStringValue);

			if (checkPrimitiveProperties)
			{
				Assert.Equal(expected.DataVersion, actual.DataVersion);
			}

			// Compressed Top Level Fields
			Assert.Equal(expected.CompressedStringValue, actual.CompressedStringValue);
		}
		#endregion Assert

		#endregion Public Methods
    }
}