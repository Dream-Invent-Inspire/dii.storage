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
}
