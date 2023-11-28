using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// Loop Configurations serve two purposes:
    /// 1) Allow mods to pool their ressources like enemies, maps, events, modifiers into pools from which the game can draw on.
    /// 2) Allows mods to create wholly new game modes, with set maps, events, enemies and other configurations. 
    /// The loader itself will always hold 2 configurations: "default" and "all". 
    /// the former being a patch for the regular run, the latter being a general lookup table for any other configuration looking for random content.
    /// </summary>
    public class LoopConfiguration
    {

        public string GlobalName { get; init; }

        /// <summary>
        /// The ammount of zones before a final boss.
        /// -1 for an endless mode (unless someone patches ending in on their own)
        /// </summary>
        public int LoopLength { get; init; } = 3;

        /// <summary>
        /// When searching for content, the global name of this config and any named here will be looked up in entries.
        /// </summary>
        public IEnumerable<string> IncludeContentFrom { get; init; }

        public LoopConfiguration(string globalName, IEnumerable<string> includeContentFrom)
        {
            GlobalName = globalName;
            IncludeContentFrom = includeContentFrom;
        }
    }
}
