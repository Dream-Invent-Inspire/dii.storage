using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes the partition key that the Lookup entity belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LookupHpkAttribute : DiiBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionKeyAttribute"/> class with the
        /// order, separator and partition key type values with which the instance should be initalized.
        /// </summary>
        /// <param name="parititonKeyType">The underlying type of the partition key, if not a <see cref="string"/>.</param>
        /// <param name="order">The immutable order which the field or property should be used to form the composite partition key.</param>
        /// <param name="separator">The immutable separator to be used when forming the composite partition key.</param>
        public LookupHpkAttribute(Type parititonKeyType = null, int order = 0)
        {
            Order = order;
            PartitionKeyType = parititonKeyType ?? typeof(string);
        }

        /// <summary>
        /// Required when more than one field or property is designated as
        /// part of the partition key. The immutable order which the field or property should
        /// be used to form the composite partition key.
        /// <para>Defaults to 0</para>
        /// </summary>
        /// <remarks>
        /// Order is optional if only one <see cref="PartitionKeyAttribute"/> is used within the entity.
        /// </remarks>
        public int Order { get; init; }


        /// <summary>
        /// Optional parameter used to designate what the underlying <see cref="Type"/> of the
        /// partition key is, if not a <see cref="string"/>.
        /// <para>Defaults to <see cref="string"/></para>
        /// </summary>
        public Type PartitionKeyType { get; init; }

        public string Group { get; init; }

        public override CustomAttributeBuilder GetConstructorBuilder()
        {
            // Getting the right constructor
            var ctor = typeof(HierarchicalPartitionKeyAttribute).GetConstructor(new[] { typeof(Type), typeof(int), typeof(char) });

            // Creating the CustomAttributeBuilder with extracted values
            var attributeBuilder = new CustomAttributeBuilder(
                ctor,
                new object[] { this.PartitionKeyType, this.Order, '|' }
            );
            return attributeBuilder;
        }

    }

}
