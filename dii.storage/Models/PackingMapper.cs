using System.Collections.Generic;
using System.Reflection;

namespace dii.storage.Models
{
	/// <summary>
	/// A mapping of object properties between a concrete and dynamically created object in the <see cref="Optimizer"/>.
	/// This class cannot be inherited.
	/// </summary>
	public class PackingMapper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackingMapper"/> class.
		/// </summary>
		public PackingMapper()
		{
			ConcreteProperties = new Dictionary<string, PropertyInfo>();
			EmitProperties = new Dictionary<string, PropertyInfo>();
		}

		/// <summary>
		/// A <see cref="Dictionary{TKey, TValue}"/> of concrete property names and their <see cref="PropertyInfo"/>.
		/// </summary>
		public Dictionary<string, PropertyInfo> ConcreteProperties { get; set; }

		/// <summary>
		/// A <see cref="Dictionary{TKey, TValue}"/> of emit property names and their <see cref="PropertyInfo"/>.
		/// </summary>
		public Dictionary<string, PropertyInfo> EmitProperties { get; set; }
	}
}