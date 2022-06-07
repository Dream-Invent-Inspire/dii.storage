using System.Collections.Generic;
using System.Reflection;

namespace dii.cosmos.Models
{
	public class PackingMapper
	{
		public PackingMapper()
		{
			ConcreteProperties = new Dictionary<string, PropertyInfo>();
			EmitProperties = new Dictionary<string, PropertyInfo>();
		}

		public Dictionary<string, PropertyInfo> ConcreteProperties { get; set; }
		public Dictionary<string, PropertyInfo> EmitProperties { get; set; }
	}
}