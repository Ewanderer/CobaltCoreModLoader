using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Components.Services
{
    public class LoopManagment
    {

        private static readonly Dictionary<string, LoopConfiguration> loop_configurations = new Dictionary<string, LoopConfiguration>() {
            { "default", new LoopConfiguration("default", new string[] {"all" }) },
            { "all", new LoopConfiguration("all", Array.Empty<string>()) },

        };

        /// <summary>
        /// The loop managment has the delicate task to collect configurations
        /// and patch itself into maps and other parts of the game to hook up map pools, events, enemies, etc based on the selected configuration.
        /// </summary>
        public LoopManagment()
        {

        }
    }
}
