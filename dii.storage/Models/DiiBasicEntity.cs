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

        public void SetInitialState(TableMetaData table, DiiBasicEntity source = null)
        {
            if (table.HasLookup())
            {
                InitialState = new Dictionary<string, string>();
                var entity = source ?? this;
                
                var hpks = table.LookupHpks.Values.SelectMany(x => x.Values).ToList();
                foreach (var hpk in hpks.Distinct())
                {
                    var hpkValue = entity.GetType().GetProperty(hpk.Name);
                    if (hpkValue != null)
                    {
                        var hpkValueString = hpkValue.GetValue(entity, null)?.ToString() ?? "";
                        //add to dictionary
                        InitialState.Add(hpkValue.Name, hpkValueString);
                    }
                }
                var ids = table.LookupIds.Values.SelectMany(x => x.Values).ToList();
                foreach (var lid in ids.Distinct())
                {
                    var idValue = entity.GetType().GetProperty(lid.Name);
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
                var hpks = table.LookupHpks.Values.SelectMany(x => x.Values).ToList();
                foreach (var hpk in hpks.Distinct())
                {
                    var hpkValue = this.GetType().GetProperty(hpk.Name);
                    if (hpkValue != null && InitialState.ContainsKey(hpk.Name))
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

                var ids = table.LookupIds.Values.SelectMany(x => x.Values).ToList();
                foreach (var lid in ids.Distinct())
                {
                    var idValue = this.GetType().GetProperty(lid.Name);
                    if (idValue != null && InitialState.ContainsKey(lid.Name))
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

    }
}
