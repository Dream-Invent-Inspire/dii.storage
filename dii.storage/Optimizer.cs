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
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dii.storage
{

    /// <summary>
    /// The dii.storage engine that dynamically creates and registers all types used for storage.
    /// Only one instance of the <see cref="Optimizer"/> can exist at a time.
    /// This class cannot be inherited.
    /// </summary>
    public sealed partial class Optimizer
	{
		#region Private Fields
		private static Optimizer _instance;
		private static readonly object _instanceLock = new object();
		private static bool _autoDetectTypes;
		private static bool _ignoreInvalidDiiEntities;

		private readonly ModuleBuilder _builder;
		//private readonly Dictionary<Type, Serializer> _packing;
		//private readonly Dictionary<Type, Serializer> _unpacking;

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
						Serializer storageTypeSerializer;
						try
						{
							//Wiring to the new typeGenerator structure from 05/08/2023
							var typeGenerator = new TableTypeGenerator(type, _builder, SubPropertyMapping, _ignoreInvalidDiiEntities);
							storageTypeSerializer = typeGenerator.Generate();
						}
						catch
						{
							//Duplicate type initialization will yield this exception.
							//This trapping is preventing Init(typeof(SameType), typeof(SameType))
							if (_ignoreInvalidDiiEntities)
								continue;
							else
								throw;
						}

						if (storageTypeSerializer != null && storageTypeSerializer.StoredEntityType != null)
						{
                            OptimizedTypeRegistrar.Register(type, storageTypeSerializer);

                            var storageType = storageTypeSerializer.StoredEntityType;
                            if (storageType != null)
                            {
                                var tableMetaData = new TableMetaData
                                {
                                    TableName = type.GetCustomAttribute<StorageNameAttribute>()?.Name ?? type.Name,
                                    ClassName = type.Name,
                                    ConcreteType = type,
                                    StorageType = storageType,
                                    TimeToLiveInSeconds = type.GetCustomAttribute<EnableTimeToLiveAttribute>()?.TimeToLiveInSeconds,
                                };

                                Tables.Add(tableMetaData);
                                TableMappings.Add(type, tableMetaData);
                            }
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
			return OptimizedTypeRegistrar.IsMapped(type);
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
			return OptimizedTypeRegistrar.IsMapped(type);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				return OptimizedTypeRegistrar.GetPackageMapping(type)?.Package(obj);
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

			if (OptimizedTypeRegistrar.IsMapped(type) && obj is T)
			{
				return OptimizedTypeRegistrar.GetPackageMapping(type)?.Package(obj);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				var mapping = OptimizedTypeRegistrar.GetUnpackageMapping(type);
				if(mapping == null)
				{
					return default(T);
				}
                return mapping.Unpackage<T>(obj);
			}

			type = typeof(T);

			if (OptimizedTypeRegistrar.IsMapped(type) && obj is JObject j)
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				var obj = JsonSerializer.Deserialize(json, OptimizedTypeRegistrar.GetPackageMapping(type).StoredEntityType);
				return OptimizedTypeRegistrar.GetPackageMapping(type).Unpackage<T>(obj);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				var partitionKeyValues = new List<object>();

				foreach (var property in OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeyProperties)
				{
					partitionKeyValues.Add(property.GetValue(obj));
				}

				var partitionKeyString = string.Join(OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeySeparator, partitionKeyValues);

				if (OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeyType != typeof(string))
                {
					return (TKey)Activator.CreateInstance(OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeyType, partitionKeyString);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				var partitionKeyValues = new List<object>();

				foreach (var property in OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeyProperties)
				{
					partitionKeyValues.Add(property.GetValue(obj));
				}

				return string.Join(OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeySeparator, partitionKeyValues);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				var idValues = new List<object>();

				foreach (var property in OptimizedTypeRegistrar.GetPackageMapping(type).IdProperties)
				{
					idValues.Add(property.GetValue(obj));
				}

				return string.Join(OptimizedTypeRegistrar.GetPackageMapping(type).IdSeparator, idValues);
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

			if (OptimizedTypeRegistrar.IsMapped(type))
			{
				return OptimizedTypeRegistrar.GetPackageMapping(type).PartitionKeyType;
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
	}
}