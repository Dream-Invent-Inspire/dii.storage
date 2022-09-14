using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using MessagePack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dii.storage
{
	/// <summary>
	/// The dii.storage engine that dynamically creates and registers all types used for storage.
	/// Only one instance of the <see cref="Optimizer"/> can exist at a time.
	/// This class cannot be inherited.
	/// </summary>
	public sealed class Optimizer
	{
		#region Private Fields
		private static Optimizer _instance;
		private static readonly object _instanceLock = new object();
		private static bool _autoDetectTypes;
		private static bool _ignoreInvalidDiiEntities;

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
		/// <summary>
		/// A collection of <see cref="TableMetaData"/> initialized with this instance of the <see cref="Optimizer"/>.
		/// </summary>
		public readonly List<TableMetaData> Tables;

		/// <summary>
		/// A <see cref="Dictionary{TKey, TValue}"/> of concrete <see cref="Type"/> mapped to their <see cref="TableMetaData"/>
		/// record initialized with this instance of the <see cref="Optimizer"/>.
		/// <para>
		/// All types within this collection implement the <see cref="IDiiEntity"/> interface.
		/// </para>
		/// </summary>
		public readonly Dictionary<Type, TableMetaData> TableMappings;

		/// <summary>
		/// A <see cref="Dictionary{TKey, TValue}"/> of concrete <see cref="Type"/> mapped to their <see cref="TableMetaData"/>
		/// record initialized with this instance of the <see cref="Optimizer"/>.
		/// <para>
		/// All types within this collection do not implement the <see cref="IDiiEntity"/> interface. They are
		/// user-defined objects that are typically found as properties within the objects found in <see cref="Tables"/>.
		/// </para>
		/// </summary>
		public readonly Dictionary<Type, Type> SubPropertyMapping;
		#endregion Public Fields

		#region Constructors
		/// <summary>
		/// Initializes an instance of <see cref="Optimizer"/>.
		/// </summary>
		/// <param name="types">An array of <see cref="Type"/> to register. All types must implement the <see cref="IDiiEntity"/> interface.</param>
		private Optimizer(params Type[] types)
		{
			_packing = new Dictionary<Type, Serializer>();
			_unpacking = new Dictionary<Type, Serializer>();

			Tables = new List<TableMetaData>();
			TableMappings = new Dictionary<Type, TableMetaData>();
			SubPropertyMapping = new Dictionary<Type, Type>();

			var assemblyName = new AssemblyName($"{_dynamicAssemblyName}.{Guid.NewGuid()}");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

			_builder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

			ConfigureTypes(types);
		}
		#endregion Constructors

		#region Public Methods
		/// <summary>
		/// Creates a singleton of the <see cref="Optimizer"/>.
		/// </summary>
		/// <param name="types">An array of <see cref="Type"/> to register. All types must implement the <see cref="IDiiEntity"/> interface.</param>
		/// <returns>
		/// The instance of <see cref="Optimizer"/>.
		/// </returns>
		public static Optimizer Init(params Type[] types)
		{
			return InitializeOptimizer(types);
		}

		/// <summary>
		/// Creates a singleton of the <see cref="Optimizer"/>.
		/// </summary>
		/// <param name="ignoreInvalidDiiEntities">When <see langword="true" />, invalid <see cref="Type"/> will be ignored and not throw an exception.</param>
		/// <param name="types">An array of <see cref="Type"/> to register. All types must implement the <see cref="IDiiEntity"/> interface.</param>
		/// <returns>
		/// The instance of <see cref="Optimizer"/>.
		/// </returns>
		public static Optimizer Init(bool ignoreInvalidDiiEntities = false, params Type[] types)
		{
			_ignoreInvalidDiiEntities = ignoreInvalidDiiEntities;

			return InitializeOptimizer(types);
		}

		/// <summary>
		/// Creates a singleton of the <see cref="Optimizer"/>.
		/// </summary>
		/// <param name="autoDetectTypes">
		/// When <see langword="true" />, the <see cref="Optimizer"/> will search all assemblies for any objects
		/// that implement the <see cref="IDiiEntity"/> interface and attempt to register them.
		/// </param>
		/// <param name="ignoreInvalidDiiEntities">When <see langword="true" />, invalid <see cref="Type"/> will be ignored and not throw an exception.</param>
		/// <returns>
		/// The instance of <see cref="Optimizer"/>.
		/// </returns>
		public static Optimizer Init(bool autoDetectTypes = false, bool ignoreInvalidDiiEntities = false)
		{
			_autoDetectTypes = autoDetectTypes;
			_ignoreInvalidDiiEntities = ignoreInvalidDiiEntities;

			return InitializeOptimizer();
		}

		/// <summary>
		/// Returns the singleton of the <see cref="Optimizer"/>.
		/// </summary>
		/// <exception cref="DiiNotInitializedException">
		/// The <see cref="Optimizer"/> has not been initialized yet.
		/// </exception>
		/// <returns>
		/// The instance of <see cref="Optimizer"/> or throws an exception if it does not exist.
		/// </returns>
		public static Optimizer Get()
		{
			if (_instance == null)
			{
				throw new DiiNotInitializedException(nameof(Optimizer));
			}

			return _instance;
		}

		/// <summary>
		/// Attempts to register an array of <see cref="Type"/> to the <see cref="Optimizer"/>.
		/// Any <see cref="Type"/> that is already registered will be ignored.
		/// </summary>
		/// <param name="types">An array of <see cref="Type"/> to register. All types must implement the <see cref="IDiiEntity"/> interface.</param>
		/// <remarks>
		/// When <see cref="Init(bool, bool)"/> has been used with autoDetectTypes = <see langword="true" />,
		/// the <see cref="Optimizer"/> will search all assemblies for any objects that implement the 
		/// <see cref="IDiiEntity"/> interface and attempt to register them.
		/// </remarks>
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

		/// <summary>
		/// Identified whether a <see cref="Type"/> is already registered with the <see cref="Optimizer"/>
		/// as a concrete type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to check.</param>
		/// <returns>
		/// Returns <see langword="true" /> when the <see cref="Type"/> is registered with the instance
		/// of <see cref="Optimizer"/> as a concrete type.
		/// </returns>
		public bool IsKnownConcreteType(Type type)
		{
			return _packing.ContainsKey(type);
		}

		/// <summary>
		/// Identified whether a <see cref="Type"/> is already registered with the <see cref="Optimizer"/>
		/// as an emit type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to check.</param>
		/// <returns>
		/// Returns <see langword="true" /> when the <see cref="Type"/> is registered with the instance
		/// of <see cref="Optimizer"/> as an emit type.
		/// </returns>
		public bool IsKnownEmitType(Type type)
		{
			return _unpacking.ContainsKey(type);
		}

		/// <summary>
		/// Parses an entity of type <typeparamref name="T"/> to a packaged object.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to package.</typeparam>
		/// <param name="obj">The object to be packaged.</param>
		/// <exception cref="ArgumentNullException">
		/// The entity to package is null.
		/// </exception>
		/// <returns>
		/// The packaged object.
		/// </returns>
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

			return default(object);
		}

		/// <summary>
		/// Parses an entity of type <see cref="object"/> to a packaged object.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to package.</typeparam>
		/// <param name="obj">The object to be packaged.</param>
		/// <exception cref="ArgumentNullException">
		/// The entity to package is null.
		/// </exception>
		/// <returns>
		/// The packaged object.
		/// </returns>
		public object ToEntityObject<T>(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var type = typeof(T);

			if (_packing.ContainsKey(type) && obj is T)
			{
				return _packing[type].Package(obj);
			}

			return default(object);
		}

		/// <summary>
		/// Parses an entity of type <see cref="object"/> to an unpackaged <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to unpackage the object to.</typeparam>
		/// <param name="obj">The object to be unpackaged.</param>
		/// <returns>
		/// The unpackaged entity.
		/// </returns>
		public T FromEntity<T>(object obj) where T : new()
		{
			if (obj == null)
			{
				return default(T);
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

		/// <summary>
		/// Parses an entity of type <typeparamref name="T"/> to a json string.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to package.</typeparam>
		/// <param name="obj">The object to be packaged.</param>
		/// <exception cref="ArgumentNullException">
		/// The object to parse to a json string is null.
		/// </exception>
		/// <returns>
		/// The packaged object as a json string.
		/// </returns>
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

			return default(string);
		}

		/// <summary>
		/// Parses a json string to an entity of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to unpackage the object to.</typeparam>
		/// <param name="json">The json string to be unpackaged.</param>
		/// <exception cref="ArgumentNullException">
		/// The json string to unpackage is null, empty or whitespace.
		/// </exception>
		/// <returns>
		/// The unpackaged entity.
		/// </returns>
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

		/// <summary>
		/// Returns the partition key for the entity cast as the type provided.
		/// This should match the <see cref="Type"/> provided to the <see cref="PartitionKeyAttribute.PartitionKeyType"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the entity.</typeparam>
		/// <typeparam name="TKey">The <see cref="Type"/> to cast the partition key as.</typeparam>
		/// <param name="obj">The entity to extract the partition key from.</param>
		/// <exception cref="ArgumentNullException">
		/// The entity to extract the partition key from is null.
		/// </exception>
		/// <returns>
		/// The parition key cast as type <typeparamref name="TKey"/>.
		/// </returns>
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

		/// <summary>
		/// Returns the partition key for the entity as a <see cref="string"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the entity.</typeparam>
		/// <param name="obj">The entity to extract the partition key from.</param>
		/// <exception cref="ArgumentNullException">
		/// The entity to extract the partition key from is null.
		/// </exception>
		/// <returns>
		/// The parition key as a <see cref="string"/>.
		/// </returns>
		public string GetPartitionKey<T>(T obj)
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

				return string.Join(_packing[type].PartitionKeySeparator, partitionKeyValues);
			}

			return default(string);
		}

		/// <summary>
		/// Returns the id for the entity as a <see cref="string"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the entity.</typeparam>
		/// <param name="obj">The entity to extract the id from.</param>
		/// <exception cref="ArgumentNullException">
		/// The entity to extract the id from is null.
		/// </exception>
		/// <returns>
		/// The id as a <see cref="string"/>.
		/// </returns>
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

			return default(string);
		}

		/// <summary>
		/// Returns the partition key type.
		/// This should match the <see cref="Type"/> provided to the <see cref="PartitionKeyAttribute.PartitionKeyType"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the entity.</typeparam>
		/// <returns>
		/// The <see cref="Type"/> of the partition key, as provided to the <see cref="PartitionKeyAttribute.PartitionKeyType"/>.
		/// </returns>
		public Type GetPartitionKeyType<T>()
		{
			var type = typeof(T);

			if (_packing.ContainsKey(type))
			{
				return _packing[type].PartitionKeyType;
			}

			return default(Type);
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

		private Type GenerateType(Type source, bool isSubEntity = false)
		{
			var jsonMap = new PackingMapper();
			var compressMap = new PackingMapper();

			var typeBuilder = _builder.DefineType(source.Name, TypeAttributes.Public);
			var typeConst = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

			var compressBuilder = _builder.DefineType($"{source.Name}Compressed", TypeAttributes.Public);
			var compressConst = compressBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
			var msgPkAttrConst = typeof(MessagePackObjectAttribute).GetConstructor(new Type[] { typeof(bool) });
			compressBuilder.SetCustomAttribute(new CustomAttributeBuilder(msgPkAttrConst, new object[] { false }));

			var partitionSeparator = Constants.DefaultPartitionKeyDelimitor;
			var partitionFields = new Dictionary<int, PropertyInfo>();
			var partitionKeyType = typeof(string);

			var idSeparator = Constants.DefaultIdDelimitor;
			var idFields = new Dictionary<int, PropertyInfo>();

			var jsonAttrConstructor = typeof(JsonPropertyNameAttribute).GetConstructor(new Type[] { typeof(string) });
			var compressAttrConstructor = typeof(KeyAttribute).GetConstructor(new Type[] { typeof(int) });

			foreach (var property in source.GetProperties())
			{
				var isAlsoId = false;

				var partitionKey = property.GetCustomAttribute<PartitionKeyAttribute>();
                var id = property.GetCustomAttribute<IdAttribute>();

                if (partitionKey != null)
				{
					isAlsoId = id != null;

                    if (partitionFields.ContainsKey(partitionKey.Order))
					{
						if (_ignoreInvalidDiiEntities)
						{
							// Do not create the entity to be used, but do not throw an exception.
							return null;
						}
						else
						{
							// Throw an exception when an invalid type is attempted.
							throw new DiiPartitionKeyDuplicateOrderException(partitionFields[partitionKey.Order].Name, property.Name, partitionKey.Order);
						}
					}

					partitionFields.Add(partitionKey.Order, property);

					// First PartitionKeyAttribute to change the separator wins.
					if (!char.IsWhiteSpace(partitionKey.Separator) && partitionSeparator == Constants.DefaultPartitionKeyDelimitor)
                    {
						partitionSeparator = partitionKey.Separator;
					}

					if (partitionKey.PartitionKeyType != null && partitionKey.PartitionKeyType != typeof(string))
                    {
						partitionKeyType = partitionKey.PartitionKeyType;
					}
					
					if (!isAlsoId)
						continue;
				}

				if (id != null)
                {
                    isAlsoId = false;

                    if (idFields.ContainsKey(id.Order))
					{
						if (_ignoreInvalidDiiEntities)
						{
							// Do not create the entity to be used, but do not throw an exception.
							return null;
						}
						else
						{
							// Throw an exception when an invalid type is attempted.
							throw new DiiIdDuplicateOrderException(idFields[id.Order].Name, property.Name, id.Order);
						}
					}

					idFields.Add(id.Order, property);

					// First IdAttribute to change the separator wins.
					if (!char.IsWhiteSpace(id.Separator) && idSeparator == Constants.DefaultIdDelimitor)
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
						if (_ignoreInvalidDiiEntities)
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
					var childProps = property.PropertyType.GetProperties();
					if (childProps.Any(x => x.GetCustomAttribute<SearchableAttribute>() != null) || childProps.Any(x => x.GetCustomAttribute<CompressAttribute>() != null))
					{
						if (property.PropertyType.GetInterface(nameof(IDiiEntity)) != null)
						{
							if (_ignoreInvalidDiiEntities)
							{
								// Do not create the entity to be used, but do not throw an exception.
								return null;
							}
							else
							{
								// Throw an exception when an invalid type is attempted.
								throw new DiiInvalidNestingException(source.Name);
							}
						}

						if (!SubPropertyMapping.ContainsKey(property.PropertyType))
						{
							SubPropertyMapping.Add(property.PropertyType, null);
							var storageType = GenerateType(property.PropertyType, true);
							SubPropertyMapping[property.PropertyType] = storageType;
							_ = AddProperty(typeBuilder, search.Abbreviation, SubPropertyMapping[property.PropertyType], jsonAttr);
						}
						else
						{
							// If the subentity has already been registered and appears again within the same object.
							if (SubPropertyMapping[property.PropertyType] != null)
                            {
								_ = AddProperty(typeBuilder, search.Abbreviation, SubPropertyMapping[property.PropertyType], jsonAttr);
							}
							else
							{
								if (typeBuilder.Name == property.PropertyType.Name)
                                {
									// If the object has a self-reference property, pass itself in this way.
									var selfReferencingPropertyType = _builder.GetType(typeBuilder.Name);
									_ = AddProperty(typeBuilder, search.Abbreviation, selfReferencingPropertyType, jsonAttr);
								}
							}
						}
					}
					else
					{
						_ = AddProperty(typeBuilder, search.Abbreviation, property.PropertyType, jsonAttr);
					}

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
			if (!isSubEntity)
			{
				_ = AddProperty(typeBuilder, Constants.ReservedPartitionKeyKey, typeof(string));
				_ = AddProperty(typeBuilder, Constants.ReservedIdKey, typeof(string));
			}

			Type storageEntityType = typeBuilder.CreateTypeInfo();
			Type compressedEntityType = compressBuilder.CreateTypeInfo();
			PropertyInfo attachments;
			PropertyInfo partitionKeyInfo = null;
			PropertyInfo idInfo = null;

			{
				var instance = Activator.CreateInstance(storageEntityType);
				storageEntityType = instance.GetType();

				var props = storageEntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(x => x.Name, x => x);
				attachments = props[Constants.ReservedCompressedKey];
				props.Remove(Constants.ReservedCompressedKey);

				if (!isSubEntity)
				{
					partitionKeyInfo = props[Constants.ReservedPartitionKeyKey];
					props.Remove(Constants.ReservedPartitionKeyKey);

					idInfo = props[Constants.ReservedIdKey];
					props.Remove(Constants.ReservedIdKey);
				}

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
					.Select(x => x.Value)
					.ToList(),
				PartitionKeySeparator = partitionSeparator.ToString(),
				PartitionKeyType = partitionKeyType,
				Id = idInfo,
				IdProperties = idFields
					.OrderBy(x => x.Key)
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

		private static PropertyBuilder AddProperty(TypeBuilder typeBuilder, string name, Type propertyType, params CustomAttributeBuilder[] customAttributeBuilders)
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
		/// <summary>
		/// 
		/// This class cannot be inherited.
		/// </summary>
		internal sealed class Serializer
		{
			#region Private Fields
			private readonly MethodInfo _ptrUnpackage;
			private readonly MethodInfo _ptrPackage;
			#endregion Private Fields

			#region Public Properties
			/// <summary>
			/// The <see cref="PropertyInfo"/> for the property designated by the <see cref="PartitionKeyAttribute"/>.
			/// </summary>
			public PropertyInfo PartitionKey { get; set; }

			/// <summary>
			/// The <see cref="PropertyInfo"/> for all properties designated by the <see cref="PartitionKeyAttribute"/>.
			/// </summary>
			public List<PropertyInfo> PartitionKeyProperties { get; set; }

			/// <summary>
			/// The character used to delimiter multiple partition key values as set in the <see cref="PartitionKeyAttribute.Separator"/>.
			/// </summary>
			public string PartitionKeySeparator { get; set; }

			/// <summary>
			/// The <see cref="Type"/> of the parition key as set in the <see cref="PartitionKeyAttribute.PartitionKeyType"/>.
			/// </summary>
			public Type PartitionKeyType { get; set; }

			/// <summary>
			/// The <see cref="PropertyInfo"/> for the property designated by the <see cref="IdAttribute"/>.
			/// </summary>
			public PropertyInfo Id { get; set; }

			/// <summary>
			/// The <see cref="PropertyInfo"/> for all properties designated by the <see cref="IdAttribute"/>.
			/// </summary>
			public List<PropertyInfo> IdProperties { get; set; }

			/// <summary>
			/// The character used to delimiter multiple id values as set in the <see cref="IdAttribute.Separator"/>.
			/// </summary>
			public string IdSeparator { get; set; }

			/// <summary>
			/// The <see cref="PropertyInfo"/> for the compressed object property.
			/// </summary>
			public PropertyInfo Attachment { get; set; }

			/// <summary>
			/// The mapping of concrete types to their json properties.
			/// </summary>
			public PackingMapper StoredEntityMapping { get; set; }

			/// <summary>
			/// The mapping of concrete types to their compressed properties. 
			/// </summary>
			public PackingMapper CompressedEntityMapping { get; set; }

			/// <summary>
			/// The dynamically created <see cref="Type"/> to represent the searchable object.
			/// </summary>
			public Type StoredEntityType { get; set; }

			/// <summary>
			/// The dynamically created <see cref="Type"/> to represent the compressed object.
			/// </summary>
			public Type CompressedEntityType { get; set; }
			#endregion Public Properties

			#region Constructors
			public Serializer() 
			{
				var t = typeof(Optimizer);

				_ptrUnpackage = t.GetMethod("FromEntity");
				_ptrPackage = t.GetMethod("ToEntityObject");
			}
			#endregion Constructors

			#region Public Methods

			/// <summary>
			/// Packages an object with compression.
			/// </summary>
			/// <param name="unpackedObject">The object to be packaged.</param>
			/// <exception cref="ArgumentNullException">
			/// The object to package is null.
			/// </exception>
			/// <returns>
			/// The packaged object.
			/// </returns>
			public object Package(object unpackedObject)
			{
				if (unpackedObject == null)
				{
					throw new ArgumentNullException(nameof(unpackedObject));
				}

				var packedObject = Activator.CreateInstance(StoredEntityType);
				var compressedEntity = Activator.CreateInstance(CompressedEntityType);

				if (PartitionKey != null)
				{
					var partitionKeyValues = new List<object>();

					foreach (var property in PartitionKeyProperties)
					{
						var partitionKeyValue = property.GetValue(unpackedObject);

						if (property.PropertyType == typeof(string))
						{
							if (!string.IsNullOrWhiteSpace(partitionKeyValue as string))
							{
								partitionKeyValues.Add(partitionKeyValue);
							}
						}
						else
                        {
							if (partitionKeyValue != null)
							{
								partitionKeyValues.Add(partitionKeyValue);
							}
						}
					}

					PartitionKey.SetValue(packedObject, string.Join(PartitionKeySeparator, partitionKeyValues));
				}

				if (Id != null)
				{
					var idValues = new List<object>();

					foreach (var property in IdProperties)
					{
						var idValue = property.GetValue(unpackedObject);

						if (property.PropertyType == typeof(string))
						{
							if (!string.IsNullOrWhiteSpace(idValue as string))
							{
								idValues.Add(idValue);
							}
						}
						else
						{
							if (idValue != null)
							{
								idValues.Add(idValue);
							}
						}
					}

					Id.SetValue(packedObject, string.Join(IdSeparator, idValues));
				}

				foreach (var property in StoredEntityMapping.ConcreteProperties)
				{
					var val = property.Value.GetValue(unpackedObject);
					if (val != null && Get().SubPropertyMapping.ContainsKey(val.GetType()))
					{
						try
						{
							var method = _ptrPackage.MakeGenericMethod(val.GetType());
							var complexSubType = method.Invoke(Get(), new object[] { val });
							StoredEntityMapping.EmitProperties[property.Key].SetValue(packedObject, complexSubType);
						}
						catch (Exception)
						{
							throw;
						}
					}
					else if(StoredEntityMapping.EmitProperties.ContainsKey(property.Key))
					{
						StoredEntityMapping.EmitProperties[property.Key].SetValue(packedObject, val);
					}
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

			/// <summary>
			/// Unpackages an object to an unpackaged as <typeparamref name="T"/>.
			/// </summary>
			/// <typeparam name="T">The <see cref="Type"/> to unpackage the object to.</typeparam>
			/// <param name="packedObject">The object to unpack.</param>
			/// <exception cref="ArgumentNullException">
			/// The object to unpackage is null.
			/// </exception>
			/// <exception cref="ArgumentException">
			/// The packaged object contained no properties.
			/// </exception>
			/// <returns>
			/// The unpackaged entity.
			/// </returns>
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

				if (PartitionKey != null)
				{
					// Reverse engineer parition key string.
					var partitionKey = ((string)PartitionKey.GetValue(packedObject)).Split(new string[] { PartitionKeySeparator }, StringSplitOptions.None);

					for (var i = 0; i < PartitionKeyProperties.Count; i++)
					{
						PartitionKeyProperties[i].SetValue(unpackedObject, partitionKey[i]);
					}
				}

				if (Id != null)
				{
					// Reverse engineer id string.
					var id = ((string)Id.GetValue(packedObject)).Split(new string[] { IdSeparator }, StringSplitOptions.None);

					for (var i = 0; i < IdProperties.Count; i++)
					{
						IdProperties[i].SetValue(unpackedObject, id[i]);
					}
				}

				foreach (var property in CompressedEntityMapping.EmitProperties)
				{
					var val = property.Value.GetValue(compressedObj);
					CompressedEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, val);
				}

				foreach (var property in StoredEntityMapping.EmitProperties)
				{
					var val = property.Value.GetValue(packedObject);
					if (Get().SubPropertyMapping.ContainsKey(StoredEntityMapping.ConcreteProperties[property.Key].PropertyType))
					{
						try
						{
							var mthd = _ptrUnpackage.MakeGenericMethod(StoredEntityMapping.ConcreteProperties[property.Key].PropertyType);
							var complexSubType = mthd.Invoke(Get(), new object[] { val });
							StoredEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, complexSubType);
						}
						catch (Exception)
						{
							throw;
						}
					}
					else
					{
						StoredEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, val);
					}
				}

				return unpackedObject;
			}
			#endregion Public Methods
		}
		#endregion Internal Serializer Class
	}
}