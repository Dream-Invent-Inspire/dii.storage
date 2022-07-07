using dii.cosmos.Attributes;
using dii.cosmos.Exceptions;
using dii.cosmos.Models;
using dii.cosmos.Models.Interfaces;
using MessagePack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dii.cosmos
{
    public class Optimizer
	{
		#region Private Fields
		private static Optimizer _instance;
		private static object _instanceLock = new object();
		private static bool _autoDetectTypes;
		private static bool _ignoreInvalidDiiCosmosEntities;

		private readonly ModuleBuilder _builder;
		private readonly Dictionary<Type, Serializer> _packing;
		private readonly Dictionary<Type, Serializer> _unpacking;

		private const string _dynamicAssemblyName = "dii.dynamic.storage";

		private readonly string[] _reservedSearchableKeys = new string[3]
		{
			Constants.ReservedPartitionKeyKey,
			Constants.ReservedIdKey,
			Constants.ReservedCompressedKey
		};
		#endregion Private Fields

		#region Public Fields
		public readonly List<TableMetaData> Tables;
		public readonly Dictionary<Type, TableMetaData> TableMappings;
		#endregion Public Fields

		#region Constructors
		private Optimizer(params Type[] types)
		{
			_packing = new Dictionary<Type, Serializer>();
			_unpacking = new Dictionary<Type, Serializer>();

			Tables = new List<TableMetaData>();
			TableMappings = new Dictionary<Type, TableMetaData>();

			var assemblyName = new AssemblyName($"{_dynamicAssemblyName}.{Guid.NewGuid()}");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

			_builder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

			ConfigureTypes(types);
		}
		#endregion Constructors

		#region Public Methods
		public static Optimizer Init(params Type[] types)
		{
			return InitializeOptimizer(types);
		}

		public static Optimizer Init(bool ignoreInvalidDiiCosmosEntities = false, params Type[] types)
		{
			_ignoreInvalidDiiCosmosEntities = ignoreInvalidDiiCosmosEntities;

			return InitializeOptimizer(types);
		}

		public static Optimizer Init(bool autoDetectTypes = false, bool ignoreInvalidDiiCosmosEntities = false)
		{
			_autoDetectTypes = autoDetectTypes;
			_ignoreInvalidDiiCosmosEntities = ignoreInvalidDiiCosmosEntities;

			return InitializeOptimizer();
		}

		public static Optimizer Get()
		{
			if (_instance == null)
			{
				throw new DiiNotInitializedException(nameof(Optimizer));
			}

			return _instance;
		}

		public void ConfigureTypes(params Type[] types)
		{
			if (_autoDetectTypes)
			{
				var currentAssemblyName = Assembly.GetExecutingAssembly().FullName;

				// Look for types.
				types = AppDomain.CurrentDomain
					.GetAssemblies()
					.Where(x => !x.IsDynamic && x.FullName != currentAssemblyName)
					.SelectMany(x => x.GetExportedTypes())
					.Where(x => x.GetTypeInfo().ImplementedInterfaces.Any(z => z.Name == nameof(IDiiEntity)))
					.ToArray();
			}

			if (types != null && types.Any())
			{
				foreach (var type in types)
				{
					if (!IsKnownConcreteType(type))
					{
						var storageType = GenerateType(type);

						if (storageType != null)
						{
							var tableMetaData = new TableMetaData
							{
								TableName = type.Name,
								ConcreteType = type,
								StorageType = storageType
							};

							Tables.Add(tableMetaData);
							TableMappings.Add(type, tableMetaData);
						}
					}
				}
			}
		}

		public bool IsKnownConcreteType(Type type)
		{
			return _packing.ContainsKey(type);
		}

		public bool IsKnownEmitType(Type type)
		{
			return _unpacking.ContainsKey(type);
		}

		public object ToEntity<T>(T obj)
		{
			if (obj == null)
            {
				throw new ArgumentNullException(nameof(obj));
            }

			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				return _packing[type].Package(obj);
			}

			return null;
		}

		public T FromEntity<T>(object obj) where T : IDiiEntity, new()
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var type = obj.GetType();

			if (_unpacking.ContainsKey(type))
			{
				return _unpacking[type].Unpackage<T>(obj);
			}

			type = typeof(T);

			if (_packing.ContainsKey(type) && obj is JObject j)
			{
				return UnpackageFromJson<T>(j.ToString());
			}

			return default(T);
		}

		public string PackageToJson<T>(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var entity = ToEntity<T>(obj);

			if (entity != null)
			{
				return JsonSerializer.Serialize(entity);
			}

			return null;
		}

		public T UnpackageFromJson<T>(string json) where T : IDiiEntity, new()
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				throw new ArgumentNullException(nameof(json));
			}

			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				var obj = JsonSerializer.Deserialize(json, _packing[type].StoredEntityType);
				return _packing[type].Unpackage<T>(obj);
			}

			return default(T);
		}

		public TKey GetPartitionKey<T, TKey>(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				var partitionKeyValues = new List<object>();

				foreach (var property in _packing[type].PartitionKeyProperties)
				{
					partitionKeyValues.Add(property.GetValue(obj));
				}

				var partitionKeyString = string.Join(_packing[type].PartitionKeySeparator, partitionKeyValues);

				if (_packing[type].PartitionKeyType != typeof(string))
                {
					return (TKey)Activator.CreateInstance(_packing[type].PartitionKeyType, partitionKeyString);
                }
			}

			return default(TKey);
		}

		public string GetId<T>(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				var idValues = new List<object>();

				foreach (var property in _packing[type].IdProperties)
				{
					idValues.Add(property.GetValue(obj));
				}

				return string.Join(_packing[type].IdSeparator, idValues);
			}

			return null;
		}

		public Type GetPartitionKeyType<T>()
		{
			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				return _packing[type].PartitionKeyType;
			}

			return null;
		}
		#endregion Public Methods

		#region Private Methods
		private static Optimizer InitializeOptimizer(params Type[] types)
		{
			bool isNew = false;

			if (_instance == null)
			{
				lock (_instanceLock)
				{
					if (_instance == null)
					{
						isNew = true;
						_instance = new Optimizer(types);
					}
				}
			}

			if (!isNew)
			{
				_instance.ConfigureTypes(types);
			}

			return _instance;
		}

		private Type GenerateType(Type source)
		{
			var jsonMap = new PackingMapper();
			var compressMap = new PackingMapper();

			var typeBuilder = _builder.DefineType(source.Name, TypeAttributes.Public);
			var typeConst = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

			var compressBuilder = _builder.DefineType($"{source.Name}Compressed", TypeAttributes.Public);
			var compressConst = compressBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
			var msgPkAttrConst = typeof(MessagePackObjectAttribute).GetConstructor(new Type[] { typeof(bool) });
			compressBuilder.SetCustomAttribute(new CustomAttributeBuilder(msgPkAttrConst, new object[] { false }));

			var partitionSeparator = Constants.DefaultPartitionDelimitor;
			var partitionFields = new SortedList<int, PropertyInfo>();
			var partitionKeyType = typeof(string);

			var idSeparator = Constants.DefaultIdDelimitor;
			var idFields = new SortedList<int, PropertyInfo>();

			var jsonAttrConstructor = typeof(JsonPropertyNameAttribute).GetConstructor(new Type[] { typeof(string) });
			var compressAttrConstructor = typeof(KeyAttribute).GetConstructor(new Type[] { typeof(int) });

			foreach (var property in source.GetProperties())
			{
				var partitionKey = property.GetCustomAttribute<PartitionKeyAttribute>();
				if (partitionKey != null)
				{
					partitionFields.Add(partitionKey.Order, property);

					if (!char.IsWhiteSpace(partitionKey.Separator))
                    {
						partitionSeparator = partitionKey.Separator;
					}

					if (partitionKey.PartitionKeyType != null && partitionKey.PartitionKeyType != typeof(string))
                    {
						partitionKeyType = partitionKey.PartitionKeyType;
					}
					
					continue;
				}

				var id = property.GetCustomAttribute<IdAttribute>();
				if (id != null)
				{
					idFields.Add(id.Order, property);

					if (!char.IsWhiteSpace(id.Separator))
					{
						idSeparator = id.Separator;
					}

					continue;
				}

				var search = property.GetCustomAttribute<SearchableAttribute>();
				if (search != null)
				{
					if (_reservedSearchableKeys.Contains(search.Abbreviation))
					{
						if (_ignoreInvalidDiiCosmosEntities)
                        {
							// Do not create the entity to be used, but do not throw an exception.
							return null;
                        }
						else
						{
							// Throw an exception when an invalid type is attempted.
							throw new DiiReservedSearchableKeyException(search.Abbreviation, property.Name, source.Name);
						}
					}

					var jsonAttr = new CustomAttributeBuilder(jsonAttrConstructor, new object[] { search.Abbreviation });

					_ = AddProperty(typeBuilder, search.Abbreviation, property.PropertyType, jsonAttr);
					jsonMap.ConcreteProperties.Add(search.Abbreviation, property);

					continue;
				}

				var compress = property.GetCustomAttribute<CompressAttribute>();
				if (compress != null)
				{
					var compressAttr = new CustomAttributeBuilder(compressAttrConstructor, new object[] { compress.Order });

					_ = AddProperty(compressBuilder, property.Name, property.PropertyType, compressAttr);
					compressMap.ConcreteProperties.Add(property.Name, property);

					continue;
				}
			}

			var jsonAttachmentAttr = new CustomAttributeBuilder(jsonAttrConstructor, new object[] { Constants.ReservedCompressedKey });
			_ = AddProperty(typeBuilder, Constants.ReservedCompressedKey, typeof(string), jsonAttachmentAttr);
			_ = AddProperty(typeBuilder, Constants.ReservedPartitionKeyKey, typeof(string));
			_ = AddProperty(typeBuilder, Constants.ReservedIdKey, typeof(string));

			Type storageEntityType = typeBuilder.CreateTypeInfo();
			Type compressedEntityType = compressBuilder.CreateTypeInfo();
			PropertyInfo attachments;
			PropertyInfo partitionKeyInfo;
			PropertyInfo idInfo;

			{
				var instance = Activator.CreateInstance(storageEntityType);
				storageEntityType = instance.GetType();

				var props = storageEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);
				attachments = props[Constants.ReservedCompressedKey];
				props.Remove(Constants.ReservedCompressedKey);

				partitionKeyInfo = props[Constants.ReservedPartitionKeyKey];
				props.Remove(Constants.ReservedPartitionKeyKey);

				idInfo = props[Constants.ReservedIdKey];
				props.Remove(Constants.ReservedIdKey);

				jsonMap.EmitProperties = props;
			}

			{
				var instance = Activator.CreateInstance(compressedEntityType);
				compressedEntityType = instance.GetType();
				var props = compressedEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);

				compressMap.EmitProperties = props;
			}

			var serializer = new Serializer
			{
				PartitionKey = partitionKeyInfo,
				PartitionKeyProperties = partitionFields
					.OrderBy(x => x.Key)
					.ThenBy(x => x.Value.Name)
					.Select(x => x.Value)
					.ToList(),
				PartitionKeySeparator = partitionSeparator.ToString(),
				PartitionKeyType = partitionKeyType,
				Id = idInfo,
				IdProperties = idFields
					.OrderBy(x => x.Key)
					.ThenBy(x => x.Value.Name)
					.Select(x => x.Value)
					.ToList(),
				IdSeparator = idSeparator.ToString(),
				Attachment = attachments,
				StoredEntityMapping = jsonMap,
				CompressedEntityMapping = compressMap,
				StoredEntityType = storageEntityType,
				CompressedEntityType = compressedEntityType
			};

			_packing.Add(source, serializer);
			_unpacking.Add(serializer.StoredEntityType, serializer);

			return serializer.StoredEntityType;
		}

		private PropertyBuilder AddProperty(TypeBuilder typeBuilder, string name, Type propertyType, params CustomAttributeBuilder[] customAttributeBuilders)
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
        #endregion Private Methods

        #region Internal Serializer Class
		internal sealed class Serializer
		{
			#region Public Properties
			public PropertyInfo PartitionKey { get; set; }
			public List<PropertyInfo> PartitionKeyProperties { get; set; }
			public string PartitionKeySeparator { get; set; }
			public Type PartitionKeyType { get; set; }
			public PropertyInfo Id { get; set; }
			public List<PropertyInfo> IdProperties { get; set; }
			public string IdSeparator { get; set; }
			public PropertyInfo Attachment { get; set; }
			public PackingMapper StoredEntityMapping { get; set; }
			public PackingMapper CompressedEntityMapping { get; set; }
			public Type StoredEntityType { get; set; }
			public Type CompressedEntityType { get; set; }
			#endregion Public Properties

			#region Constructors
			public Serializer() { }
			#endregion Constructors

			#region Public Methods
			public object Package(object unpackedObject)
			{
				if (unpackedObject == null)
				{
					throw new ArgumentNullException(nameof(unpackedObject));
				}

				var packedObject = Activator.CreateInstance(StoredEntityType);
				var compressedEntity = Activator.CreateInstance(CompressedEntityType);

				var partitionKeyValues = new List<object>();

				foreach (var property in PartitionKeyProperties)
				{
					partitionKeyValues.Add(property.GetValue(unpackedObject));
				}

				PartitionKey.SetValue(packedObject, string.Join(PartitionKeySeparator, partitionKeyValues));

				var idValues = new List<object>();

				foreach (var property in IdProperties)
				{
					idValues.Add(property.GetValue(unpackedObject));
				}

				Id.SetValue(packedObject, string.Join(IdSeparator, idValues));

				foreach (var property in StoredEntityMapping.ConcreteProperties)
				{
					var val = property.Value.GetValue(unpackedObject);
					StoredEntityMapping.EmitProperties[property.Key].SetValue(packedObject, val);
				}

				foreach (var property in CompressedEntityMapping.ConcreteProperties)
				{
					var val = property.Value.GetValue(unpackedObject);
					CompressedEntityMapping.EmitProperties[property.Key].SetValue(compressedEntity, val);
				}

				var compressedBytes = MessagePackSerializer.Serialize(CompressedEntityType, compressedEntity);
				var compressedString = Convert.ToBase64String(compressedBytes);

				Attachment.SetValue(packedObject, compressedString);

				return packedObject;
			}

			public T Unpackage<T>(object packedObject) where T : IDiiEntity, new()
			{
				if (packedObject == null)
				{
					throw new ArgumentNullException(nameof(packedObject));
				}

				var unpackedObject = new T();

				var compressedString = (string)Attachment.GetValue(packedObject);

				if (string.IsNullOrEmpty(compressedString))
				{
					throw new ArgumentException("Packed object contained no properties.", nameof(packedObject));
				}

				var compressedBytes = Convert.FromBase64String(compressedString);
				var compressedObj = MessagePackSerializer.Deserialize(CompressedEntityType, compressedBytes);

				// Reverse engineer parition key string.
				var partitionKey = ((string)PartitionKey.GetValue(packedObject)).Split(new string[] { PartitionKeySeparator }, StringSplitOptions.None);

				for (var i = 0; i < PartitionKeyProperties.Count; i++)
				{
					PartitionKeyProperties[i].SetValue(unpackedObject, partitionKey[i]);
				}

				// Reverse engineer id string.
				var id = ((string)Id.GetValue(packedObject)).Split(new string[] { IdSeparator }, StringSplitOptions.None);

				for (var i = 0; i < IdProperties.Count; i++)
				{
					IdProperties[i].SetValue(unpackedObject, id[i]);
				}

				foreach (var property in CompressedEntityMapping.EmitProperties)
				{
					var val = property.Value.GetValue(compressedObj);
					CompressedEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, val);
				}

				foreach (var property in StoredEntityMapping.EmitProperties)
				{
					var val = property.Value.GetValue(packedObject);
					StoredEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, val);
				}

				return unpackedObject;
			}
			#endregion Public Methods
		}
		#endregion Internal Serializer Class
	}
}