using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using dii.storage.Models;
using dii.storage.Attributes;
using static dii.storage.Optimizer;
using System.Data;

namespace dii.storage.Utilities
{

    public class DynamicTypeCreator
    {
        public static Type CreateDynamicType(string typeName, List<PropertyInfo> properties)
        {

            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            //var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, null, new[] { typeof(IDiiLookupEntity) });
            //var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(object), new[] { typeof(IDiiEntity) });
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            // Define a default constructor (a constructor that takes no parameters)
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            // Get the ILGenerator for the constructor
            var ilGenerator = constructorBuilder.GetILGenerator();

            // Load 'this' onto the evaluation stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Load the address of the base (System.Object) constructor
            var objectConstructor = typeof(object).GetConstructor(new Type[0]);
            ilGenerator.Emit(OpCodes.Call, objectConstructor);

            // Return from the constructor method
            ilGenerator.Emit(OpCodes.Ret);

            //properties = properties.Where(p => p.Name != "SchemaVersion" && p.Name != "DataVersion" && p.Name != "LastUpdated").ToList();
            foreach (var property in properties)
            {
                DefineProperty(typeBuilder, property.Name, property.PropertyType);
            }

            //These are for the interface implementations...handle separately
            var schemaVersion = properties.FirstOrDefault(p => p.Name == "SchemaVersion");
            if (schemaVersion == null)
            {
                DefineProperty(typeBuilder, "SchemaVersion", typeof(Version));
            }

            //var dataVersion = properties.FirstOrDefault(p => p.Name == "DataVersion");
            //if (dataVersion == null)
            //{
            //    DefineProperty(typeBuilder, "DataVersion", typeof(string));
            //}

            //var lastUpdated = properties.FirstOrDefault(p => p.Name == "LastUpdated");
            //if (lastUpdated == null)
            //{
            //    DefineProperty(typeBuilder, "LastUpdated", typeof(DateTime));
            //}

            return typeBuilder.CreateTypeInfo().AsType();
        }

        public static Type CreateLookupType(Dictionary<int, PropertyInfo> lookupHpks, Dictionary<int, PropertyInfo> lookupIds, List<PropertyInfo> searchableFields, TableMetaData lookupTableMetaData)
        {
            var assemblyName = new AssemblyName("TempDynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TempDynamicModule");

            var typeBuilder = moduleBuilder.DefineType(lookupTableMetaData.TableName, TypeAttributes.Public);
            //var typeConst = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var props = lookupIds?.Select(x => x.Value).ToList() ?? new List<PropertyInfo>();
            if (props.Count == 0)
            {
                throw new InvalidOperationException("Lookup tables must have at least one LookupId property.");
            }
            for (int i = 0; i < props.Count; i++)
            {
                var idAttribute = props[i].GetCustomAttribute<LookupIdAttribute>();
                if (idAttribute != null)
                {
                    SetAttributedProperty(typeBuilder, props[i], idAttribute, typeof(LookupIdAttribute));
                }
            }
            if (lookupHpks != null)
            {
                for (int i = 0; i < lookupHpks.Count; i++)
                {
                    var hpkAttribute = lookupHpks[i].GetCustomAttribute<LookupHpkAttribute>();
                    if (hpkAttribute != null)
                    {
                        SetAttributedProperty(typeBuilder, lookupHpks[i], hpkAttribute, typeof(LookupHpkAttribute));
                        props.Add(lookupHpks[i]);
                    }
                }
            }
            if (searchableFields != null)
            {
                for (int i = 0; i < searchableFields.Count; i++)
                {
                    if (!props.Contains(searchableFields[i]))
                    {
                        var searchableAttribute = searchableFields[i].GetCustomAttribute<SearchableAttribute>();
                        if (searchableAttribute != null)
                        {
                            SetAttributedProperty(typeBuilder, searchableFields[i], searchableAttribute, typeof(SearchableAttribute));
                            props.Add(searchableFields[i]);
                        }
                    }
                }
            }

            return typeBuilder.CreateTypeInfo().AsType(); //now that all the (base) properties are there, create the type

        }

        public static TypeBuilder CreateTypeWithPropertyAsString(PropertyInfo originalProperty, ModuleBuilder moduleBuilder)
        {
            string typeName = "DynamicType_" + Guid.NewGuid().ToString().Replace("-", "");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            // Define a single property of type string using the original property's name
            var propertyName = originalProperty.Name;
            var propertyType = typeof(string);
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Create get and set methods for the property and associate them with the property
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public, propertyType, Type.EmptyTypes);
            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public, null, new Type[] { propertyType });
            var setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            return typeBuilder;
        }


        private static void SetAttributedProperty(TypeBuilder typeBuilder, PropertyInfo property, DiiBaseAttribute attribute, Type attributeType)
        {
            //var att = property.GetCustomAttribute<SearchableAttribute>();
            if (attribute != null)
            {
                // Define a single property "Name" of type string
                var propertyName = property.Name;
                var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", property.PropertyType, FieldAttributes.Private);
                var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, property.PropertyType, null);

                // Getting the right constructor
                //var ctor = attributeType.GetConstructor(new[] { typeof(Type), typeof(int) });

                // Creating the CustomAttributeBuilder with extracted values
                var attributeBuilder = attribute.GetConstructorBuilder();
                //	new CustomAttributeBuilder(
                //	ctor,
                //	new object[] { attribute.IdKeyType, attribute.Order }
                //);

                propertyBuilder.SetCustomAttribute(attributeBuilder);

                // Create get and set methods for the property and associate them with the property
                var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public, property.PropertyType, Type.EmptyTypes);
                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);

                var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public, null, new Type[] { property.PropertyType });
                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }
        }

        private static void DefineProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, bool getterOnly = false)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public, propertyType, Type.EmptyTypes);
            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);

            if (!getterOnly)
            {
                var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public, null, new Type[] { propertyType });
                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
            }           
        }
    }


}
