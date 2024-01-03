using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static dii.storage.Optimizer;

namespace dii.storage
{
    public class TypeGenerator
    {
        #region Properties...
        /// <summary>
        /// The <seealso cref="Type"/> of the source entity for 
        /// mapping to the new compressable type.
        /// </summary>
        protected Type sourceType { get; set; }

        /// <summary>
        /// The Array of <seealso cref="PropertyInfo"/> for the source entity's 
        /// properties.
        /// </summary>
        protected PropertyInfo[] sourceProperties { get; set; }

        /// <summary>
        /// The <seealso cref="TypeBuilder"/> for the housing type being
        /// generated. This is what is serialized in normal
        /// JSON format.
        /// </summary>
        protected TypeBuilder typeBuilder;

        /// <summary>
        /// The constructor for the housing type.
        /// </summary>
        protected ConstructorBuilder typeConst;

        /// <summary>
        /// The type for the <seealso cref="TypeBuilder"/> for the 
        /// sub-object containing compressed values. This is 
        /// serialized using MessagePack
        /// </summary>
        protected TypeBuilder compressBuilder;

        /// <summary>
        /// The constructor for the sub-object containing compressed
        /// values.
        /// </summary>
        protected ConstructorBuilder compressConst;

        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="MessagePack.MessagePackObjectAttribute"/>
        /// </summary>
        protected ConstructorInfo msgPkAttrConst { get {  return Constants.MessagePackAttributeConstructor; } }

        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="JsonPropertyNameAttribute"/>
        /// </summary>
        protected ConstructorInfo jsonAttrConstructor {  get { return Constants.JsonPropertyNameAttributeConstructor; } }

        /// <summary>
        /// The <see cref="ConstructorInfo"/> for the <see cref="KeyAttribute"/>
        /// </summary>
        protected ConstructorInfo compressKeyAttrConstructor { get { return Constants.CompressKeyAttributeConstructor; } }

        /// <summary>
        /// The DynamicModule Builder added to the assembly to house the generated storage types.
        /// </summary>
        protected ModuleBuilder moduleBuilder { get; private set; }

        protected bool suppressConfigErrors { get; private set; }

        protected PackingMapper jsonMap { get; private set; }
        protected PackingMapper compressMap { get; private set; }

        protected Dictionary<string, PropertyInfo> workingPropertySet { get; set; } = new Dictionary<string, PropertyInfo>();


        private readonly string[] _reservedSearchableKeys = new string[3]
        {
            Constants.ReservedPartitionKeyKey,
            Constants.ReservedIdKey,
            Constants.ReservedCompressedKey
        };

        public readonly Dictionary<Type, Type> SubPropertyMapping;

        public Type GeneratedType { get; private set; }
        #endregion
        public TypeGenerator(Type source, ModuleBuilder builder, Dictionary<Type,Type> subPropertyMappings, bool suppressConfigurationErrors = false)
        {
            moduleBuilder = builder;
            if (moduleBuilder.GetType(source.Name) != null)
            {
                throw new DiiDuplicateTypeInitializedException(source);
            }
            suppressConfigErrors = suppressConfigurationErrors;
            SubPropertyMapping = subPropertyMappings;

            jsonMap = new PackingMapper();
            compressMap = new PackingMapper();

            sourceType = source;
            sourceProperties = source.GetProperties();
            typeBuilder = moduleBuilder.DefineType(source.Name, TypeAttributes.Public);
            var typeConst = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            
            compressBuilder = moduleBuilder.DefineType($"{source.Name}Compressed", TypeAttributes.Public);
            compressConst = compressBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            compressBuilder.SetCustomAttribute(new CustomAttributeBuilder(msgPkAttrConst, new object[] { false }));

        }

        public Serializer Generate()
        {
            if (this.typeBuilder != null)
            {
                foreach (var p in sourceProperties)
                {
                    try
                    {
                        ProcessProperty(p);
                    }
                    catch (Exception ex)
                    {
                        if (
                            suppressConfigErrors &&
                            (
                                ex is DiiPartitionKeyDuplicateOrderException
                                || ex is DiiIdDuplicateOrderException
                                || ex is DiiReservedSearchableKeyException
                            )
                        )
                        {
                            //If a handled configuration exception is thrown and suppressConfigErrors
                            //is configured, do not create or map the type and halt processing.
                            //Useful for debugging multiple new types with advanced configuration
                            return null;
                        }
                        throw;
                    }
                }

                AppendExtendedFields();
            }
            var serializer = AppendStandardFields();
            MapTypes( serializer );
            return serializer;
        }

        protected virtual void MapTypes(Serializer serializer)
        {
            if (sourceType == null)
            {
                return;
            }
            if (!SubPropertyMapping.ContainsKey(sourceType))
            {
                SubPropertyMapping.Add(sourceType, serializer.StoredEntityType);
            }
        }

        protected void ProcessProperty(PropertyInfo p)
        {
            ProcessExtended(p);
            ProcessSearchable(p);
            ProcessCompressed(p);
        }

        protected Serializer AppendStandardFields()
        {
            var jsonAttachmentAttr = new CustomAttributeBuilder(jsonAttrConstructor, new object[] { Constants.ReservedCompressedKey });
            _ = AddProperty(typeBuilder, Constants.ReservedCompressedKey, typeof(string), jsonAttachmentAttr);

            //Add the timestamp and version fields
            _ = AddProperty(typeBuilder, Constants.ReservedTimestampKey, typeof(long));
            _ = AddProperty(typeBuilder, Constants.ReservedDataVersionKey, typeof(string));
            _ = AddProperty(typeBuilder, Constants.ReservedChangeTrackerKey, typeof(string));
            //This is the last opportunity to add properties to the type.

            Type storageEntityType = typeBuilder.CreateTypeInfo();
            Type compressedEntityType = compressBuilder.CreateTypeInfo();
            PropertyInfo attachments;
            
            var serializer = new Serializer();

            //At this point a Dynamicly Emited Type currently in storageEntityType
            //is not the same as the concrete type.  At the time of writing
            //to handle the mapping appropriately you have to instantiate, GetType()
            //to get the newly concrete type to work with.
            var jsonInstance = Activator.CreateInstance(storageEntityType);
            storageEntityType = jsonInstance.GetType();

            //Handle mapping for the JSON stored entity.
            var jsonProps = storageEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);
            attachments = jsonProps[Constants.ReservedCompressedKey];
            jsonProps.Remove(Constants.ReservedCompressedKey);

            CustomizeSearchableSerializer(jsonProps, serializer);
            jsonMap.EmitProperties = jsonProps;

            //Handle mapping for the compressed entity fragments.
            var compressInstance = Activator.CreateInstance(compressedEntityType);
            compressedEntityType = compressInstance.GetType();
            var compressProps = compressedEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);

            CustomizeCompressedSerializer(compressProps, serializer);
            compressMap.EmitProperties = compressProps;

            //Map remaining Serializer properties.
            serializer.Attachment = attachments;
            serializer.StoredEntityMapping = jsonMap;
            serializer.CompressedEntityMapping = compressMap;
            serializer.StoredEntityType = storageEntityType;
            serializer.CompressedEntityType = compressedEntityType;
            
            PropertyInfo trackerPropertyInfo = storageEntityType.GetProperty(Constants.ReservedChangeTrackerKey);
            if (trackerPropertyInfo != null)
            {
                serializer.diiChangeTrackerProperty = trackerPropertyInfo;
            }
            return serializer;
        }

        protected virtual void AppendExtendedFields() { }

        protected virtual Serializer CustomizeSearchableSerializer(Dictionary<string, PropertyInfo> properties, Serializer serializer) { return serializer; }
        protected virtual Serializer CustomizeCompressedSerializer(Dictionary<string, PropertyInfo> properties, Serializer serializer) { return serializer; }

        protected void ProcessSearchable(PropertyInfo p)
        {
            var search = p.GetCustomAttribute<SearchableAttribute>();
            if (search == null)
            {
                //Short circuit if the property is not mapped with
                //searchable.
                return;
            }

            if (_reservedSearchableKeys.Contains(search.Abbreviation))
            {
                // Throw an exception when an invalid abbreviation is used.
                // This protects against re-using Cosmos pre-defined reserved words.
                throw new DiiReservedSearchableKeyException(search.Abbreviation, p.Name, sourceType.Name);
            }

            //We must handle collection navigation to handle searchable and compressed types within
            //a collection.
            var isEnumerable = typeof(IEnumerable<>).IsAssignableFrom(p.PropertyType);
            Type enumerableType = null;
            if (isEnumerable)
            {
                var listTrueTypes = p.PropertyType.GetGenericArguments();
                foreach (var t in listTrueTypes)
                {
                    //We do not need a proxy for primitives. They are already optimized.
                    if (t.IsPrimitive)
                    {
                        enumerableType = t;
                        continue;
                    }

                    var existingType = moduleBuilder.GetType(t.Name);
                    if (existingType != null)
                    {
                        var tmp = new TypeGenerator(t, moduleBuilder, SubPropertyMapping, suppressConfigErrors);
                        var tmpSer = tmp.Generate();
                        existingType = enumerableType = tmpSer.StoredEntityType;
                    }

                    if (!SubPropertyMapping.ContainsKey(t))
                    {
                        SubPropertyMapping.Add(t, existingType);
                    }
                }
            }

            var jsonAttr = new CustomAttributeBuilder(jsonAttrConstructor, new object[] { search.Abbreviation });

            //Below block handles complex sub-types.
            var childProps = p.PropertyType.GetProperties();
            var searchableChildren = childProps.Select(x => new { Prop = x, SearchConfig = x.GetCustomAttribute<SearchableAttribute>() }).Where(x => x.SearchConfig != null).ToDictionary(x => x.Prop, x => x.SearchConfig);
            var compressChildren = childProps.Select(x => new { Prop = x, CompressConfig = x.GetCustomAttribute<CompressAttribute>() }).Where(x => x.CompressConfig != null).ToDictionary(x => x.Prop, x => x.CompressConfig);
            if (!isEnumerable && (searchableChildren.Any() || compressChildren.Any()))
            {
                if (p.PropertyType.GetInterface(nameof(IDiiEntity)) != null)
                {
                    // Throw an exception when an invalid type is attempted.
                    throw new DiiInvalidNestingException(sourceType.Name);
                }

                Type childType = null;
                //If the type is not mapped already
                if (!SubPropertyMapping.ContainsKey(p.PropertyType))
                {
                    var existingType = moduleBuilder.GetType(p.PropertyType.Name);
                    //This means a TableType is being re-used.  This is less
                    //than ideal but not exception-worthy. Up to the caller to
                    //not make silly object structures.
                    if (existingType == null)
                    {

                        //Generate the storable type
                        var tmp = new TypeGenerator(p.PropertyType, moduleBuilder, SubPropertyMapping, suppressConfigErrors);
                        var tmpSer = tmp.Generate();
                        existingType = childType = tmpSer.StoredEntityType;
                        //Register the sub-type.
                        OptimizedTypeRegistrar.Register(p.PropertyType, tmpSer);
                    }

                    //There is a chance the TypeGenerator self registers the type
                    //if it is nested within the generated type.
                    if (!SubPropertyMapping.ContainsKey(p.PropertyType))
                        SubPropertyMapping.Add(p.PropertyType, existingType);
                    else
                        childType = SubPropertyMapping[p.PropertyType];
                }
                else
                {
                    childType = SubPropertyMapping[p.PropertyType];
                }

                if (childType != null)
                {
                    //Map the storable type to the collection property.
                    _ = AddProperty(typeBuilder, search.Abbreviation, childType, jsonAttr);
                }
            }
            else if (isEnumerable)
            {
                var listGeneric = typeof(List<>);
                //Turn the list into the actual concrete type.
                //We use List<> here because we can just affix any collection to this type
                //for serialization.
                var listType = listGeneric.MakeGenericType(enumerableType);
                _ = AddProperty(typeBuilder, search.Abbreviation, listType, jsonAttr);
            }
            else
            {
                _ = AddProperty(typeBuilder, search.Abbreviation, p.PropertyType, jsonAttr);
            }

            if (!jsonMap.ConcreteProperties.ContainsKey(search.Abbreviation))
            {
                jsonMap.ConcreteProperties.Add(search.Abbreviation, p);
            }
        }

        protected void ProcessCompressed(PropertyInfo p)
        {
            var compress = p.GetCustomAttribute<CompressAttribute>();
            if (compress != null)
            {
                var compressAttr = new CustomAttributeBuilder(compressKeyAttrConstructor, new object[] { compress.Order });

                _ = AddProperty(compressBuilder, p.Name, p.PropertyType, compressAttr);
                compressMap.ConcreteProperties.Add(p.Name, p);
            }
        }

        protected PropertyBuilder AddProperty(TypeBuilder typeBuilder, string name, Type propertyType, params CustomAttributeBuilder[] customAttributeBuilders)
        {
            if (this.workingPropertySet.ContainsKey(name)) return null;

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
            this.workingPropertySet.Add(name, prop);

            return prop;
        }

        protected virtual void ProcessExtended(PropertyInfo p) { }
    }
}
