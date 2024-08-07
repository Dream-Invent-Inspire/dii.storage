﻿using System.Reflection;
using Xunit;

namespace dii.storage.tests.Utilities
{
	public static class TestHelpers
    {
		#region Private Fields
		private static BindingFlags _privateBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
		private static BindingFlags _publicBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;
		#endregion Private Fields

		#region Public Methods

		#region Teardown
		public static void ResetOptimizerInstance()
		{
			var instance = Optimizer.Get();
			var type = typeof(Optimizer);

			FieldInfo builderField = type.GetField("_builder", _privateBindingFlags);
			builderField.SetValue(instance, null);

			OptimizedTypeRegistrar.ClearAllTypes(true);

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
                Assert.Equal(tablesInitialized[i].ClassName, optimizer.Tables[i].ClassName);
			}

			foreach (var tableType in tableMappingsInitialized.Keys)
            {
                Assert.Equal(tableMappingsInitialized[tableType].TableName, optimizer.TableMappings[tableType].TableName);
                Assert.Equal(tableMappingsInitialized[tableType].ClassName, optimizer.TableMappings[tableType].ClassName);
			}
		}

		#endregion Assert

		#endregion Public Methods
    }
}