using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface ICobaltCoreLookup
    {
        public Assembly CobaltCoreAssembly { get; }
    }
}