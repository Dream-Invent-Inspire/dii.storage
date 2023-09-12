using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using MessagePack;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Models
{
    public class DiiBasicEntity : IDiiEntity
    {
        public DiiBasicEntity()
        {
        }

        private Version _schemaVersion = new Version(1, 0);

        [IgnoreMember]
        public Version SchemaVersion
        {
            get { return _schemaVersion; }
            set { _schemaVersion = value; }
        }

        /// <inheritdoc/>
        [Searchable("_etag")]
        public string _etag { get; set; }

        [JsonIgnore]
        public string DataVersion
        {
            get
            {
                return _etag;
            }
            set
            {
                _etag = value;
            }
        }

        [Searchable("_ts")]
        public long _ts { get; set; }

        [JsonIgnore]
        public DateTime DiiTimestampUTC
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(_ts).UtcDateTime;
            }
            set
            {
                _ts = (long)value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        /// <summary>
        /// If this is a lookup entity, this will be populated with the initial state of the entity as soon as it is unpacked.
        /// </summary>
        [IgnoreMember]
        protected Dictionary<string, string> InitialState = new Dictionary<string, string>();

        [IgnoreMember]
        public bool HasInitialState
        {
            get
            {
                return (InitialState?.Any() ?? false);
            }
        }

        /// <summary>
        /// If this is a lookup entity, this will be populated with the current state of changes to the entity.
        /// </summary>
        public string DiiChangeTrackerString
        {
            get
            {
                return ((ChangeTracker?.Any() ?? false) ? JsonConvert.SerializeObject(ChangeTracker) : string.Empty);
            }
            set
            {
                ChangeTracker = ((!string.IsNullOrEmpty(value)) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(value) : new Dictionary<string, string>());
            }
        }

        public Dictionary<string, string> ChangeTracker = new Dictionary<string, string>();

        /// <summary>
        /// Called as soon as the entity is unpacked from the database.
        /// </summary>
        /// <param name="table"></param>
        public void SetInitialState(TableMetaData table)
        {
            SetInitialState(table, null);
            return;
        }

        public void SetInitialState(TableMetaData table, DiiBasicEntity source)
        {
            if (table.HasLookup())
            {
                InitialState = new Dictionary<string, string>();
                var entity = source ?? this;
                foreach (var hpk in table.LookupHpks)
                {
                    var hpkValue = entity.GetType().GetProperty(table.LookupHpks[hpk.Key].Name);
                    if (hpkValue != null)
                    {
                        var hpkValueString = hpkValue.GetValue(entity, null)?.ToString() ?? "";
                        //add to dictionary
                        InitialState.Add(hpkValue.Name, hpkValueString);
                    }
                }
                foreach (var lid in table.LookupIds)
                {
                    var idValue = entity.GetType().GetProperty(table.LookupIds[lid.Key].Name);
                    if (idValue != null)
                    {
                        var idValueString = idValue.GetValue(entity, null)?.ToString() ?? "";
                        //add to dictionary
                        InitialState.Add(idValue.Name, idValueString);
                    }
                }
            }
            return;

        }


        /// <summary>
        /// Called immediately prior to packing for persistence.
        /// </summary>
        /// <param name="table"></param>
        public void SetChangeTracker(TableMetaData table)
        {
            if (table.HasLookup())
            {
                ChangeTracker = new Dictionary<string, string>();
                foreach (var hpk in table.LookupHpks)
                {
                    var hpkValue = this.GetType().GetProperty(table.LookupHpks[hpk.Key].Name);
                    if (hpkValue != null && InitialState.ContainsKey(table.LookupHpks[hpk.Key].Name))
                    {
                        var hpkValueString = hpkValue.GetValue(this, null)?.ToString() ?? "";
                        var initialHpkValueString = InitialState[hpkValue.Name] ?? "";
                        if (!initialHpkValueString.Equals(hpkValueString))
                        {
                            //add to dictionary
                            ChangeTracker.Add(hpkValue.Name, initialHpkValueString);
                        }
                    }
                }
                foreach (var lid in table.LookupIds)
                {
                    var idValue = this.GetType().GetProperty(table.LookupIds[lid.Key].Name);
                    if (idValue != null && InitialState.ContainsKey(table.LookupHpks[lid.Key].Name))
                    {
                        var idValueString = idValue.GetValue(this, null)?.ToString() ?? "";
                        var initialIdValueString = InitialState[idValue.Name] ?? "";
                        if (!initialIdValueString.Equals(idValueString))
                        {
                            //add to dictionary
                            ChangeTracker.Add(idValue.Name, initialIdValueString); //this is the initial value that has been changed
                        }
                    }
                }
            }

            return;
        }

        public static string GetAnyKeyChangesSerialized(TableMetaData table, Dictionary<string, object> patchOperations)
        {
            var changes = GetAnyKeyChanges(table, patchOperations);
            return JsonConvert.SerializeObject(changes);
        }

        /// <summary>
        /// Picks up any changes to key values in the patch operations and returns them as a dictionary.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="patchOperations"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetAnyKeyChanges(TableMetaData table, Dictionary<string, object> patchOperations)
        {
            var changes = new Dictionary<string, string>();
            foreach (var op in patchOperations)
            {
                var lst = op.Key.Split('/') ?? new string[] { op.Key };
                var path = lst[lst.Count() - 1];
                foreach (var lid in table.LookupIds)
                {
                    var sat = lid.Value.GetCustomAttribute<SearchableAttribute>();
                    if (lid.Value.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        (sat?.Abbreviation?.Equals(path, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        changes.Add(sat?.Abbreviation ?? lid.Value.Name, op.Value.ToString());
                    }
                }
                foreach (var lhpk in table.LookupHpks)
                {
                    var sat = lhpk.Value.GetCustomAttribute<SearchableAttribute>();
                    if (lhpk.Value.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        (sat?.Abbreviation?.Equals(path, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        changes.Add(sat?.Abbreviation ?? lhpk.Value.Name, op.Value.ToString());
                    }
                }
                //Look for isDeleted indicator
                if (path.Equals(Constants.ReservedDeletedKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    changes.Add(Constants.ReservedDeletedKey, op.Value.ToString());
                }
            }
            return changes;
        }

    }


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
            for (int i=0; i < lst.Count; ++i)
            {
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
