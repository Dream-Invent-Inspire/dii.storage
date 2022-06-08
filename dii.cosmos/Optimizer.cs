using dii.cosmos.Attributes;
using dii.cosmos.Exceptions;
using dii.cosmos.Models;
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
		private static object _lock = new object();
		private static Optimizer _instance;

		private readonly ModuleBuilder _builder;
		private readonly Dictionary<Type, Serializer> _packing;
		private readonly Dictionary<Type, Serializer> _unpacking;

		private const string _dynamicAssemblyName = "dii.dynamic.storage";
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

			var assemblyName = new AssemblyName(_dynamicAssemblyName);
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

			_builder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

			ConfigureTypes(types);
		}
		#endregion Constructors

		#region Public Methods
		public static Optimizer Init(params Type[] types)
		{
			bool isNew = false;

			if (_instance == null)
			{
				lock (_lock)
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

		public static Optimizer Get()
		{
			if (_instance == null)
			{
				throw new DiiNotInitializedException("Optimizer");
			}

			return _instance;
		}

		public void ConfigureTypes(params Type[] types)
		{
			if (types != null && types.Any())
			{
				foreach (var type in types)
				{
					if (!IsKnownConcreteType(type))
					{
						var storageType = GenerateType(type);

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

		public T FromEntity<T>(object obj) where T : new()
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

		public T UnpackageFromJson<T>(string json) where T : new()
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
		#endregion Public Methods

		#region Private Methods
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

			var jsonAttrConstructor = typeof(JsonPropertyNameAttribute).GetConstructor(new Type[] { typeof(string) });
			var compressAttrConstructor = typeof(KeyAttribute).GetConstructor(new Type[] { typeof(int) });

			foreach (var property in source.GetProperties())
			{
				var partition = property.GetCustomAttribute<PartitionKeyAttribute>();
				if (partition != null)
				{
					partitionFields.Add(partition.Order, property);

					if (!char.IsWhiteSpace(partition.Separator))
                    {
						partitionSeparator = partition.Separator;
					}
					
					continue;
				}

				var search = property.GetCustomAttribute<SearchableAttribute>();
				if (search != null)
				{
					if (search.Abbreviation == Constants.ReservedCompressedKey)
					{
						throw new DiiReservedSearchableKeyException(Constants.ReservedCompressedKey, property.Name, source.Name);
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

			Type storageEntityType = typeBuilder.CreateTypeInfo();
			Type compressedEntityType = compressBuilder.CreateTypeInfo();
			PropertyInfo attachments;
			PropertyInfo partitionKeyInfo;

			{
				var instance = Activator.CreateInstance(storageEntityType);
				storageEntityType = instance.GetType();

				var props = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);
				attachments = props[Constants.ReservedCompressedKey];
				props.Remove(Constants.ReservedCompressedKey);

				partitionKeyInfo = props[Constants.ReservedPartitionKeyKey];
				props.Remove(Constants.ReservedPartitionKeyKey);

				jsonMap.EmitProperties = props;
			}

			{
				var instance = Activator.CreateInstance(compressedEntityType);
				var props = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);
				compressedEntityType = instance.GetType();

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

				var keyValues = new List<object>();

				foreach (var property in PartitionKeyProperties)
				{
					keyValues.Add(property.GetValue(unpackedObject));
				}

				PartitionKey.SetValue(packedObject, string.Join(PartitionKeySeparator, keyValues));

				var compressedBytes = MessagePackSerializer.Serialize(CompressedEntityType, compressedEntity);
				var compressedString = Convert.ToBase64String(compressedBytes);

				Attachment.SetValue(packedObject, compressedString);

				return packedObject;
			}

			public T Unpackage<T>(object packedObject) where T : new()
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

				// Reverse engineer PK string.
				var partitionKey = ((string)PartitionKey.GetValue(packedObject)).Split(new string[] { PartitionKeySeparator }, StringSplitOptions.None);

				for (var i = 0; i < PartitionKeyProperties.Count; i++)
				{
					PartitionKeyProperties[i].SetValue(unpackedObject, partitionKey[i]);
				}

				return unpackedObject;
			}
			#endregion Public Methods
		}
		#endregion Internal Serializer Class
	}
}