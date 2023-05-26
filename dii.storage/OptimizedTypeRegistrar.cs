using System;
using System.Collections.Generic;
using static dii.storage.Optimizer;

namespace dii.storage
{
    public static class OptimizedTypeRegistrar
	{
        private static Dictionary<Type, Serializer> _packing = new Dictionary<Type, Serializer>();
        private static Dictionary<Type, Serializer> _unpacking = new Dictionary<Type, Serializer>();

		/// <summary>
		/// Retrieves the Package Mapping for the provided type.
		/// </summary>
		/// <param name="type">The type to be packaged.</param>
		/// <returns>The serializer to package the specified type.</returns>
		public static Serializer GetPackageMapping(Type type)
		{
			if(_packing.ContainsKey(type))
			{
				return _packing[type];
			}

			return null;
		}

		/// <summary>
		/// Retrieves the Package Mapping to Unpackage the specified type.
		/// </summary>
		/// <param name="type">The type to unpackage</param>
		/// <returns>The serializer to unpackage the specified type.</returns>
		public static Serializer GetUnpackageMapping(Type type)
		{
			if(_unpacking.ContainsKey(type))
			{
				return _unpacking[type];
			}

			return null;
		}

		/// <summary>
		/// Will evaluate if the type is mapped for either packing or unpacking.
		/// </summary>
		/// <param name="type">The type to verify is mapped</param>
		/// <returns>True if the type is registered for either packing or unpacking.</returns>
		public static bool IsMapped(Type type)
		{
			return _packing.ContainsKey(type) || _unpacking.ContainsKey(type);
		}

		/// <summary>
		/// Registers the specified type and it's optimization serializer
		/// </summary>
		/// <param name="type">The concrete type that has been optimized</param>
		/// <param name="serializer">The serializer that handles packaging and unpackaging</param>
		public static void Register(Type type, Serializer serializer)
		{
			try
			{
				if(!_packing.ContainsKey(type))
				{
                    _packing.Add(type, serializer);
				}
				else
				{
					//Take the last configured Serializer for multi-registration.
					_packing[type] = serializer;
				}
				
				if(!_unpacking.ContainsKey(serializer.StoredEntityType))
				{
                    _unpacking.Add(serializer.StoredEntityType, serializer);
                }else
				{
					_unpacking[serializer.StoredEntityType] = serializer;
				}
				
			}
			catch(Exception ex)
			{
				throw;
			}
        }

		/// <summary>
		/// This is a method intended for Testing purposes only.  It will clear all registered types.
		/// </summary>
		/// <param name="iUnderstandThisShouldOnlyBeUsedForTesting">
		/// We are serious, this was not intended for runtime type reprocessing.
		/// The engine is not optimized for recurring runtime type generation and
		/// registration outside of test scenarios.
		/// </param>
		public static void ClearAllTypes(bool iUnderstandThisShouldOnlyBeUsedForTesting)
		{
			if (iUnderstandThisShouldOnlyBeUsedForTesting)
			{
				_packing.Clear();
				_unpacking.Clear();
			}
		}
    }
}