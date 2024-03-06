using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static dii.storage.Optimizer;

namespace dii.storage
{
    public class TableTypeGenerator : TypeGenerator
    {
        public TableTypeGenerator(Type source, ModuleBuilder builder, Dictionary<Type,Type> subPropertyMapping, bool suppressConfigurationErrors = false) : base(source, builder, subPropertyMapping, suppressConfigurationErrors)
        { }

        protected Dictionary<int, PropertyInfo> partitionFields { get; set; } = new Dictionary<int, PropertyInfo>();
        protected Dictionary<int, PropertyInfo> hierarchicalPartitionFields { get; set; } = new Dictionary<int, PropertyInfo>();

        protected Dictionary<string, Dictionary<int, PropertyInfo>> LookupHpkFields { get; set; } = new Dictionary<string, Dictionary<int, PropertyInfo>>();

        protected Dictionary<int, PropertyInfo> idFields { get; set; } = new Dictionary<int, PropertyInfo>();

        protected Dictionary<string, Dictionary<int, PropertyInfo>> LookupIdFields { get; set; } = new Dictionary<string, Dictionary<int, PropertyInfo>>();

        protected List<PropertyInfo> SearchableFields { get; set; } = new List<PropertyInfo>();

        protected PropertyInfo ChangeTrackerField { get; set; }

        /// <summary>
        /// The delimiter to separate aggregate partiton keys.
        /// </summary>
        protected char partitionSeparator { get; set; } = Constants.DefaultPartitionKeyDelimitor;

        /// <summary>
        /// The <see cref="Type"/> of the partition key.
        /// </summary>
        protected Type partitionKeyType { get; set; } = Constants.StringType;

        /// <summary>
        /// The delimiter to separate aggregate id keys.
        /// </summary>
        protected char idSeparator { get; set; } = Constants.DefaultIdDelimitor;

        /// <summary>
        /// Intentionally a no-op.  Table type mappings are intended to be handled within the optimizer.
        /// </summary>
        /// <param name="serializer"></param>
        protected override void MapTypes(Serializer serializer)
        {
            //Do not call the base intentionally.  Table type mapping is handled by the caller.
        }

        /// <summary>
        /// Processes the extended properties of a Table mapped type.
        /// </summary>
        /// <param name="p">The PropertyInfo from the source object being processed.</param>
        protected override void ProcessExtended(PropertyInfo p)
        {
            base.ProcessExtended(p);
            ProcessPartitionKey(p);
            ProcessHierarchicalPartitionKey(p);
            ProcessIdField(p);
            ProcessSearchableField(p);

            //Lookup
            ProcessLookupHpk(p);
            ProcessLookupIdField(p);
        }

        /// <summary>
        /// Processes any Hierarchical PartitionKey mappings and configuration
        /// for the source property.
        /// </summary>
        /// <param name="p">The <see cref="PropertyInfo"/> of the source property.</param>
        /// <returns>True if the property was a PartitionKey, false otherwise.</returns>
        protected bool ProcessHierarchicalPartitionKey(PropertyInfo p)
        {
            var hierarchicalPartitionKey = p.GetCustomAttribute<HierarchicalPartitionKeyAttribute>();
            if (hierarchicalPartitionKey != null)
            {
                //Two fields cannot occupy the same position in the partition key.
                if (hierarchicalPartitionFields.ContainsKey(hierarchicalPartitionKey.Order))
                {
                    throw new DiiPartitionKeyDuplicateOrderException(hierarchicalPartitionFields[hierarchicalPartitionKey.Order].Name, p.Name, hierarchicalPartitionKey.Order);
                }

                hierarchicalPartitionFields.Add(hierarchicalPartitionKey.Order, p);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Processes any PartitionKey mappings and configuration
        /// for the source property.
        /// </summary>
        /// <param name="p">The <see cref="PropertyInfo"/> of the source property.</param>
        /// <returns>True if the property was a PartitionKey, false otherwise.</returns>
        protected bool ProcessPartitionKey(PropertyInfo p)
        {
            var partitionKey = p.GetCustomAttribute<PartitionKeyAttribute>();
            if (partitionKey != null)
            {
                //Two fields cannot occupy the same position in the partition key.
                if (partitionFields.ContainsKey(partitionKey.Order))
                {
                    throw new DiiPartitionKeyDuplicateOrderException(partitionFields[partitionKey.Order].Name, p.Name, partitionKey.Order);
                }

                partitionFields.Add(partitionKey.Order, p);

                //First PartitionKeyAttribute to change the separator wins.
                //Example: If a the first occurring property sets the separator to / 
                //and the second sets it to \ the first property that was encountered
                //and set the separator will permanently set the separator for the type.
                //So in the example, even though the separator sets twice, it will remain
                // '/'
                if (!char.IsWhiteSpace(partitionKey.Separator) && partitionSeparator == Constants.DefaultPartitionKeyDelimitor)
                {
                    partitionSeparator = partitionKey.Separator;
                }

                if (partitionKey.PartitionKeyType != null && partitionKey.PartitionKeyType != typeof(string))
                {
                    partitionKeyType = partitionKey.PartitionKeyType;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Process the mapped IdAttribute to the table type
        /// </summary>
        /// <param name="p">The <seealso cref="PropertyInfo"/> being processed.</param>
        /// <returns>True if the property was a IdField, false otherwise.</returns>
        /// <exception cref="DiiIdDuplicateOrderException"></exception>
        protected bool ProcessIdField(PropertyInfo p)
        {
            var id = p.GetCustomAttribute<IdAttribute>();
            if (id != null)
            {
                if (idFields.ContainsKey(id.Order))
                {
                    // Throw an exception when an invalid type is attempted.
                    throw new DiiIdDuplicateOrderException(idFields[id.Order].Name, p.Name, id.Order);
                }

                idFields.Add(id.Order, p);

                // First IdAttribute to change the separator wins.
                if (!char.IsWhiteSpace(id.Separator) && idSeparator == Constants.DefaultIdDelimitor)
                {
                    idSeparator = id.Separator;
                }

                return true;
            }
            return false;
        }

        protected bool ProcessLookupHpk(PropertyInfo p)
        {
            var LookupHpks = p.GetCustomAttributes<LookupHpkAttribute>();
            if (LookupHpks != null && LookupHpks.Any())
            {
                if (LookupHpkFields == null) LookupHpkFields = new Dictionary<string, Dictionary<int, PropertyInfo>>();

                foreach (var attr in LookupHpks)
                {
                    Dictionary<int, PropertyInfo> hpkProps = null;

                    if (!string.IsNullOrEmpty(attr.Group))
                    {
                        if (!LookupHpkFields.ContainsKey(attr.Group))
                        {
                            //new group
                            hpkProps = new Dictionary<int, PropertyInfo>();
                            LookupHpkFields.Add(attr.Group, hpkProps);
                        }
                        else
                            hpkProps = LookupHpkFields[attr.Group];
                    }
                    if (LookupHpkFields.Count() == 0)
                    {
                        hpkProps = new Dictionary<int, PropertyInfo>();
                        LookupHpkFields.Add(Constants.LookupDefaultGroupSuffix, hpkProps);
                    }

                    hpkProps = hpkProps ?? LookupHpkFields.Values.FirstOrDefault();

                    //Two fields cannot occupy the same position in the partition key.
                    if (hpkProps.ContainsKey(attr.Order))
                    {
                        throw new DiiPartitionKeyDuplicateOrderException(hpkProps[attr.Order].Name, p.Name, attr.Order);
                    }

                    hpkProps.Add(attr.Order, p);
                }
                return true;
            }
            return false;
        }

        protected bool ProcessLookupIdField(PropertyInfo p)
        {
            var id = p.GetCustomAttribute<LookupIdAttribute>();
            if (id != null)
            {
                Dictionary<int, PropertyInfo> idProps = null;

                if (LookupIdFields == null) LookupIdFields = new Dictionary<string, Dictionary<int, PropertyInfo>>();

                if (!string.IsNullOrEmpty(id.Group))
                {
                    if (!LookupIdFields.ContainsKey(id.Group))
                    {
                        //new group
                        idProps = new Dictionary<int, PropertyInfo>();
                        LookupIdFields.Add(id.Group, idProps);
                    }
                    else
                        idProps = LookupIdFields[id.Group];
                }
                if (LookupIdFields.Count() == 0)
                {
                    idProps = new Dictionary<int, PropertyInfo>();
                    LookupIdFields.Add(Constants.LookupDefaultGroupSuffix, idProps);
                }

                idProps = idProps ?? LookupIdFields.Values.FirstOrDefault();

                if (idProps.ContainsKey(id.Order))
                {
                    // Throw an exception when an invalid type is attempted.
                    throw new DiiIdDuplicateOrderException(idProps[id.Order].Name, p.Name, id.Order);
                }

                idProps.Add(id.Order, p);

                return true;
            }
            return false;
        }

        protected void ProcessSearchableField(PropertyInfo p)
        {
            var id = p.GetCustomAttribute<SearchableAttribute>();
            if (id != null)
            {
                if (SearchableFields == null)
                {
                    SearchableFields = new List<PropertyInfo>();
                }
                SearchableFields.Add(p);
            }
        }

        protected bool ProcessChangeTrackerField(PropertyInfo p)
        {
            if (p.Name.Equals(Constants.ReservedChangeTrackerKey, StringComparison.InvariantCultureIgnoreCase))
            {
                ChangeTrackerField = p;

                return true;
            }
            return false;
        }

        protected override void AppendExtendedFields()
        {
            base.AppendExtendedFields();

            //Append the necessary table fields for the PK and the ID
            if (this.hierarchicalPartitionFields?.Any() ?? false)
            {
                foreach (var field in hierarchicalPartitionFields.OrderBy(x => x.Key))
                {
                    _ = AddProperty(typeBuilder, field.Value.Name, field.Value.PropertyType);
                }
            }
            else
            {
                _ = AddProperty(typeBuilder, Constants.ReservedPartitionKeyKey, typeof(string));
            }

            if (this.idFields?.Any() ?? false)
            {
                foreach (var field in idFields.Values.Where(x => !this.workingPropertySet.ContainsKey(x.Name)).ToList())
                {
                    _ = AddProperty(typeBuilder, field.Name, field.PropertyType);
                };
            }
            _ = AddProperty(typeBuilder, Constants.ReservedIdKey, typeof(string));

            if (this.LookupHpkFields?.Any() ?? false)
            {
                var hpks = LookupHpkFields.Values.SelectMany(x => x.Values).ToList();
                foreach (var hpk in hpks.Distinct().Where(x => !this.workingPropertySet.ContainsKey(x.Name)).ToList())
                {
                    _ = AddProperty(typeBuilder, hpk.Name, hpk.PropertyType);
                }
            }
            if (this.LookupIdFields?.Any() ?? false)
            {
                var ids = LookupIdFields.Values.SelectMany(x => x.Values).ToList();
                foreach (var id in ids.Distinct().Where(x => !this.workingPropertySet.ContainsKey(x.Name)).ToList())
                {
                    _ = AddProperty(typeBuilder, id.Name, id.PropertyType);
                }
            }
        }

        protected override Serializer CustomizeSearchableSerializer(Dictionary<string, PropertyInfo> properties, Serializer serializer)
        {
            serializer = base.CustomizeSearchableSerializer(properties, serializer);

            var idInfo = properties[Constants.ReservedIdKey];
            properties.Remove(Constants.ReservedIdKey);
            serializer.Id = idInfo;

            if (idFields != null && idFields.Any())
            {
                serializer.IdProperties = idFields
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value)
                        .ToList();
                serializer.IdSeparator = idSeparator.ToString();
            }

            if (hierarchicalPartitionFields?.Any() ?? false)
            {
                serializer.HierarchicalPartitionKeyProperties = hierarchicalPartitionFields;
                serializer.PartitionKeyType = partitionKeyType;
            }

            //This handles Lookup container configuration
            if (LookupHpkFields?.Any() ?? false)
            {
                serializer.LookupHpkProperties = LookupHpkFields;
            }
            if (LookupIdFields?.Any() ?? false)
            {
                serializer.LookupIdProperties = LookupIdFields;
                serializer.diiChangeTrackerProperty = ChangeTrackerField;
            }
            if (SearchableFields?.Any() ?? false)
            {
                serializer.SearchableProperties = SearchableFields;
            }

            if (partitionFields != null && partitionFields.Any())
            {
                //Handle PK mapping for the stored type.
                var partitionKeyInfo = properties[Constants.ReservedPartitionKeyKey];
                serializer.PartitionKey = partitionKeyInfo;
                properties.Remove(Constants.ReservedPartitionKeyKey);

                serializer.PartitionKeyProperties = partitionFields
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value)
                        .ToList();
                serializer.PartitionKeySeparator = partitionSeparator.ToString();
                serializer.PartitionKeyType = partitionKeyType;
            }

            return serializer;
        }

    }
}
