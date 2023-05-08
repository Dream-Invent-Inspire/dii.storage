using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Exceptions
{
    internal class DiiDuplicateTypeInitializedException : Exception
    {
        public DiiDuplicateTypeInitializedException(Type type) 
            : base($"The type {type.FullName} has already been initialized.")
        { }
    }
}
