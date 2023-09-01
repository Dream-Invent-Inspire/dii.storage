using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using dii.storage.Utilities;
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
using System.Threading.Channels;
using System.Threading;
using System.Reflection.PortableExecutable;

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

        private readonly string LookupTableSuffix = "Lookup";

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
        private Optimizer(string dbid, params Type[] types)
		{
			Tables = new List<TableMetaData>();
			TableMappings = new Dictionary<Type, TableMetaData>();
			SubPropertyMapping = new Dictionary<Type, Type>();

			var assemblyName = new AssemblyName($"{_dynamicAssemblyName}.{Guid.NewGuid()}");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

			_builder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

			ConfigureTypes(dbid, types);
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
		public static Optimizer Init(string dbid, params Type[] types)
		{
			return InitializeOptimizer(dbid, types);
		}

		/// <summary>
		/// Creates a singleton of the <see cref="Optimizer"/>.
		/// </summary>
		/// <param name="ignoreInvalidDiiEntities">When <see langword="true" />, invalid <see cref="Type"/> will be ignored and not throw an exception.</param>
		/// <param name="types">An array of <see cref="Type"/> to register. All types must implement the <see cref="IDiiEntity"/> interface.</param>
		/// <returns>
		/// The instance of <see cref="Optimizer"/>.
		/// </returns>
		public static Optimizer Init(string dbid, bool ignoreInvalidDiiEntities = false, params Type[] types)
		{
			_ignoreInvalidDiiEntities = ignoreInvalidDiiEntities;

			return InitializeOptimizer(dbid, types);
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
		public static Optimizer Init(string dbid, bool autoDetectTypes = false, bool ignoreInvalidDiiEntities = false)
		{
			_autoDetectTypes = autoDetectTypes;
			_ignoreInvalidDiiEntities = ignoreInvalidDiiEntities;

			return InitializeOptimizer(dbid);
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

        public static void Clear()
        {
            _instance = null;
        }

        
		public void ConfigureTypes(Dictionary<string, List<Type>> types)
		{

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
		public void ConfigureTypes(string dbid, params Type[] types)
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
						Serializer storageTypeSerializer = RegisterType(type);
						if (storageTypeSerializer != null && storageTypeSerializer.StoredEntityType != null)
						{
							var tableMetaData = new TableMetaData
							{
								DbId = dbid,
								TableName = type.GetCustomAttribute<StorageNameAttribute>()?.Name ?? type.Name,
								ClassName = type.Name,
								ConcreteType = type,
								StorageType = storageTypeSerializer.StoredEntityType,
								TimeToLiveInSeconds = type.GetCustomAttribute<EnableTimeToLiveAttribute>()?.TimeToLiveInSeconds,
							};
							if (storageTypeSerializer.HierarchicalPartitionKeyProperties != null && storageTypeSerializer.HierarchicalPartitionKeyProperties.Any())
							{
								tableMetaData.HierarchicalPartitionKeys = storageTypeSerializer.HierarchicalPartitionKeyProperties.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
							}

							//Handles Lookup containers
							if (storageTypeSerializer.LookupHpkProperties != null && storageTypeSerializer.LookupHpkProperties.Any())
							{
								tableMetaData.LookupHpks = storageTypeSerializer.LookupHpkProperties.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
							}
							if (storageTypeSerializer.LookupIdProperties != null && storageTypeSerializer.LookupIdProperties.Any())
							{
								tableMetaData.LookupIds = storageTypeSerializer.LookupIdProperties.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
							}
							if (storageTypeSerializer.SearchableProperties != null && storageTypeSerializer.SearchableProperties.Any())
							{
								tableMetaData.SearchableFields = storageTypeSerializer.SearchableProperties;
							}
                            if (storageTypeSerializer.IdProperties != null && storageTypeSerializer.IdProperties.Any())
                            {
                                tableMetaData.IdProperties = storageTypeSerializer.IdProperties
									.Select((property, index) => new { Index = index, Property = property })
									.ToDictionary(item => item.Index, item => item.Property);
								tableMetaData.IdSeparator = storageTypeSerializer.IdSeparator;
                            }

                            Tables.Add(tableMetaData);
							TableMappings.Add(type, tableMetaData);

							if (tableMetaData.LookupIds?.Any() ?? false) //Note: We really only care if there are Lookup Ids, odds are in those cases there should also be HPKs
							{
								//Create the dynamic Lookup table and register it
								string tblName = type.GetCustomAttribute<StorageNameAttribute>()?.Name ?? type.Name;
								var lookupType = RegisterLookupType(
                                    //                           tableMetaData.LookupHpks,
                                    //tableMetaData.LookupIds,
                                    //tableMetaData.SearchableFields?.ToList(),
                                    sourceTableMetaData: tableMetaData, // Source table metadata
                                    lookupTableMetaData: new TableMetaData
									{
										DbId = dbid,
										TimeToLiveInSeconds = type.GetCustomAttribute<EnableTimeToLiveAttribute>()?.TimeToLiveInSeconds,
										TableName = $"{tblName}{LookupTableSuffix}",
										ClassName = $"{tableMetaData.ClassName}{LookupTableSuffix}",
                                        //this is used for the change feed wire up
                                        SourceTableNameForLookup = tableMetaData.TableName, //this is THIS entity's table name (not the lookup table)
										SourceTableTypeForLookup = type, //this is THIS entity's type (not the lookup type) for the Lookup object to cross reference
										IsLookupTable = true
									});

								tableMetaData.LookupType = lookupType; //this is for the lookup object unpacking
							}
						}
					}
				}
			}
		}

		public object HydrateEntityByType(Type type, string json)
		{
            if (type == null)
			{
                throw new ArgumentNullException(nameof(type));
            }

            if (!IsKnownConcreteType(type))
			{
                throw new Exception($"The type {type.FullName} is not registered with the Optimizer. Please register the type before attempting to construct an instance.");
            }

            var method = this.GetType().GetMethod("UnpackageFromJson").MakeGenericMethod(type);
            var sourceObj = method.Invoke(this, new object[] { json });
			return sourceObj;
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
			var props = OptimizedTypeRegistrar.GetPackageMapping(type)?.PartitionKeyProperties;

            if (OptimizedTypeRegistrar.IsMapped(type) && props != null && props.Any())
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

		/// <summary>
		/// Return corresponding Lookup Table
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public TableMetaData GetLookupTableMetaData<T>(string group = null)
		{
			return Tables.Where(x => x.IsLookupTable && x.SourceTableTypeForLookup == typeof(T)).FirstOrDefault();
		}

		#endregion Public Methods

		#region Private Methods
		private static Optimizer InitializeOptimizer(string dbid, params Type[] types)
		{
			bool isNew = false;

			if (_instance == null)
			{
				lock (_instanceLock)
				{
					if (_instance == null)
					{
						isNew = true;
						_instance = new Optimizer(dbid, types);
					}
				}
			}

			if (!isNew)
			{
				_instance.ConfigureTypes(dbid, types);
			}

			return _instance;
		}

        private Serializer RegisterType(Type type)
        {
            Serializer storageTypeSerializer = null;
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
                    return null;
                else
                    throw;
            }

            if (storageTypeSerializer != null && storageTypeSerializer.StoredEntityType != null)
            {
                OptimizedTypeRegistrar.Register(type, storageTypeSerializer);
            }
            return storageTypeSerializer;
        }

        private Type RegisterLookupType(TableMetaData sourceTableMetaData, TableMetaData lookupTableMetaData)
        {
            var otherFields = new List<PropertyInfo>();
            otherFields.AddRange(sourceTableMetaData.HierarchicalPartitionKeys.Values);
            otherFields.AddRange(sourceTableMetaData.IdProperties.Values);

            //Generate the dynamic type for the Lookup entity
            Type dynamicType = DynamicTypeCreator.CreateLookupType(sourceTableMetaData.LookupHpks, sourceTableMetaData.LookupIds, otherFields, lookupTableMetaData);
            var lookupTypeSerializer = RegisterType(dynamicType);

            //Add to the lookup table
            lookupTableMetaData.ConcreteType = dynamicType;
            lookupTableMetaData.StorageType = lookupTypeSerializer.StoredEntityType;
            lookupTableMetaData.HierarchicalPartitionKeys = sourceTableMetaData.LookupHpks.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);

            //ToDo: not sure about this...does this need to be set on the lookup table?
            lookupTableMetaData.LookupIds = sourceTableMetaData.LookupIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);

            Tables.Add(lookupTableMetaData);
            TableMappings.Add(dynamicType, lookupTableMetaData);
            return dynamicType;
        }


        #endregion Private Methods
    }
}