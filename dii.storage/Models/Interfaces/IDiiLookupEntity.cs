using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Models.Interfaces
{
    public interface IDiiLookupEntity
    {
        DateTime LastUpdated { get; set; }
    }
}
