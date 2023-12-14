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
        /// <summary>
        /// THe name of htis configuration for referencing.
        /// </summary>
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

        /// <summary>
        /// set this if the configuration is intended for a certain cast.
        /// </summary>
        public IEnumerable<int> RequiredCastDeckIds { get; init; } = Array.Empty<int>();
        
        /// <summary>
        /// If RequiredCastDeckIds is used, this is the minimum number of matches required.
        /// </summary>
        public int MinimumRequiredCastCount = 0;
        
        /// <summary>
        /// If RequiredCastDeckIds is used, this is the maximum number of matches permitted.
        /// </summary>
        public int MaximumRequiredCastCount = 0;

        /// <summary>
        /// Limit which ships can be used for this config, by their name.
        /// </summary>
        public IEnumerable<string> PermittedShipNames { get; init; } = Array.Empty<string>();

        public LoopConfiguration(string globalName, IEnumerable<string> includeContentFrom)
        {
            GlobalName = globalName;
            IncludeContentFrom = includeContentFrom;
        }
    }
}
