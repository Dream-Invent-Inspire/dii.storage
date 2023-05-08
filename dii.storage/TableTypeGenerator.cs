using dii.storage.Attributes;
using dii.storage.Exceptions;
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
        protected Dictionary<int, PropertyInfo> idFields { get; set; } = new Dictionary<int, PropertyInfo>();

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
            ProcessIdField(p);
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

        protected override void AppendExtendedFields()
        {
            base.AppendExtendedFields();

            //Append the necessary table fields for the PK and the ID
            _ = AddProperty(typeBuilder, Constants.ReservedPartitionKeyKey, typeof(string));
            _ = AddProperty(typeBuilder, Constants.ReservedIdKey, typeof(string));
        }

        protected override Serializer CustomizeSearchableSerializer(Dictionary<string, PropertyInfo> properties, Serializer serializer)
        {
            serializer = base.CustomizeSearchableSerializer(properties, serializer);

            //Handle PK mapping for the stored type.
            var partitionKeyInfo = properties[Constants.ReservedPartitionKeyKey];
            serializer.PartitionKey = partitionKeyInfo;
            properties.Remove(Constants.ReservedPartitionKeyKey);


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

            if (partitionFields != null && partitionFields.Any())
            {
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
