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
        public static Type CreateLookupType(Dictionary<int, PropertyInfo> lookupHpks, Dictionary<int, PropertyInfo> lookupIds, List<PropertyInfo> otherFields, TableMetaData lookupTableMetaData)
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
                    AddProperty(typeBuilder, props[i].Name, props[i].PropertyType, idAttribute.GetConstructorBuilder());
                }
            }
            if (lookupHpks != null)
            {
                for (int i = 0; i < lookupHpks.Count; i++)
                {
                    var hpkAttribute = lookupHpks[i].GetCustomAttribute<LookupHpkAttribute>();
                    if (hpkAttribute != null)
                    {
                        AddProperty(typeBuilder, lookupHpks[i].Name, lookupHpks[i].PropertyType, hpkAttribute.GetConstructorBuilder());
                        props.Add(lookupHpks[i]);
                    }
                }
            }
            if (otherFields != null)
            {
                //These would/should be the HPK and Id properties for the source object
                for (int i = 0; i < otherFields.Count; i++)
                {
                    if (!props.Contains(otherFields[i]))
                    {
                        var searchableAttribute = otherFields[i].GetCustomAttribute<SearchableAttribute>();
                        if (searchableAttribute != null)
                        {
                            AddProperty(typeBuilder, otherFields[i].Name, otherFields[i].PropertyType, searchableAttribute.GetConstructorBuilder());
                        }
                        else
                        {
                            AddProperty(typeBuilder, otherFields[i].Name, otherFields[i].PropertyType); //no attribute, just set the property
                        }
                        props.Add(otherFields[i]);
                    }
                }
            }

            //Add the Dii TimestampEpoch property  of type int
            AddProperty(typeBuilder, Constants.ReservedTimestampKey, typeof(long)); //DiiBasicEntity.DiiTimestampEpoch

            //Add the DataVersion property of type string for optimistic concurrency
            AddProperty(typeBuilder, Constants.ReservedDataVersionKey, typeof(string)); //DiiBasicEntity.DataVersion

            return typeBuilder.CreateTypeInfo().AsType(); //now that all the (base) properties are there, create the type

        }

        protected static PropertyBuilder AddProperty(TypeBuilder typeBuilder, string name, Type propertyType, params CustomAttributeBuilder[] customAttributeBuilders)
        {
            var field = typeBuilder.DefineField($"_{name}", propertyType, FieldAttributes.Private);
            var prop = typeBuilder.DefineProperty(name, PropertyAttributes.None, propertyType, null);

            // Create the Get Accessor
            var propGet = typeBuilder.DefineMethod($"get_{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes);

            var getIL = propGet.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, field);
            getIL.Emit(OpCodes.Ret);

            // Create the Set Accessor
            var propSet = typeBuilder.DefineMethod($"set_{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { propertyType });

            var setIL = propSet.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, field);
            setIL.Emit(OpCodes.Ret);

            // Bind them to the prop
            prop.SetGetMethod(propGet);
            prop.SetSetMethod(propSet);

            foreach (var customAttributeBuilder in customAttributeBuilders)
            {
                prop.SetCustomAttribute(customAttributeBuilder);
            }

            return prop;
        }

    }


}
