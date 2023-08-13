using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// Mods contain manifests, which the mod loader uses to have their data setup.
    /// this is the parent containing base defentions of manifest such as global name.
    /// </summary>
    public interface IManifest
    {
        /// <summary>
        /// The unique modifier of this manifest. must be unique within and across all assemblies for mods.
        /// </summary>
        string Name { get; }
    }
}
