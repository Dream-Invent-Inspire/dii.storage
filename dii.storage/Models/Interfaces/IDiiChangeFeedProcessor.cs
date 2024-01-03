using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.Models.Interfaces
{
    public interface IDiiChangeFeedProcessor
    {
        Task HandleCosmosChangesAsync(IReadOnlyCollection<JObject> changes, CancellationToken cancellationToken);
    }
}
