using System.Reflection;
using System.Threading.Tasks;

namespace dii.cosmos.tests.Utilities
{
    public static class TestHelpers
    {
        private static BindingFlags _privateBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
        private static BindingFlags _publicBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;

        public static async Task TeardownCosmosDbAsync()
        {
            var context = Context.Get();

            if (context.Db != null)
            {
                _ = await context.Db.DeleteAsync().ConfigureAwait(false);
            }
        }

        public static void ResetContextInstance()
        {
            var instance = Context.Get();
            var type = typeof(Context);

            FieldInfo configField = type.GetField("Config", _publicBindingFlags);
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
    }
}