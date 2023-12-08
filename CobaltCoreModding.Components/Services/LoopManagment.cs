using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Components.Services
{
    public class LoopManagment : ILoopConfigRegistry
    {

        private static readonly Dictionary<string, LoopConfiguration> loop_configurations = new Dictionary<string, LoopConfiguration>() {
            { "default", new LoopConfiguration("default", new string[] {"all" }) },
            { "all", new LoopConfiguration("all", Array.Empty<string>()) },

        };

        private static ILogger? Logger;


        /// <summary>
        /// The loop managment has the delicate task to collect configurations
        /// and patch itself into maps and other parts of the game to hook up map pools, events, enemies, etc based on the selected configuration.
        /// </summary>
        public LoopManagment(ILogger logger)
        {
            Logger = logger;
            ActiveLoopConfiguration = "default";
        }

        public Assembly CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly!;

        public ExternalDeck LookupDeck(string globalName)
        {
            return DeckRegistry.LookupDeck(globalName)!;
        }

        public LoopConfiguration LookupLoopConfig(string globalName)
        {
            if (!loop_configurations.TryGetValue(globalName, out var result))
                throw new Exception();
            return result;
        }

        public void RunLogic()
        {
            //Load all manifests

            //patch enemy function on each zone.

            //patch event pool

            //patch artifact pool

            //patch validate run function
        }

        private static LoopConfiguration? active_configuration;

        public static string ActiveLoopConfiguration
        {
            get => active_configuration!.GlobalName;

            set
            {
                if (!loop_configurations.TryGetValue(value, out var config))
                    Logger?.LogCritical("Attempted to set to unkown config {0}", value);
                else
                    active_configuration = config;
            }

        }

        public IManifest LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName)!;
        }

        public ExternalSprite LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName)!;
        }

        public bool RegisterLoopConfig(LoopConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.GlobalName))
            {
                return false;
            }

            if (!loop_configurations.TryAdd(configuration.GlobalName, configuration))
            {
                return false;
            }

            return true;
        }
    }
}
