using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace dii.storage.Attributes
{
    public abstract class DiiBaseAttribute : Attribute
    {
        public abstract CustomAttributeBuilder GetConstructorBuilder();
    }
}
