using dii.storage.Attributes;
using dii.storage.Models;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace dii.storage
{
    public sealed partial class Optimizer
	{

        #region Internal Serializer Class
        /// <summary>
        /// 
        /// This class cannot be inherited.
        /// </summary>
        public class Serializer
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
            /// The <see cref="PropertyInfo"/> for all properties designated by the <see cref="PartitionKeyAttribute"/>.
            /// </summary>
            public Dictionary<int, PropertyInfo> HierarchicalPartitionKeyProperties { get; set; }


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

				if (HierarchicalPartitionKeyProperties != null && HierarchicalPartitionKeyProperties.Count > 0)
				{
					// just add these to the Concrete types to be handled like any other property
					foreach (var property in HierarchicalPartitionKeyProperties.Values)
					{
						if (!StoredEntityMapping.ConcreteProperties.ContainsKey(property.Name))
                            StoredEntityMapping.ConcreteProperties.Add(property.Name, property);
                    }
                }
                else if (PartitionKey != null)
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
					if(val == null) { continue; }
					var valType = val.GetType();
					if(val != null && valType.IsInstanceOfType(typeof(IList<>)))
					{
						var tmp = new List<object>();
						foreach(var e in (IEnumerable)val)
						{
							tmp.Add(Package(e));
						}

						var genericType = valType.GetGenericArguments().First();
                        var collectionType = typeof(List<>).MakeGenericType(genericType);
						var collection = Activator.CreateInstance(collectionType, tmp);

                        StoredEntityMapping.EmitProperties[property.Key].SetValue(packedObject, collection);
                    }
					else if (val != null && Get().SubPropertyMapping.ContainsKey(valType))
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

                if (HierarchicalPartitionKeyProperties != null && HierarchicalPartitionKeyProperties.Count > 0)
                {
					// These are real properties so do nothing
                }
                else if (PartitionKey != null)
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
                    if (val == null) { continue; }
                    var valType = val.GetType();
                    if (val != null && valType.IsInstanceOfType(typeof(IList<>)))
                    {
                        var tmp = new List<object>();
                        var genericType = valType.GetGenericArguments().First();
                        foreach (var e in (IEnumerable)val)
                        {
                            var mthd = _ptrUnpackage.MakeGenericMethod(genericType);
                            var complexSubType = mthd.Invoke(Get(), new object[] { val });
                            tmp.Add(complexSubType);
                        }

                        var collectionType = typeof(List<>).MakeGenericType(genericType);
                        var collection = Activator.CreateInstance(collectionType, tmp);

                        StoredEntityMapping.ConcreteProperties[property.Key].SetValue(unpackedObject, collection);
                    }
                    else if (Get().SubPropertyMapping.ContainsKey(StoredEntityMapping.ConcreteProperties[property.Key].PropertyType))
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