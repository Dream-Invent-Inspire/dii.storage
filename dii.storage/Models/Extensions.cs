using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Models
{
    public static class DictionaryExtensions
    {
        public static void TransferProperties(this Dictionary<int, PropertyInfo> dict, object source, ref object target, string idSeparator = null)
        {
            if (source == null || target == null)
            {
                throw new ArgumentNullException("Source or target cannot be null.");
            }

            var lookupProperties = source.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
            var lst = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value); //Ensure we go in order of the properties
            for (int i = 0; i < lst.Count; ++i)
            {
                if (!lst.ContainsKey(i))
                {
                    continue;
                }
                string propName = lst[i].Name;
                PropertyInfo sourcePropInfo = lst[i];

                if (sourcePropInfo.CanRead && lookupProperties.ContainsKey(propName))
                {
                    var value = lookupProperties[propName].GetValue(source);
                    if (value != null)
                    {
                        SetTargetValue(i, value, ref target, lst[i], idSeparator);
                    }
                    else
                    {
                        //wtf....this id property value is null in the dynamic lookup object
                    }
                }
            }
        }

        private static void SetTargetValue(int position, object value, ref object target, PropertyInfo propInfo, string idSeparator = null)
        {
            if (target == null)
            {
                return;
            }

            if (target is string)
            {
                if (!string.IsNullOrWhiteSpace((string)target)) target += $"{idSeparator ?? Constants.DefaultIdDelimitor.ToString()}";
                target += value.ToString();
            }
            else if (target is PartitionKeyBuilder)
            {
                ((PartitionKeyBuilder)target).Add(value.ToString());
            }
            else if (target is List<object>)
            {
                ((List<object>)target).Add(value);
            }
            else if (target is Dictionary<string, string>)
            {
                ((Dictionary<string, string>)target).Add(propInfo.Name, value.ToString());
            }
            else if (target is Dictionary<int, List<string>>)
            {
                if (!((Dictionary<int, List<string>>)target).ContainsKey(position))
                {
                    ((Dictionary<int, List<string>>)target).Add(position, new List<string>() { value.ToString() });
                }
                else
                {
                    ((Dictionary<int, List<string>>)target)[position].Add(value.ToString());
                }
            }
            else
            {
                propInfo.SetValue(target, value);
            }

        }
    }

}
