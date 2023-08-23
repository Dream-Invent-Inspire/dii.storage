using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dii.storage.cosmos.Models.Interfaces
{
    public interface ILookupAdapter
    {
        Task<dynamic> LookupAsync(object lookupType, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<dynamic> UpsertAsync(object lookupType, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
    }

}
