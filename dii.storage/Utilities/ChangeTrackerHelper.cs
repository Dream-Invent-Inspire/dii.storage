using dii.storage.Attributes;
using dii.storage.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Utilities
{
    public class ChangeTrackerHelper
    {
        public string GetAnyKeyChangesSerialized(TableMetaData table, Dictionary<string, object> patchOperations)
        {
            var changes = GetAnyKeyChanges(table, patchOperations);
            return JsonConvert.SerializeObject(changes);
        }

        /// <summary>
        /// Picks up any changes to key/id values in the patch operations and returns them as a dictionary.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="patchOperations"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetAnyKeyChanges(TableMetaData table, Dictionary<string, object> patchOperations)
        {
            var changes = new Dictionary<string, string>();
            foreach (var op in patchOperations)
            {
                var lst = op.Key.Split('/') ?? new string[] { op.Key };
                var path = lst[lst.Count() - 1];

                var ids = table.LookupIds.Values.SelectMany(x => x.Values).ToList();
                foreach (var lid in ids)
                {
                    var sat = lid.GetCustomAttribute<SearchableAttribute>();
                    if (lid.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        (sat?.Abbreviation?.Equals(path, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        changes.Add(sat?.Abbreviation ?? lid.Name, op.Value.ToString());
                    }
                }

                var hpks = table.LookupHpks.Values.SelectMany(x => x.Values).ToList();
                foreach (var lhpk in hpks)
                {
                    var sat = lhpk.GetCustomAttribute<SearchableAttribute>();
                    if (lhpk.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase) ||
                        (sat?.Abbreviation?.Equals(path, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        changes.Add(sat?.Abbreviation ?? lhpk.Name, op.Value.ToString());
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
}
