using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos
{
    public class DiiCosmosBaseAdapter
    {
        public const int MAX_CONCURRENCY = 100;

        public StringBuilder GetSQLWithIdsAndHpks(List<string> ids, (string keycol1, List<string> keyValues) hpk1, (string keycol2, List<string> keyValues) hpk2, (string keycol3, List<string> keyValues) hpk3)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT * FROM c WHERE ");

            if (hpk1.keyValues != null && !string.IsNullOrEmpty(hpk1.keycol1))
            {
                stringBuilder.Append($"c.{hpk1.keycol1} IN ({string.Join(",", hpk1.keyValues.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }
            if (hpk2.keyValues != null && !string.IsNullOrEmpty(hpk2.keycol2))
            {
                stringBuilder.Append($"c.{hpk2.keycol2} IN ({string.Join(",", hpk2.keyValues.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }
            if (hpk3.keyValues != null && !string.IsNullOrEmpty(hpk3.keycol3))
            {
                stringBuilder.Append($"c.{hpk3.keycol3} IN ({string.Join(",", hpk3.keyValues.Distinct().Select(x => $"\"{x}\"").ToList())}) AND ");
            }

            stringBuilder.Append($"c.id IN ({string.Join(",", ids.Distinct().Select(x => $"\"{x}\"").ToList())})");

            return stringBuilder;

        }


        public async Task<List<Tout>> ProcessConcurrently<Tin, Tout>(IEnumerable<Tin> operations,
            Func<Tin, Task<Tout>> taskProducer,
            RequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var concurrentTasks = new List<Task<Tout>>();
            var itemResponses = new List<Tout>();

            foreach (Tin op in operations)
            {
                var task = taskProducer(op);
                concurrentTasks.Add(task);

                if (concurrentTasks.Count == MAX_CONCURRENCY)
                {
                    var res = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);
                    itemResponses.AddRange(res);
                    concurrentTasks.Clear();
                }
            }

            if (concurrentTasks.Count > 0)
            {
                var lastres = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);
                itemResponses.AddRange(lastres);
            }

            return itemResponses;
        }


    }
}
