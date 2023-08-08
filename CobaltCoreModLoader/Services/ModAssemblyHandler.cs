using CobaltCoreModding.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// A singleton to help store any assembly and its manifest.
    /// Can also run their bootup according to dependency.
    /// </summary>
    public class ModAssemblyHandler
    {
        private List<Tuple<Assembly, IModManifest?>> mod_lookup_list = new List<Tuple<Assembly, IModManifest?>>();

        public IEnumerable<Tuple<Assembly, IModManifest?>> ModLookup => mod_lookup_list;


    }
}
